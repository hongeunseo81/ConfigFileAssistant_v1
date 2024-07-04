namespace ConfigFileAssistant_v1
{
    partial class MainForm
    {
        /// <summary>
        /// 필수 디자이너 변수입니다.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 사용 중인 모든 리소스를 정리합니다.
        /// </summary>
        /// <param name="disposing">관리되는 리소스를 삭제해야 하면 true이고, 그렇지 않으면 false입니다.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form 디자이너에서 생성한 코드

        /// <summary>
        /// 디자이너 지원에 필요한 메서드입니다. 
        /// 이 메서드의 내용을 코드 편집기로 수정하지 마세요.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.browseButton = new System.Windows.Forms.Button();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.editButton = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.NEXT = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.dataGridView = new System.Windows.Forms.DataGridView();
            this.csOnlyFilter = new System.Windows.Forms.RadioButton();
            this.ymlOnlyFilter = new System.Windows.Forms.RadioButton();
            this.typeMismatchFilter = new System.Windows.Forms.RadioButton();
            this.okFilter = new System.Windows.Forms.RadioButton();
            this.panel3 = new System.Windows.Forms.Panel();
            this.variablesListView = new System.Windows.Forms.ListView();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).BeginInit();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.browseButton);
            this.panel1.Controls.Add(this.filePathTextBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1385, 69);
            this.panel1.TabIndex = 2;
            // 
            // browseButton
            // 
            this.browseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.browseButton.Location = new System.Drawing.Point(628, 27);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(103, 21);
            this.browseButton.TabIndex = 3;
            this.browseButton.Text = "BROWSE";
            this.browseButton.UseVisualStyleBackColor = true;
            this.browseButton.Click += new System.EventHandler(this.browseButton_Click);
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.filePathTextBox.Location = new System.Drawing.Point(28, 27);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.Size = new System.Drawing.Size(594, 21);
            this.filePathTextBox.TabIndex = 4;
            // 
            // editButton
            // 
            this.editButton.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.editButton.Location = new System.Drawing.Point(726, 132);
            this.editButton.Name = "editButton";
            this.editButton.Size = new System.Drawing.Size(88, 36);
            this.editButton.TabIndex = 3;
            this.editButton.Text = "EDIT";
            this.editButton.UseVisualStyleBackColor = true;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.NEXT);
            this.panel2.Controls.Add(this.cancelButton);
            this.panel2.Location = new System.Drawing.Point(0, 709);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1391, 79);
            this.panel2.TabIndex = 3;
            // 
            // NEXT
            // 
            this.NEXT.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.NEXT.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NEXT.Location = new System.Drawing.Point(1214, 18);
            this.NEXT.Name = "NEXT";
            this.NEXT.Size = new System.Drawing.Size(137, 36);
            this.NEXT.TabIndex = 6;
            this.NEXT.Text = "NEXT";
            this.NEXT.UseVisualStyleBackColor = true;
            this.NEXT.Click += new System.EventHandler(this.NEXT_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cancelButton.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.Location = new System.Drawing.Point(1061, 18);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(137, 36);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "CANCEL";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // dataGridView
            // 
            this.dataGridView.BackgroundColor = System.Drawing.Color.WhiteSmoke;
            this.dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView.GridColor = System.Drawing.Color.Gray;
            this.dataGridView.Location = new System.Drawing.Point(28, 174);
            this.dataGridView.Name = "dataGridView";
            this.dataGridView.RowTemplate.Height = 23;
            this.dataGridView.Size = new System.Drawing.Size(786, 453);
            this.dataGridView.TabIndex = 4;
            // 
            // csOnlyFilter
            // 
            this.csOnlyFilter.AutoSize = true;
            this.csOnlyFilter.ForeColor = System.Drawing.Color.Black;
            this.csOnlyFilter.Location = new System.Drawing.Point(24, 14);
            this.csOnlyFilter.Name = "csOnlyFilter";
            this.csOnlyFilter.Size = new System.Drawing.Size(79, 16);
            this.csOnlyFilter.TabIndex = 5;
            this.csOnlyFilter.TabStop = true;
            this.csOnlyFilter.Text = "CS_ONLY";
            this.csOnlyFilter.UseVisualStyleBackColor = true;
            this.csOnlyFilter.CheckedChanged += new System.EventHandler(this.csOnlyFilter_CheckedChanged);
            // 
            // ymlOnlyFilter
            // 
            this.ymlOnlyFilter.AutoSize = true;
            this.ymlOnlyFilter.ForeColor = System.Drawing.Color.Black;
            this.ymlOnlyFilter.Location = new System.Drawing.Point(126, 14);
            this.ymlOnlyFilter.Name = "ymlOnlyFilter";
            this.ymlOnlyFilter.Size = new System.Drawing.Size(88, 16);
            this.ymlOnlyFilter.TabIndex = 6;
            this.ymlOnlyFilter.TabStop = true;
            this.ymlOnlyFilter.Text = "YML_ONLY";
            this.ymlOnlyFilter.UseVisualStyleBackColor = true;
            this.ymlOnlyFilter.CheckedChanged += new System.EventHandler(this.ymlOnlyFilter_CheckedChanged);
            // 
            // typeMismatchFilter
            // 
            this.typeMismatchFilter.AutoSize = true;
            this.typeMismatchFilter.ForeColor = System.Drawing.Color.Black;
            this.typeMismatchFilter.Location = new System.Drawing.Point(227, 14);
            this.typeMismatchFilter.Name = "typeMismatchFilter";
            this.typeMismatchFilter.Size = new System.Drawing.Size(127, 16);
            this.typeMismatchFilter.TabIndex = 7;
            this.typeMismatchFilter.TabStop = true;
            this.typeMismatchFilter.Text = "TYPE_MISMATCH";
            this.typeMismatchFilter.UseVisualStyleBackColor = true;
            this.typeMismatchFilter.CheckedChanged += new System.EventHandler(this.typeMismatchFilter_CheckedChanged);
            // 
            // okFilter
            // 
            this.okFilter.AutoSize = true;
            this.okFilter.Location = new System.Drawing.Point(370, 14);
            this.okFilter.Name = "okFilter";
            this.okFilter.Size = new System.Drawing.Size(40, 16);
            this.okFilter.TabIndex = 8;
            this.okFilter.TabStop = true;
            this.okFilter.Text = "OK";
            this.okFilter.UseVisualStyleBackColor = true;
            this.okFilter.CheckedChanged += new System.EventHandler(this.okFilter_CheckedChanged);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.okFilter);
            this.panel3.Controls.Add(this.typeMismatchFilter);
            this.panel3.Controls.Add(this.ymlOnlyFilter);
            this.panel3.Controls.Add(this.csOnlyFilter);
            this.panel3.Location = new System.Drawing.Point(922, 160);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(429, 44);
            this.panel3.TabIndex = 9;
            // 
            // variablesListView
            // 
            this.variablesListView.HideSelection = false;
            this.variablesListView.Location = new System.Drawing.Point(922, 210);
            this.variablesListView.Name = "variablesListView";
            this.variablesListView.Size = new System.Drawing.Size(429, 232);
            this.variablesListView.TabIndex = 10;
            this.variablesListView.UseCompatibleStateImageBehavior = false;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1385, 796);
            this.Controls.Add(this.variablesListView);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.editButton);
            this.Controls.Add(this.dataGridView);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "ConfigFileAssistant";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView)).EndInit();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button editButton;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button NEXT;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.DataGridView dataGridView;
        private System.Windows.Forms.RadioButton csOnlyFilter;
        private System.Windows.Forms.RadioButton ymlOnlyFilter;
        private System.Windows.Forms.RadioButton typeMismatchFilter;
        private System.Windows.Forms.RadioButton okFilter;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.ListView variablesListView;
    }
}

