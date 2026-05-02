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
		System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ArticulMaster.Form1));
		this.lblTitle = new System.Windows.Forms.Label();
		this.comboVendors = new System.Windows.Forms.ComboBox();
		this.txtPrice = new System.Windows.Forms.TextBox();
		this.lblCount = new System.Windows.Forms.Label();
		this.btnEdit = new System.Windows.Forms.Button();
		this.btnImport = new System.Windows.Forms.Button();
		this.txtSearch = new System.Windows.Forms.TextBox();
		this.btnSearch = new System.Windows.Forms.Button();
		this.btnDelete = new System.Windows.Forms.Button();
		this.lblSearchStatus = new System.Windows.Forms.Label();
		this.txtResult = new System.Windows.Forms.TextBox();
		base.SuspendLayout();
		this.lblTitle.AutoSize = true;
		this.lblTitle.Font = new System.Drawing.Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold);
		this.lblTitle.Location = new System.Drawing.Point(26, 27);
		this.lblTitle.Name = "lblTitle";
		this.lblTitle.Size = new System.Drawing.Size(247, 30);
		this.lblTitle.TabIndex = 0;
		this.lblTitle.Text = "ARTICUL MASTER PRO";
		this.comboVendors.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboVendors.FormattingEnabled = true;
		this.comboVendors.Location = new System.Drawing.Point(49, 70);
		this.comboVendors.Name = "comboVendors";
		this.comboVendors.Size = new System.Drawing.Size(190, 23);
		this.comboVendors.TabIndex = 1;
		this.txtPrice.BackColor = System.Drawing.SystemColors.Window;
		this.txtPrice.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
		this.txtPrice.Location = new System.Drawing.Point(99, 228);
		this.txtPrice.Name = "txtPrice";
		this.txtPrice.Size = new System.Drawing.Size(100, 23);
		this.txtPrice.TabIndex = 2;
		this.txtPrice.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.txtPrice.KeyDown += new System.Windows.Forms.KeyEventHandler(txtPrice_KeyDown);
		this.lblCount.AutoSize = true;
		this.lblCount.Location = new System.Drawing.Point(96, 106);
		this.lblCount.Name = "lblCount";
		this.lblCount.Size = new System.Drawing.Size(107, 15);
		this.lblCount.TabIndex = 4;
		this.lblCount.Text = "Артикулів у базі: 0";
		this.btnEdit.Location = new System.Drawing.Point(245, 70);
		this.btnEdit.Name = "btnEdit";
		this.btnEdit.Size = new System.Drawing.Size(41, 23);
		this.btnEdit.TabIndex = 5;
		this.btnEdit.Text = "Edit";
		this.btnEdit.UseVisualStyleBackColor = true;
		this.btnEdit.Click += new System.EventHandler(btnEdit_Click);
		this.btnImport.Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.btnImport.Location = new System.Drawing.Point(112, 477);
		this.btnImport.Name = "btnImport";
		this.btnImport.Size = new System.Drawing.Size(75, 23);
		this.btnImport.TabIndex = 6;
		this.btnImport.Text = "Import";
		this.btnImport.UseVisualStyleBackColor = true;
		this.btnImport.Click += new System.EventHandler(btnImport_Click);
		this.txtSearch.ForeColor = System.Drawing.Color.Gray;
		this.txtSearch.Location = new System.Drawing.Point(49, 135);
		this.txtSearch.Name = "txtSearch";
		this.txtSearch.Size = new System.Drawing.Size(100, 23);
		this.txtSearch.TabIndex = 7;
		this.txtSearch.Text = "Пошук";
		this.txtSearch.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		this.txtSearch.Enter += new System.EventHandler(txtSearch_Enter);
		this.txtSearch.Leave += new System.EventHandler(txtSearch_Leave);
		this.btnSearch.Location = new System.Drawing.Point(155, 134);
		this.btnSearch.Name = "btnSearch";
		this.btnSearch.Size = new System.Drawing.Size(55, 23);
		this.btnSearch.TabIndex = 8;
		this.btnSearch.Text = "\ud83d\udd0d";
		this.btnSearch.UseVisualStyleBackColor = true;
		this.btnSearch.Click += new System.EventHandler(btnSearch_Click);
		this.btnDelete.Location = new System.Drawing.Point(216, 135);
		this.btnDelete.Name = "btnDelete";
		this.btnDelete.Size = new System.Drawing.Size(50, 23);
		this.btnDelete.TabIndex = 9;
		this.btnDelete.Text = "Del";
		this.btnDelete.UseVisualStyleBackColor = true;
		this.btnDelete.Click += new System.EventHandler(btnDelete_Click);
		this.lblSearchStatus.Font = new System.Drawing.Font("Segoe UI", 15.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.lblSearchStatus.Location = new System.Drawing.Point(70, 179);
		this.lblSearchStatus.Name = "lblSearchStatus";
		this.lblSearchStatus.Size = new System.Drawing.Size(158, 31);
		this.lblSearchStatus.TabIndex = 10;
		this.lblSearchStatus.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
		this.txtResult.BackColor = System.Drawing.Color.LightSlateGray;
		this.txtResult.BorderStyle = System.Windows.Forms.BorderStyle.None;
		this.txtResult.Cursor = System.Windows.Forms.Cursors.IBeam;
		this.txtResult.Font = new System.Drawing.Font("Segoe UI", 18f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 204);
		this.txtResult.ForeColor = System.Drawing.Color.GreenYellow;
		this.txtResult.Location = new System.Drawing.Point(70, 276);
		this.txtResult.Name = "txtResult";
		this.txtResult.ReadOnly = true;
		this.txtResult.Size = new System.Drawing.Size(158, 32);
		this.txtResult.TabIndex = 11;
		this.txtResult.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
		base.AutoScaleDimensions = new System.Drawing.SizeF(7f, 15f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
		this.BackColor = System.Drawing.Color.LightSlateGray;
		base.ClientSize = new System.Drawing.Size(298, 523);
		base.Controls.Add(this.txtResult);
		base.Controls.Add(this.lblSearchStatus);
		base.Controls.Add(this.btnDelete);
		base.Controls.Add(this.btnSearch);
		base.Controls.Add(this.txtSearch);
		base.Controls.Add(this.btnImport);
		base.Controls.Add(this.btnEdit);
		base.Controls.Add(this.lblCount);
		base.Controls.Add(this.txtPrice);
		base.Controls.Add(this.comboVendors);
		base.Controls.Add(this.lblTitle);
		this.ForeColor = System.Drawing.Color.DarkBlue;
		base.Icon = (System.Drawing.Icon)resources.GetObject("$this.Icon");
		base.Name = "Form1";
		this.Text = "Articul Master Pro";
		base.Load += new System.EventHandler(Form1_Load);
		base.ResumeLayout(false);
		base.PerformLayout();
	}
}
