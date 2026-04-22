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
        // Пам'ять для цін поточного вендора
        private HashSet<int> occupiedPrices = new HashSet<int>();

        // Список вендорів (можна додавати нових тут)
        private List<string> vendorsList = new List<string> {
            "[207] Pc.Lviv", "[212] eLaptop", "[33] PXL", "[37] Fortserg1",
            "[241] Gadgetusa", "[11] It-Technolodgy", "[213] IT-Lviv",
            "[233] LPStore", "[228] Ruslan111", "[224] SvChoice"
        };

        public Form1()
        {
            InitializeComponent();
            // Налаштування, щоб програма при старті була по центру екрана
            this.StartPosition = FormStartPosition.CenterScreen;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            // Ініціалізація випадаючого списку
            comboVendors.Items.Clear();
            foreach (var v in vendorsList) comboVendors.Items.Add(v);

            // Подія зміни вендора — завантажуємо відповідну базу
            comboVendors.SelectedIndexChanged += (s, ev) => LoadDatabaseForSelectedVendor();

            if (comboVendors.Items.Count > 0) comboVendors.SelectedIndex = 0;

            // Початковий стан пошуку (сірий плейсхолдер)
            txtSearch.Text = "пошук";
            txtSearch.ForeColor = Color.Gray;
        }

        private void LoadDatabaseForSelectedVendor()
        {
            occupiedPrices.Clear();
            string code = GetVendorCode(comboVendors.Text);
            string fileName = $"{code}.txt";

            if (File.Exists(fileName))
            {
                string[] lines = File.ReadAllLines(fileName);
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
                string fileName = $"{vendorCode}.txt";

                // Логіка унікальності ціни (зменшуємо на 1, поки не знайдемо вільну)
                while (occupiedPrices.Contains(price))
                {
                    price--;
                }

                occupiedPrices.Add(price);
                File.AppendAllText(fileName, price.ToString() + Environment.NewLine);

                // Вивід результату
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

        // --- КНОПКА РЕДАГУВАННЯ (Блокнот) ---
        private void btnEdit_Click(object sender, EventArgs e)
        {
            string code = GetVendorCode(comboVendors.Text);
            string fileName = $"{code}.txt";

            if (File.Exists(fileName))
            {
                var process = new System.Diagnostics.Process();
                process.StartInfo = new System.Diagnostics.ProcessStartInfo(fileName) { UseShellExecute = true };
                process.EnableRaisingEvents = true;
                process.Exited += (s, ev) =>
                {
                    this.Invoke(new Action(() => LoadDatabaseForSelectedVendor()));
                };
                process.Start();
            }
            else
            {
                MessageBox.Show(this, $"Файл {fileName} ще не створений.", "Редагування");
            }
        }

        // --- КНОПКА ІМПОРТУ ---
        private void btnImport_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFile = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
            if (openFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    string vendorCode = GetVendorCode(comboVendors.Text);
                    string dbFile = $"{vendorCode}.txt";
                    string[] lines = File.ReadAllLines(openFile.FileName);
                    int added = 0;

                    foreach (var line in lines)
                    {
                        if (int.TryParse(line.Trim(), out int p))
                        {
                            if (occupiedPrices.Add(p))
                            {
                                File.AppendAllText(dbFile, p + Environment.NewLine);
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

        // --- КНОПКА ПОШУКУ ---
        private void btnSearch_Click(object sender, EventArgs e)
        {
            if (int.TryParse(txtSearch.Text.Trim(), out int searchPrice))
            {
                if (occupiedPrices.Contains(searchPrice))
                {
                    lblSearchStatus.ForeColor = Color.Red; // Виводимо в НОВИЙ label
                    lblSearchStatus.Text = "ЗАЙНЯТО";
                    MessageBox.Show(this, $"Ціна {searchPrice} вже є в базі!", "Пошук");
                }
                else
                {
                    lblSearchStatus.ForeColor = Color.Lime; // Виводимо в НОВИЙ label
                    lblSearchStatus.Text = "ВІЛЬНО";
                }
            }
        }

        // --- КНОПКА ВИДАЛЕННЯ ---
        private void btnDelete_Click(object sender, EventArgs e)
        {
            string input = txtSearch.Text.Trim();
            if (int.TryParse(input, out int p) && occupiedPrices.Remove(p))
            {
                try
                {
                    string code = GetVendorCode(comboVendors.Text);
                    File.WriteAllLines($"{code}.txt", occupiedPrices.Select(n => n.ToString()));
                    UpdateCount();

                    lblSearchStatus.ForeColor = Color.Orange; // Виводимо в НОВИЙ label
                    lblSearchStatus.Text = $"DEL: {p}";

                    txtSearch.Clear();
                    System.Media.SystemSounds.Exclamation.Play();
                }
                catch (Exception ex) { MessageBox.Show(this, "Помилка: " + ex.Message); }
            }
        }

        // --- ЛОГІКА ПЛЕЙСХОЛДЕРА (Слово "пошук") ---
        private void txtSearch_Enter(object sender, EventArgs e)
        {
            if (txtSearch.Text == "пошук")
            {
                txtSearch.Text = "";
                // Тут встановлюємо колір тексту, який ти будеш вводити
                // Якщо фон темний — ставимо White, якщо світлий — Black
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