using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArticulMaster
{
    public partial class Form1 : Form
    {
        private string currentVersion = "4.3.0.4"; // Підняв до 4.4 для тесту оновлення
        private HashSet<int> occupiedPrices = new HashSet<int>();

        private List<string> vendorsList = new List<string> {
            "[207] Pc.Lviv", "[212] eLaptop", "[33] PXL", "[37] Fortserg1",
            "[241] Gadgetusa", "[11] It-Technolodgy", "[213] IT-Lviv",
            "[233] LPStore", "[228] Ruslan111", "[224] SvChoice"
        };

        public Form1()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        // --- НОВИЙ МЕТОД ДЛЯ НАДІЙНОГО ЗБЕРЕЖЕННЯ ФАЙЛІВ ---
        private string GetFilePath(string code)
        {
            // Шлях до папки користувача: C:\Users\Ім'я\AppData\Local\ArticulMaster
            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArticulMaster");

            // Якщо папки ще немає (перший запуск) - створюємо її
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }

            return Path.Combine(appDataPath, $"{code}.txt");
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = $"Articul Master Pro v.{currentVersion}";
            comboVendors.Items.Clear();
            foreach (var v in vendorsList) comboVendors.Items.Add(v);

            comboVendors.SelectedIndexChanged += (s, ev) => LoadDatabaseForSelectedVendor();

            if (comboVendors.Items.Count > 0) comboVendors.SelectedIndex = 0;

            txtSearch.Text = "пошук";
            txtSearch.ForeColor = Color.Gray;
        }

        private void LoadDatabaseForSelectedVendor()
        {
            occupiedPrices.Clear();
            string code = GetVendorCode(comboVendors.Text);
            string filePath = GetFilePath(code); // Використовуємо новий шлях

            if (File.Exists(filePath))
            {
                string[] lines = File.ReadAllLines(filePath);
                foreach (string line in lines)
                {
                    if (int.TryParse(line.Trim(), out int p)) occupiedPrices.Add(p);
                }
            }
            UpdateCount();
        }

        private string GetVendorCode(string vendorStr)
        {
            Match match = Regex.Match(vendorStr, @"\[(\d+)\]");
            return match.Success ? match.Groups[1].Value : "000";
        }

        private void GenerateArticul()
        {
            if (string.IsNullOrWhiteSpace(txtPrice.Text)) return;

            if (int.TryParse(txtPrice.Text.Trim(), out int price))
            {
                string vendorCode = GetVendorCode(comboVendors.Text);
                string filePath = GetFilePath(vendorCode); // Використовуємо новий шлях

                while (occupiedPrices.Contains(price))
                {
                    price--;
                }

                occupiedPrices.Add(price);
                File.AppendAllText(filePath, price.ToString() + Environment.NewLine);

                txtResult.ForeColor = Color.SpringGreen;
                txtResult.Text = $"{price}_{vendorCode}";
                Clipboard.SetText(txtResult.Text);
                System.Media.SystemSounds.Beep.Play();

                txtPrice.Clear();
                UpdateCount();
            }
        }

        private void UpdateCount()
        {
            lblCount.Text = $"Артикулів у базі: {occupiedPrices.Count}";
        }

        private void txtPrice_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                GenerateArticul();
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            string code = GetVendorCode(comboVendors.Text);
            string filePath = GetFilePath(code); // Використовуємо новий шлях

            if (File.Exists(filePath))
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true };
                process.EnableRaisingEvents = true;
                process.Exited += (s, ev) =>
                {
                    this.Invoke(new Action(() => LoadDatabaseForSelectedVendor()));
                };
                process.Start();
            }
            else
            {
                MessageBox.Show(this, "Файл ще не створений. Додайте перший артикул!", "Редагування");
            }
        }

        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string vendorCode = GetVendorCode(comboVendors.Text);
                    string dbFilePath = GetFilePath(vendorCode); // Використовуємо новий шлях
                    string[] lines = File.ReadAllLines(openFile.FileName);
                    int added = 0;

                    foreach (var line in lines)
                    {
                        if (int.TryParse(line.Trim(), out int p))
                        {
                            if (occupiedPrices.Add(p))
                            {
                                File.AppendAllText(dbFilePath, p + Environment.NewLine);
                                added++;
                            }
                        }
                    }
                    UpdateCount();
                    MessageBox.Show(this, $"Імпорт завершено!\nДодано нових цін: {added}", "Імпорт");
                }
                catch (Exception ex) { MessageBox.Show(this, "Помилка імпорту: " + ex.Message); }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtSearch.Text.Trim(), out int searchPrice))
            {
                if (occupiedPrices.Contains(searchPrice))
                {
                    lblSearchStatus.ForeColor = Color.Red;
                    lblSearchStatus.Text = "ЗАЙНЯТО";
                    MessageBox.Show(this, $"Ціна {searchPrice} вже є в базі!", "Пошук");
                }
                else
                {
                    lblSearchStatus.ForeColor = Color.Lime;
                    lblSearchStatus.Text = "ВІЛЬНО";
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            string input = txtSearch.Text.Trim();
            if (int.TryParse(input, out int p) && occupiedPrices.Remove(p))
            {
                try
                {
                    string code = GetVendorCode(comboVendors.Text);
                    string filePath = GetFilePath(code); // Використовуємо новий шлях
                    File.WriteAllLines(filePath, occupiedPrices.Select(n => n.ToString()));
                    UpdateCount();

                    lblSearchStatus.ForeColor = Color.Orange;
                    lblSearchStatus.Text = $"DEL: {p}";

                    txtSearch.Clear();
                    System.Media.SystemSounds.Exclamation.Play();
                }
                catch (Exception ex) { MessageBox.Show(this, "Помилка: " + ex.Message); }
            }
        }

        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "пошук")
            {
                txtSearch.Text = "";
                txtSearch.ForeColor = Color.Black;
            }
        }

        private void txtSearch_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearch.Text))
            {
                txtSearch.Text = "пошук";
                txtSearch.ForeColor = Color.Gray;
            }
        }
    }
}