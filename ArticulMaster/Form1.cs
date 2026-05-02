using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Media;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace ArticulMaster;

public class Form1 : Form
{
	private string currentVersion = "4.3.0.4";

	private HashSet<int> occupiedPrices = new HashSet<int>();

	private List<string> vendorsList = new List<string> { "[207] Pc.Lviv", "[212] eLaptop", "[33] PXL", "[37] Fortserg1", "[241] Gadgetusa", "[11] It-Technolodgy", "[213] IT-Lviv", "[233] LPStore", "[228] Ruslan111", "[224] SvChoice" };

	private IContainer components;

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

	private TextBox txtResult;

	public Form1()
	{
		InitializeComponent();
		base.StartPosition = FormStartPosition.CenterScreen;
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
		Text = "Articul Master Pro v." + currentVersion;
		comboVendors.Items.Clear();
		foreach (string v in vendorsList)
		{
			comboVendors.Items.Add(v);
		}
		comboVendors.SelectedIndexChanged += delegate
		{
			LoadDatabaseForSelectedVendor();
		};
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
			string[] array = File.ReadAllLines(filePath);
			for (int i = 0; i < array.Length; i++)
			{
				if (int.TryParse(array[i].Trim(), out var p))
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
		if (!match.Success)
		{
			return "000";
		}
		return match.Groups[1].Value;
	}

	private void GenerateArticul()
	{
		if (!string.IsNullOrWhiteSpace(txtPrice.Text) && int.TryParse(txtPrice.Text.Trim(), out var price))
		{
			string vendorCode = GetVendorCode(comboVendors.Text);
			string filePath = GetFilePath(vendorCode);
			while (occupiedPrices.Contains(price))
			{
				price--;
			}
			occupiedPrices.Add(price);
			File.AppendAllText(filePath, price + Environment.NewLine);
			txtResult.ForeColor = Color.SpringGreen;
			txtResult.Text = $"{price}_{vendorCode}";
			Clipboard.SetText(txtResult.Text);
			SystemSounds.Beep.Play();
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
			Process process = new Process();
			process.StartInfo = new ProcessStartInfo(filePath)
			{
				UseShellExecute = true
			};
			process.EnableRaisingEvents = true;
			process.Exited += delegate
			{
				Invoke(delegate
				{
					LoadDatabaseForSelectedVendor();
				});
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
		OpenFileDialog openFile = new OpenFileDialog
		{
			Filter = "Text Files (*.txt)|*.txt"
		};
		if (openFile.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			string vendorCode = GetVendorCode(comboVendors.Text);
			string dbFilePath = GetFilePath(vendorCode);
			string[] array = File.ReadAllLines(openFile.FileName);
			int added = 0;
			string[] array2 = array;
			for (int i = 0; i < array2.Length; i++)
			{
				if (int.TryParse(array2[i].Trim(), out var p) && occupiedPrices.Add(p))
				{
					File.AppendAllText(dbFilePath, p + Environment.NewLine);
					added++;
				}
			}
			UpdateCount();
			MessageBox.Show(this, $"Імпорт завершено!\nДодано нових цін: {added}", "Імпорт");
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Помилка імпорту: " + ex.Message);
		}
	}

	private void btnSearch_Click(object sender, EventArgs e)
	{
		if (int.TryParse(txtSearch.Text.Trim(), out var searchPrice))
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
		if (!int.TryParse(txtSearch.Text.Trim(), out var p) || !occupiedPrices.Remove(p))
		{
			return;
		}
		try
		{
			string code = GetVendorCode(comboVendors.Text);
			File.WriteAllLines(GetFilePath(code), occupiedPrices.Select((int n) => n.ToString()));
			UpdateCount();
			lblSearchStatus.ForeColor = Color.Orange;
			lblSearchStatus.Text = $"DEL: {p}";
			txtSearch.Clear();
			SystemSounds.Exclamation.Play();
		}
		catch (Exception ex)
		{
			MessageBox.Show(this, "Помилка: " + ex.Message);
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

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
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
        SuspendLayout();
        // 
        // lblTitle
        // 
        lblTitle.AutoSize = true;
        lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
        lblTitle.Location = new Point(43, 27);
        lblTitle.Name = "lblTitle";
        lblTitle.Size = new Size(213, 30);
        lblTitle.TabIndex = 0;
        lblTitle.Text = "Articul Master PRO";
        // 
        // comboVendors
        // 
        comboVendors.DropDownStyle = ComboBoxStyle.DropDownList;
        comboVendors.FormattingEnabled = true;
        comboVendors.Location = new Point(49, 70);
        comboVendors.Name = "comboVendors";
        comboVendors.Size = new Size(190, 23);
        comboVendors.TabIndex = 1;
        // 
        // txtPrice
        // 
        txtPrice.BackColor = SystemColors.Window;
        txtPrice.BorderStyle = BorderStyle.FixedSingle;
        txtPrice.Location = new Point(99, 228);
        txtPrice.Name = "txtPrice";
        txtPrice.Size = new Size(100, 23);
        txtPrice.TabIndex = 2;
        txtPrice.TextAlign = HorizontalAlignment.Center;
        txtPrice.KeyDown += txtPrice_KeyDown;
        // 
        // lblCount
        // 
        lblCount.AutoSize = true;
        lblCount.Location = new Point(96, 106);
        lblCount.Name = "lblCount";
        lblCount.Size = new Size(107, 15);
        lblCount.TabIndex = 4;
        lblCount.Text = "Артикулів у базі: 0";
        // 
        // btnEdit
        // 
        btnEdit.Location = new Point(245, 70);
        btnEdit.Name = "btnEdit";
        btnEdit.Size = new Size(41, 23);
        btnEdit.TabIndex = 5;
        btnEdit.Text = "Edit";
        btnEdit.UseVisualStyleBackColor = true;
        btnEdit.Click += btnEdit_Click;
        // 
        // btnImport
        // 
        btnImport.Font = new Font("Segoe UI", 9F, FontStyle.Bold, GraphicsUnit.Point, 204);
        btnImport.Location = new Point(112, 477);
        btnImport.Name = "btnImport";
        btnImport.Size = new Size(75, 23);
        btnImport.TabIndex = 6;
        btnImport.Text = "Import";
        btnImport.UseVisualStyleBackColor = true;
        btnImport.Click += btnImport_Click;
        // 
        // txtSearch
        // 
        txtSearch.ForeColor = Color.Gray;
        txtSearch.Location = new Point(49, 135);
        txtSearch.Name = "txtSearch";
        txtSearch.Size = new Size(100, 23);
        txtSearch.TabIndex = 7;
        txtSearch.Text = "Пошук";
        txtSearch.TextAlign = HorizontalAlignment.Center;
        txtSearch.Enter += txtSearch_Enter;
        txtSearch.Leave += txtSearch_Leave;
        // 
        // btnSearch
        // 
        btnSearch.Location = new Point(155, 134);
        btnSearch.Name = "btnSearch";
        btnSearch.Size = new Size(55, 23);
        btnSearch.TabIndex = 8;
        btnSearch.Text = "🔍";
        btnSearch.UseVisualStyleBackColor = true;
        btnSearch.Click += btnSearch_Click;
        // 
        // btnDelete
        // 
        btnDelete.Location = new Point(216, 135);
        btnDelete.Name = "btnDelete";
        btnDelete.Size = new Size(50, 23);
        btnDelete.TabIndex = 9;
        btnDelete.Text = "Del";
        btnDelete.UseVisualStyleBackColor = true;
        btnDelete.Click += btnDelete_Click;
        // 
        // lblSearchStatus
        // 
        lblSearchStatus.Font = new Font("Segoe UI", 15.75F, FontStyle.Bold, GraphicsUnit.Point, 204);
        lblSearchStatus.Location = new Point(70, 179);
        lblSearchStatus.Name = "lblSearchStatus";
        lblSearchStatus.Size = new Size(158, 31);
        lblSearchStatus.TabIndex = 10;
        lblSearchStatus.TextAlign = ContentAlignment.MiddleCenter;
        // 
        // txtResult
        // 
        txtResult.BackColor = Color.LightSlateGray;
        txtResult.BorderStyle = BorderStyle.None;
        txtResult.Cursor = Cursors.IBeam;
        txtResult.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point, 204);
        txtResult.ForeColor = Color.GreenYellow;
        txtResult.Location = new Point(70, 276);
        txtResult.Name = "txtResult";
        txtResult.ReadOnly = true;
        txtResult.Size = new Size(158, 32);
        txtResult.TabIndex = 11;
        txtResult.TextAlign = HorizontalAlignment.Center;
        // 
        // Form1
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.LightSlateGray;
        ClientSize = new Size(298, 523);
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
