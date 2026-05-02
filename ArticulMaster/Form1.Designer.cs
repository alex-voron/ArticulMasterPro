namespace ArticulMaster
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
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
            lblTitle.Location = new Point(26, 27);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new Size(247, 30);
            lblTitle.TabIndex = 0;
            lblTitle.Text = "ARTICUL MASTER PRO";
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

        #endregion

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
    }
}
