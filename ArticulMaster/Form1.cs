using System.ComponentModel;
using System.Diagnostics;
using System.Media;
using System.Reflection;
using System.Text.RegularExpressions;

namespace ArticulMaster;

public class Form1 : Form
{
    private string currentVersion = "";
    private HashSet<int> occupiedPrices = new HashSet<int>();
    private List<string> vendorsList = new List<string> {
        "[207] Pc.Lviv", "[212] eLaptop", "[33] PXL", "[37] Fortserg1",
        "[241] Gadgetusa", "[11] It-Technolodgy", "[213] IT-Lviv",
        "[233] LPStore", "[228] Ruslan111", "[224] SvChoice"
    };

    private IContainer components = null;
    private Label lblTitle;
    private ComboBox comboVendors;
    private TextBox txtPrice;
    private Label lblCount;
    private Button btnEdit;
    private Button btnImport;
    private TextBox txtSearch;
    private Button btnSearch;
    private Button btnDelete;
    private Label lblSearchStatus;
    private Button btnGenerate; // Одна змінна для кнопки
    private TextBox textBox1;
    private TextBox textBox2;
    private TextBox txtResult;

    public Form1()
    {
        InitializeComponent();
        this.StartPosition = FormStartPosition.CenterScreen;

        // Викликаємо метод отримання версії
        SetCurrentVersion();

        // Прив'язка події кліку до кнопки
        btnGenerate.Click += (s, e) => GenerateArticul();
    }

    private void SetCurrentVersion()
    {
        Version version = Assembly.GetExecutingAssembly().GetName().Version;
        if (version != null)
        {
            // Використовуємо формат Major.Minor.Build.Revision для повної автоматизації
            currentVersion = $"{version.Major}.{version.Minor}.{version.Build}.{version.Revision}";
        }
        else
        {
            currentVersion = "1.0.0.0";
        }
    }

    private string GetFilePath(string code)
    {
        string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ArticulMaster");
        if (!Directory.Exists(appDataPath))
        {
            Directory.CreateDirectory(appDataPath);
        }
        return Path.Combine(appDataPath, code + ".txt");
    }

    private void Form1_Load(object sender, EventArgs e)
    {
        this.Text = "Articul Master Pro v." + currentVersion;

        comboVendors.Items.Clear();
        foreach (string v in vendorsList)
        {
            comboVendors.Items.Add(v);
        }

        comboVendors.SelectedIndexChanged += (s, ev) => LoadDatabaseForSelectedVendor();

        if (comboVendors.Items.Count > 0)
        {
            comboVendors.SelectedIndex = 0;
        }

        txtSearch.Text = "пошук";
        txtSearch.ForeColor = Color.Gray;
    }

    private void LoadDatabaseForSelectedVendor()
    {
        occupiedPrices.Clear();
        string code = GetVendorCode(comboVendors.Text);
        string filePath = GetFilePath(code);
        if (File.Exists(filePath))
        {
            string[] lines = File.ReadAllLines(filePath);
            foreach (string line in lines)
            {
                if (int.TryParse(line.Trim(), out var p))
                {
                    occupiedPrices.Add(p);
                }
            }
        }
        UpdateCount();
    }

    private string GetVendorCode(string vendorStr)
    {
        Match match = Regex.Match(vendorStr, "\\[(\\d+)\\]");
        return match.Success ? match.Groups[1].Value : "000";
    }

    private void GenerateArticul()
    {
        string input = txtPrice.Text?.Trim();
        if (string.IsNullOrWhiteSpace(input) || !int.TryParse(input, out var price))
        {
            return;
        }

        string vendorCode = GetVendorCode(comboVendors.Text);
        string filePath = GetFilePath(vendorCode);
        int originalPrice = price;

        // Пошук вільної ціни вниз
        while (occupiedPrices.Contains(price))
        {
            price--;
        }

        try
        {
            string directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            occupiedPrices.Add(price);
            File.AppendAllText(filePath, price + Environment.NewLine);

            // Якщо ціна змінилася — підсвічуємо золотим, якщо ні — весняним зеленим
            txtResult.ForeColor = (price == originalPrice) ? Color.SpringGreen : Color.Gold;
            txtResult.Text = $"{price}_{vendorCode}";

            Clipboard.SetText(txtResult.Text);
            SystemSounds.Beep.Play();

            txtPrice.Clear();
            UpdateCount();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Помилка: {ex.Message}", "Помилка файлу");
        }
    }

    private void UpdateCount()
    {
        lblCount.Text = $"Артикулів у базі: {occupiedPrices.Count}";
    }

    private void txtPrice_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.Return)
        {
            e.SuppressKeyPress = true;
            GenerateArticul();
        }
    }

    private void btnEdit_Click(object sender, EventArgs e)
    {
        string code = GetVendorCode(comboVendors.Text);
        string filePath = GetFilePath(code);
        if (File.Exists(filePath))
        {
            ProcessStartInfo psi = new ProcessStartInfo(filePath) { UseShellExecute = true };
            Process p = Process.Start(psi);
            if (p != null)
            {
                p.EnableRaisingEvents = true;
                p.Exited += (s, ev) => this.Invoke(() => LoadDatabaseForSelectedVendor());
            }
        }
        else
        {
            MessageBox.Show("Файл ще не створений.", "Редагування");
        }
    }

    private void btnImport_Click(object sender, EventArgs e)
    {
        OpenFileDialog openFile = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
        if (openFile.ShowDialog() != DialogResult.OK) return;

        try
        {
            string vendorCode = GetVendorCode(comboVendors.Text);
            string dbFilePath = GetFilePath(vendorCode);
            string[] lines = File.ReadAllLines(openFile.FileName);
            int added = 0;

            foreach (string line in lines)
            {
                if (int.TryParse(line.Trim(), out var p) && occupiedPrices.Add(p))
                {
                    File.AppendAllText(dbFilePath, p + Environment.NewLine);
                    added++;
                }
            }
            UpdateCount();
            MessageBox.Show($"Додано нових цін: {added}", "Імпорт");
        }
        catch (Exception ex) { MessageBox.Show("Помилка імпорту: " + ex.Message); }
    }

    private void btnSearch_Click(object sender, EventArgs e)
    {
        if (int.TryParse(txtSearch.Text.Trim(), out var searchPrice))
        {
            if (occupiedPrices.Contains(searchPrice))
            {
                lblSearchStatus.ForeColor = Color.Red;
                lblSearchStatus.Text = "ЗАЙНЯТО";
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
        if (int.TryParse(txtSearch.Text.Trim(), out var p) && occupiedPrices.Remove(p))
        {
            try
            {
                string code = GetVendorCode(comboVendors.Text);
                File.WriteAllLines(GetFilePath(code), occupiedPrices.Select(n => n.ToString()));
                UpdateCount();
                lblSearchStatus.ForeColor = Color.Orange;
                lblSearchStatus.Text = $"DEL: {p}";
                txtSearch.Clear();
                SystemSounds.Exclamation.Play();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
        }
    }

    private void txtSearch_Enter(object sender, EventArgs e)
    {
        if (txtSearch.Text == "пошук") { txtSearch.Text = ""; txtSearch.ForeColor = Color.Black; }
    }

    private void txtSearch_Leave(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(txtSearch.Text)) { txtSearch.Text = "пошук"; txtSearch.ForeColor = Color.Gray; }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing && components != null) components.Dispose();
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        ComponentResourceManager resources = new ComponentResourceManager(typeof(Form1));
        lblTitle = new Label();
        comboVendors = new ComboBox();
        txtPrice = new TextBox();
        lblCount = new Label();
        btnEdit = new Button();
        btnImport = new Button();
        txtSearch = new TextBox();
        btnSearch = new Button();
        btnDelete = new Button();
        lblSearchStatus = new Label();
        txtResult = new TextBox();
        btnGenerate = new Button();
        textBox1 = new TextBox();
        textBox2 = new TextBox();
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.Location = new Point(105, 27);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(213, 30);
        lblTitle.TabIndex = 11;
        lblTitle.Text = "Articul Master PRO";
        // 
        // comboVendors
        // 
        comboVendors.DropDownStyle = ComboBoxStyle.DropDownList;
        comboVendors.Location = new Point(116, 70);
        comboVendors.Name = "comboVendors";
        comboVendors.Size = new Size(190, 23);
        comboVendors.TabIndex = 10;
        // 
        // txtPrice
        // 
        txtPrice.BorderStyle = BorderStyle.FixedSingle;
        txtPrice.Location = new Point(161, 228);
        txtPrice.Name = "txtPrice";
        txtPrice.Size = new Size(100, 23);
        txtPrice.TabIndex = 9;
        txtPrice.TextAlign = HorizontalAlignment.Center;
        txtPrice.KeyDown += txtPrice_KeyDown;
        // 
        // lblCount
        // 
        lblCount.AutoSize = true;
        lblCount.Font = new Font("Segoe UI", 12F);
        lblCount.Location = new Point(141, 96);
        lblCount.Name = "lblCount";
        lblCount.Size = new Size(141, 21);
        lblCount.TabIndex = 8;
        lblCount.Text = "Артикулів у базі: 0";
        // 
        // btnEdit
        // 
        btnEdit.Font = new Font("Segoe UI", 8F);
        btnEdit.Location = new Point(312, 70);
        btnEdit.Name = "btnEdit";
        btnEdit.Size = new Size(74, 23);
        btnEdit.TabIndex = 7;
        btnEdit.Text = "Редагувати";
        btnEdit.Click += btnEdit_Click;
        // 
        // btnImport
        // 
        btnImport.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
        btnImport.Location = new Point(174, 477);
        btnImport.Name = "btnImport";
        btnImport.Size = new Size(75, 23);
        btnImport.TabIndex = 6;
        btnImport.Text = "Import";
        btnImport.Click += btnImport_Click;
        // 
        // txtSearch
        // 
        txtSearch.BorderStyle = BorderStyle.FixedSingle;
        txtSearch.Location = new Point(161, 136);
        txtSearch.Name = "txtSearch";
        txtSearch.Size = new Size(100, 23);
        txtSearch.TabIndex = 5;
        txtSearch.TextAlign = HorizontalAlignment.Center;
        txtSearch.Enter += txtSearch_Enter;
        txtSearch.Leave += txtSearch_Leave;
        // 
        // btnSearch
        // 
        btnSearch.Location = new Point(275, 135);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(55, 23);
        btnSearch.TabIndex = 4;
        btnSearch.Text = "пошук";
        btnSearch.Click += btnSearch_Click;
        // 
        // btnDelete
        // 
        btnDelete.Location = new Point(345, 135);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(41, 23);
        btnDelete.TabIndex = 3;
        btnDelete.Text = "Del";
        btnDelete.Click += btnDelete_Click;
        // 
        // lblSearchStatus
        // 
        lblSearchStatus.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold);
        lblSearchStatus.Location = new Point(132, 179);
        lblSearchStatus.Name = "lblSearchStatus";
        lblSearchStatus.Size = new Size(158, 31);
        lblSearchStatus.TabIndex = 2;
        lblSearchStatus.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // txtResult
        // 
        txtResult.BackColor = SystemColors.GradientActiveCaption;
        txtResult.BorderStyle = BorderStyle.None;
        txtResult.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
        txtResult.ForeColor = Color.Chartreuse;
        txtResult.Location = new Point(132, 276);
        txtResult.Name = "txtResult";
        txtResult.ReadOnly = true;
        txtResult.Size = new Size(158, 32);
        txtResult.TabIndex = 1;
        txtResult.TextAlign = HorizontalAlignment.Center;
        // 
        // btnGenerate
        // 
        btnGenerate.Location = new Point(296, 228);
        btnGenerate.Name = "btnGenerate";
        btnGenerate.Size = new Size(90, 23);
        btnGenerate.TabIndex = 0;
        btnGenerate.Text = "Генерувати";
        // 
        // textBox1
        // 
        textBox1.BackColor = SystemColors.GradientActiveCaption;
        textBox1.BorderStyle = BorderStyle.None;
        textBox1.Font = new Font("Segoe UI", 12F);
        textBox1.Location = new Point(25, 229);
        textBox1.Name = "textBox1";
        textBox1.Size = new Size(120, 22);
        textBox1.TabIndex = 12;
        textBox1.Text = "Введіть артикул";
        // 
        // textBox2
        // 
        textBox2.BackColor = SystemColors.GradientActiveCaption;
        textBox2.BorderStyle = BorderStyle.None;
        textBox2.Font = new Font("Segoe UI", 12F);
        textBox2.Location = new Point(25, 136);
        textBox2.Name = "textBox2";
        textBox2.Size = new Size(120, 22);
        textBox2.TabIndex = 13;
        textBox2.Text = "Пошук Артикула";
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = SystemColors.GradientActiveCaption;
        ClientSize = new Size(422, 523);
        Controls.Add(textBox2);
        Controls.Add(textBox1);
        Controls.Add(btnGenerate);
        Controls.Add(txtResult);
        Controls.Add(lblSearchStatus);
        Controls.Add(btnDelete);
        Controls.Add(btnSearch);
        Controls.Add(txtSearch);
        Controls.Add(btnImport);
        Controls.Add(btnEdit);
        Controls.Add(lblCount);
        Controls.Add(txtPrice);
        Controls.Add(comboVendors);
        Controls.Add(lblTitle);
        ForeColor = Color.DarkBlue;
        Icon = (Icon)resources.GetObject("$this.Icon");
        Name = "Form1";
        Text = "Articul Master Pro";
        Load += Form1_Load;
        ResumeLayout(false);
        PerformLayout();
    }
}