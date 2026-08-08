using System;
using System.Drawing;
using System.Windows.Forms;

namespace TemplateGenerator
{
    partial class Form1
    {
        private System.ComponentModel.IContainer components = null;
        
        private Panel panelMain;
        private Panel panelTop;
        private Label lblTitle;
        private Label lblTemplatePath;
        private Label lblExcelPath;
        private Label lblInstancesCount;
        private Button btnLoadTemplate;
        private Button btnLoadExcel;
        private Button btnGenerate;
        private Button btnValidate;
        private Button btnConvertTags;
        private Button btnClearAll;
        private RichTextBox richTextBoxPreview;
        private DataGridView dataGridViewReplacements;
        private Label lblReplacements;
        private Label lblPreview;
        private TableLayoutPanel mainLayout;
        private Panel rightPanel;
        private Label lblMappingInfo;
        private RichTextBox richTextBoxMapping;
        private Panel leftPanel;
        private Panel leftTopPanel;
        private Panel logPanel;
        private Label lblLog;
        private RichTextBox richTextBoxLog;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            
            this.panelMain = new Panel();
            this.panelTop = new Panel();
            this.lblTitle = new Label();
            this.lblTemplatePath = new Label();
            this.lblExcelPath = new Label();
            this.lblInstancesCount = new Label();
            this.btnLoadTemplate = new Button();
            this.btnLoadExcel = new Button();
            this.btnGenerate = new Button();
            this.btnValidate = new Button();
            this.btnConvertTags = new Button();
            this.btnClearAll = new Button();
            this.richTextBoxPreview = new RichTextBox();
            this.dataGridViewReplacements = new DataGridView();
            this.lblReplacements = new Label();
            this.lblPreview = new Label();
            this.mainLayout = new TableLayoutPanel();
            this.rightPanel = new Panel();
            this.lblMappingInfo = new Label();
            this.richTextBoxMapping = new RichTextBox();
            this.leftPanel = new Panel();
            this.leftTopPanel = new Panel();
            this.logPanel = new Panel();
            this.lblLog = new Label();
            this.richTextBoxLog = new RichTextBox();

            this.panelMain.SuspendLayout();
            this.panelTop.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReplacements)).BeginInit();
            this.mainLayout.SuspendLayout();
            this.rightPanel.SuspendLayout();
            this.leftPanel.SuspendLayout();
            this.leftTopPanel.SuspendLayout();
            this.logPanel.SuspendLayout();
            this.SuspendLayout();

            // panelMain
            this.panelMain.BackColor = Color.FromArgb(13, 37, 63);
            this.panelMain.Controls.Add(this.mainLayout);
            this.panelMain.Dock = DockStyle.Fill;
            this.panelMain.Location = new Point(0, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Size = new Size(1450, 900);
            this.panelMain.TabIndex = 0;

            // panelTop
            this.panelTop.BackColor = Color.FromArgb(0, 80, 150);
            this.panelTop.Controls.Add(this.lblTitle);
            this.panelTop.Dock = DockStyle.Top;
            this.panelTop.Location = new Point(0, 0);
            this.panelTop.Name = "panelTop";
            this.panelTop.Size = new Size(1450, 50);
            this.panelTop.TabIndex = 0;

            // lblTitle
            this.lblTitle.AutoSize = true;
            this.lblTitle.Font = new Font("Segoe UI", 16F, FontStyle.Bold);
            this.lblTitle.ForeColor = Color.White;
            this.lblTitle.Location = new Point(20, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new Size(411, 30);
            this.lblTitle.TabIndex = 0;
            this.lblTitle.Text = "Генератор экземпляров по шаблону";

            // mainLayout
            this.mainLayout.BackColor = Color.FromArgb(13, 37, 63);
            this.mainLayout.ColumnCount = 4;
            this.mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));
            this.mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40F));
            this.mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            this.mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 10F));
            
            // Добавляем все элементы управления в mainLayout
            this.mainLayout.Controls.Add(this.leftPanel, 0, 2);
            this.mainLayout.Controls.Add(this.richTextBoxPreview, 1, 2);
            this.mainLayout.Controls.Add(this.rightPanel, 2, 2);
            this.mainLayout.Controls.Add(this.btnLoadTemplate, 3, 0);
            this.mainLayout.Controls.Add(this.btnLoadExcel, 3, 1);
            this.mainLayout.Controls.Add(this.btnValidate, 3, 2);
            this.mainLayout.Controls.Add(this.btnGenerate, 3, 3);
            this.mainLayout.Controls.Add(this.lblTemplatePath, 0, 0);
            this.mainLayout.Controls.Add(this.lblExcelPath, 1, 0);
            this.mainLayout.Controls.Add(this.lblInstancesCount, 2, 0);
            this.mainLayout.Controls.Add(this.btnConvertTags, 3, 4);
            this.mainLayout.Controls.Add(this.btnClearAll, 3, 5);
            this.mainLayout.Controls.Add(this.logPanel, 0, 6);  // <-- ВАЖНО: добавляем logPanel
            
            this.mainLayout.Dock = DockStyle.Fill;
            this.mainLayout.Location = new Point(0, 50);
            this.mainLayout.Name = "mainLayout";
            this.mainLayout.Padding = new Padding(10);
            this.mainLayout.RowCount = 7;
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 30F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 70F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 50F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Absolute, 40F));
            this.mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 30F));
            this.mainLayout.Size = new Size(1450, 850);
            this.mainLayout.TabIndex = 1;

            // leftPanel
            this.leftPanel.BackColor = Color.FromArgb(20, 50, 80);
            this.leftPanel.Controls.Add(this.dataGridViewReplacements);
            this.leftPanel.Controls.Add(this.leftTopPanel);
            this.leftPanel.Dock = DockStyle.Fill;
            this.leftPanel.Location = new Point(13, 73);
            this.leftPanel.Name = "leftPanel";
            this.leftPanel.Padding = new Padding(5);
            this.leftPanel.Size = new Size(270, 458);
            this.leftPanel.TabIndex = 11;

            // leftTopPanel
            this.leftTopPanel.BackColor = Color.FromArgb(20, 50, 80);
            this.leftTopPanel.Controls.Add(this.lblReplacements);
            this.leftTopPanel.Dock = DockStyle.Top;
            this.leftTopPanel.Location = new Point(5, 5);
            this.leftTopPanel.Name = "leftTopPanel";
            this.leftTopPanel.Size = new Size(260, 30);
            this.leftTopPanel.TabIndex = 12;

            // lblReplacements
            this.lblReplacements.AutoSize = true;
            this.lblReplacements.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblReplacements.ForeColor = Color.FromArgb(200, 220, 240);
            this.lblReplacements.Location = new Point(3, 5);
            this.lblReplacements.Name = "lblReplacements";
            this.lblReplacements.Size = new Size(295, 19);
            this.lblReplacements.TabIndex = 7;
            this.lblReplacements.Text = "Позиции для поиска (номер совпадает с Excel)";

            // dataGridViewReplacements
            this.dataGridViewReplacements.AllowUserToAddRows = false;
            this.dataGridViewReplacements.AllowUserToDeleteRows = false;
            this.dataGridViewReplacements.BackgroundColor = Color.FromArgb(30, 60, 90);
            this.dataGridViewReplacements.BorderStyle = BorderStyle.None;
            this.dataGridViewReplacements.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridViewReplacements.Dock = DockStyle.Fill;
            this.dataGridViewReplacements.GridColor = Color.FromArgb(60, 100, 140);
            this.dataGridViewReplacements.Location = new Point(5, 35);
            this.dataGridViewReplacements.Name = "dataGridViewReplacements";
            this.dataGridViewReplacements.RowHeadersVisible = false;
            this.dataGridViewReplacements.ScrollBars = ScrollBars.Vertical;
            this.dataGridViewReplacements.Size = new Size(260, 418);
            this.dataGridViewReplacements.TabIndex = 10;
            this.dataGridViewReplacements.CellValueChanged += new DataGridViewCellEventHandler(this.dataGridViewReplacements_CellValueChanged);
            this.dataGridViewReplacements.CurrentCellDirtyStateChanged += new EventHandler(this.dataGridViewReplacements_CurrentCellDirtyStateChanged);

            // rightPanel
            this.rightPanel.BackColor = Color.FromArgb(20, 50, 80);
            this.rightPanel.Controls.Add(this.lblMappingInfo);
            this.rightPanel.Controls.Add(this.richTextBoxMapping);
            this.rightPanel.Dock = DockStyle.Fill;
            this.rightPanel.Location = new Point(543, 73);
            this.rightPanel.Name = "rightPanel";
            this.rightPanel.Padding = new Padding(5);
            this.rightPanel.Size = new Size(408, 458);
            this.rightPanel.TabIndex = 12;

            // lblMappingInfo
            this.lblMappingInfo.AutoSize = true;
            this.lblMappingInfo.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblMappingInfo.ForeColor = Color.FromArgb(200, 220, 240);
            this.lblMappingInfo.Location = new Point(8, 8);
            this.lblMappingInfo.Name = "lblMappingInfo";
            this.lblMappingInfo.Size = new Size(273, 19);
            this.lblMappingInfo.TabIndex = 8;
            this.lblMappingInfo.Text = "Информация о заменах для экземпляров";

            // richTextBoxMapping
            this.richTextBoxMapping.BackColor = Color.FromArgb(30, 60, 90);
            this.richTextBoxMapping.BorderStyle = BorderStyle.FixedSingle;
            this.richTextBoxMapping.Dock = DockStyle.Fill;
            this.richTextBoxMapping.Font = new Font("Consolas", 9F);
            this.richTextBoxMapping.ForeColor = Color.White;
            this.richTextBoxMapping.Location = new Point(5, 30);
            this.richTextBoxMapping.Name = "richTextBoxMapping";
            this.richTextBoxMapping.ReadOnly = true;
            this.richTextBoxMapping.Size = new Size(398, 423);
            this.richTextBoxMapping.TabIndex = 9;
            this.richTextBoxMapping.Text = "Загрузите Excel файл для отображения информации о заменах...\n\n" +
                "Формат Excel:\n" +
                "Столбец A: Имя экземпляра (LOOP)\n" +
                "Столбец B: Tag No (новый тег)\n" +
                "Столбец C: № позиции для замены (1-10)";
            this.richTextBoxMapping.WordWrap = true;

            // lblPreview - размещаем в mainLayout, но не добавляем как отдельный контрол
            // Он будет добавлен позже в коде

            // richTextBoxPreview
            this.richTextBoxPreview.BackColor = Color.FromArgb(30, 60, 90);
            this.richTextBoxPreview.BorderStyle = BorderStyle.FixedSingle;
            this.richTextBoxPreview.Dock = DockStyle.Fill;
            this.richTextBoxPreview.Font = new Font("Consolas", 9F);
            this.richTextBoxPreview.ForeColor = Color.White;
            this.richTextBoxPreview.Location = new Point(299, 73);
            this.richTextBoxPreview.Name = "richTextBoxPreview";
            this.richTextBoxPreview.Size = new Size(238, 458);
            this.richTextBoxPreview.TabIndex = 9;
            this.richTextBoxPreview.Text = "";
            this.richTextBoxPreview.WordWrap = false;

            // Добавляем lblPreview в mainLayout
            this.mainLayout.Controls.Add(this.lblPreview, 1, 1);

            // lblPreview
            this.lblPreview.AutoSize = true;
            this.lblPreview.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblPreview.ForeColor = Color.FromArgb(200, 220, 240);
            this.lblPreview.Location = new Point(299, 40);
            this.lblPreview.Name = "lblPreview";
            this.lblPreview.Size = new Size(204, 19);
            this.lblPreview.TabIndex = 8;
            this.lblPreview.Text = "Предпросмотр файла шаблона";

            // lblTemplatePath
            this.lblTemplatePath.AutoSize = true;
            this.lblTemplatePath.ForeColor = Color.FromArgb(200, 220, 240);
            this.lblTemplatePath.Location = new Point(13, 10);
            this.lblTemplatePath.Name = "lblTemplatePath";
            this.lblTemplatePath.Size = new Size(58, 15);
            this.lblTemplatePath.TabIndex = 0;
            this.lblTemplatePath.Text = "Шаблон: -";

            // lblExcelPath
            this.lblExcelPath.AutoSize = true;
            this.lblExcelPath.ForeColor = Color.FromArgb(200, 220, 240);
            this.lblExcelPath.Location = new Point(299, 10);
            this.lblExcelPath.Name = "lblExcelPath";
            this.lblExcelPath.Size = new Size(42, 15);
            this.lblExcelPath.TabIndex = 1;
            this.lblExcelPath.Text = "Excel: -";

            // lblInstancesCount
            this.lblInstancesCount.AutoSize = true;
            this.lblInstancesCount.ForeColor = Color.FromArgb(200, 220, 240);
            this.lblInstancesCount.Location = new Point(543, 10);
            this.lblInstancesCount.Name = "lblInstancesCount";
            this.lblInstancesCount.Size = new Size(134, 15);
            this.lblInstancesCount.TabIndex = 2;
            this.lblInstancesCount.Text = "Найдено экземпляров: 0";

            // btnLoadTemplate
            this.btnLoadTemplate.BackColor = Color.FromArgb(0, 120, 215);
            this.btnLoadTemplate.FlatAppearance.BorderSize = 0;
            this.btnLoadTemplate.FlatStyle = FlatStyle.Flat;
            this.btnLoadTemplate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnLoadTemplate.ForeColor = Color.White;
            this.btnLoadTemplate.Location = new Point(967, 13);
            this.btnLoadTemplate.Name = "btnLoadTemplate";
            this.btnLoadTemplate.Size = new Size(130, 24);
            this.btnLoadTemplate.TabIndex = 3;
            this.btnLoadTemplate.Text = "📁 Загрузить шаблон";
            this.btnLoadTemplate.UseVisualStyleBackColor = false;
            this.btnLoadTemplate.Click += new EventHandler(this.btnLoadTemplate_Click);

            // btnLoadExcel
            this.btnLoadExcel.BackColor = Color.FromArgb(0, 120, 215);
            this.btnLoadExcel.FlatAppearance.BorderSize = 0;
            this.btnLoadExcel.FlatStyle = FlatStyle.Flat;
            this.btnLoadExcel.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnLoadExcel.ForeColor = Color.White;
            this.btnLoadExcel.Location = new Point(967, 43);
            this.btnLoadExcel.Name = "btnLoadExcel";
            this.btnLoadExcel.Size = new Size(130, 24);
            this.btnLoadExcel.TabIndex = 4;
            this.btnLoadExcel.Text = "📊 Загрузить экземпляры";
            this.btnLoadExcel.UseVisualStyleBackColor = false;
            this.btnLoadExcel.Click += new EventHandler(this.btnLoadExcel_Click);

            // btnValidate
            this.btnValidate.BackColor = Color.FromArgb(0, 150, 136);
            this.btnValidate.FlatAppearance.BorderSize = 0;
            this.btnValidate.FlatStyle = FlatStyle.Flat;
            this.btnValidate.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnValidate.ForeColor = Color.White;
            this.btnValidate.Location = new Point(967, 73);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new Size(130, 24);
            this.btnValidate.TabIndex = 5;
            this.btnValidate.Text = "✅ Проверить";
            this.btnValidate.UseVisualStyleBackColor = false;
            this.btnValidate.Click += new EventHandler(this.btnValidate_Click);

            // btnConvertTags
            this.btnConvertTags.BackColor = Color.FromArgb(255, 193, 7);
            this.btnConvertTags.FlatAppearance.BorderSize = 0;
            this.btnConvertTags.FlatStyle = FlatStyle.Flat;
            this.btnConvertTags.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            this.btnConvertTags.ForeColor = Color.Black;
            this.btnConvertTags.Location = new Point(967, 615);
            this.btnConvertTags.Name = "btnConvertTags";
            this.btnConvertTags.Size = new Size(130, 40);
            this.btnConvertTags.TabIndex = 13;
            this.btnConvertTags.Text = "🔄 Преобразовать тэг";
            this.btnConvertTags.UseVisualStyleBackColor = false;
            this.btnConvertTags.Click += new EventHandler(this.btnConvertTags_Click);

            // btnClearAll
            this.btnClearAll.BackColor = Color.FromArgb(220, 50, 50);
            this.btnClearAll.FlatAppearance.BorderSize = 0;
            this.btnClearAll.FlatStyle = FlatStyle.Flat;
            this.btnClearAll.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.btnClearAll.ForeColor = Color.White;
            this.btnClearAll.Location = new Point(967, 665);
            this.btnClearAll.Name = "btnClearAll";
            this.btnClearAll.Size = new Size(130, 35);
            this.btnClearAll.TabIndex = 14;
            this.btnClearAll.Text = "🗑 Очистить все";
            this.btnClearAll.UseVisualStyleBackColor = false;
            this.btnClearAll.Click += new EventHandler(this.btnClearAll_Click);

            // btnGenerate
            this.btnGenerate.BackColor = Color.FromArgb(255, 87, 34);
            this.btnGenerate.FlatAppearance.BorderSize = 0;
            this.btnGenerate.FlatStyle = FlatStyle.Flat;
            this.btnGenerate.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
            this.btnGenerate.ForeColor = Color.White;
            this.btnGenerate.Location = new Point(967, 565);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new Size(130, 44);
            this.btnGenerate.TabIndex = 6;
            this.btnGenerate.Text = "🚀 Сгенерировать";
            this.btnGenerate.UseVisualStyleBackColor = false;
            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);

            // logPanel - растягивается на все колонки
            this.logPanel.BackColor = Color.FromArgb(10, 30, 50);
            this.logPanel.Controls.Add(this.lblLog);
            this.logPanel.Controls.Add(this.richTextBoxLog);
            this.logPanel.Dock = DockStyle.Fill;
            this.logPanel.Location = new Point(13, 720);
            this.logPanel.Name = "logPanel";
            this.logPanel.Padding = new Padding(5);
            this.logPanel.Size = new Size(1424, 116);
            this.logPanel.TabIndex = 15;

            // lblLog
            this.lblLog.AutoSize = true;
            this.lblLog.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
            this.lblLog.ForeColor = Color.FromArgb(200, 220, 240);
            this.lblLog.Location = new Point(8, 5);
            this.lblLog.Name = "lblLog";
            this.lblLog.Size = new Size(131, 19);
            this.lblLog.TabIndex = 13;
            this.lblLog.Text = "📋 События и ошибки";

            // richTextBoxLog
            this.richTextBoxLog.BackColor = Color.FromArgb(10, 25, 45);
            this.richTextBoxLog.BorderStyle = BorderStyle.None;
            this.richTextBoxLog.Dock = DockStyle.Fill;
            this.richTextBoxLog.Font = new Font("Consolas", 9F);
            this.richTextBoxLog.ForeColor = Color.White;
            this.richTextBoxLog.Location = new Point(5, 27);
            this.richTextBoxLog.Name = "richTextBoxLog";
            this.richTextBoxLog.ReadOnly = true;
            this.richTextBoxLog.Size = new Size(1414, 84);
            this.richTextBoxLog.TabIndex = 12;
            this.richTextBoxLog.Text = "";
            this.richTextBoxLog.WordWrap = false;

            // Устанавливаем ColumnSpan для logPanel (растягиваем на все 4 колонки)
            this.mainLayout.SetColumnSpan(this.logPanel, 4);

            // Form1
            this.AutoScaleDimensions = new SizeF(7F, 15F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(13, 37, 63);
            this.ClientSize = new Size(1450, 900);
            this.Controls.Add(this.panelMain);
            this.MinimumSize = new Size(1300, 800);
            this.Name = "Form1";
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Text = "Генератор экземпляров по шаблону";
            
            this.panelMain.ResumeLayout(false);
            this.panelTop.ResumeLayout(false);
            this.panelTop.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewReplacements)).EndInit();
            this.mainLayout.ResumeLayout(false);
            this.mainLayout.PerformLayout();
            this.rightPanel.ResumeLayout(false);
            this.rightPanel.PerformLayout();
            this.leftPanel.ResumeLayout(false);
            this.leftTopPanel.ResumeLayout(false);
            this.leftTopPanel.PerformLayout();
            this.logPanel.ResumeLayout(false);
            this.logPanel.PerformLayout();
            this.ResumeLayout(false);
        }
    }
}