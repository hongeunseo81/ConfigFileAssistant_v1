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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.panel2 = new System.Windows.Forms.Panel();
            this.nextButton = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.VariableDataTreeListView = new BrightIdeasSoftware.DataTreeListView();
            this.expandAllButton = new System.Windows.Forms.Button();
            this.treeModeRadioButton = new System.Windows.Forms.RadioButton();
            this.editModeRadioButton = new System.Windows.Forms.RadioButton();
            this.dataTreeListView1 = new BrightIdeasSoftware.DataTreeListView();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.VariableDataTreeListView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTreeListView1)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1869, 69);
            this.panel1.TabIndex = 2;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.nextButton);
            this.panel2.Controls.Add(this.cancelButton);
            this.panel2.Location = new System.Drawing.Point(0, 894);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(921, 93);
            this.panel2.TabIndex = 3;
            // 
            // nextButton
            // 
            this.nextButton.Location = new System.Drawing.Point(744, 24);
            this.nextButton.Name = "nextButton";
            this.nextButton.Size = new System.Drawing.Size(134, 46);
            this.nextButton.TabIndex = 1;
            this.nextButton.Text = "NEXT";
            this.nextButton.UseVisualStyleBackColor = true;
            this.nextButton.Click += new System.EventHandler(this.nextButton_Click);
            // 
            // cancelButton
            // 
            this.cancelButton.Location = new System.Drawing.Point(604, 24);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(134, 46);
            this.cancelButton.TabIndex = 0;
            this.cancelButton.Text = "CANCEL";
            this.cancelButton.UseVisualStyleBackColor = true;
            this.cancelButton.Click += new System.EventHandler(this.cancelButton_Click);
            // 
            // VariableDataTreeListView
            // 
            this.VariableDataTreeListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.VariableDataTreeListView.CellEditUseWholeCell = false;
            this.VariableDataTreeListView.DataSource = null;
            this.VariableDataTreeListView.Font = new System.Drawing.Font("굴림", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.VariableDataTreeListView.FullRowSelect = true;
            this.VariableDataTreeListView.GridLines = true;
            this.VariableDataTreeListView.HeaderUsesThemes = true;
            this.VariableDataTreeListView.HeaderWordWrap = true;
            this.VariableDataTreeListView.HideSelection = false;
            this.VariableDataTreeListView.Location = new System.Drawing.Point(27, 163);
            this.VariableDataTreeListView.Name = "VariableDataTreeListView";
            this.VariableDataTreeListView.OwnerDrawnHeader = true;
            this.VariableDataTreeListView.RootKeyValueString = "";
            this.VariableDataTreeListView.ShowGroups = false;
            this.VariableDataTreeListView.Size = new System.Drawing.Size(851, 681);
            this.VariableDataTreeListView.TabIndex = 5;
            this.VariableDataTreeListView.UseCompatibleStateImageBehavior = false;
            this.VariableDataTreeListView.UseFiltering = true;
            this.VariableDataTreeListView.View = System.Windows.Forms.View.Details;
            this.VariableDataTreeListView.VirtualMode = true;
            // 
            // expandAllButton
            // 
            this.expandAllButton.Location = new System.Drawing.Point(27, 123);
            this.expandAllButton.Name = "expandAllButton";
            this.expandAllButton.Size = new System.Drawing.Size(58, 34);
            this.expandAllButton.TabIndex = 6;
            this.expandAllButton.UseVisualStyleBackColor = true;
            this.expandAllButton.Click += new System.EventHandler(this.expandAllButton_Click);
            // 
            // treeModeRadioButton
            // 
            this.treeModeRadioButton.AutoSize = true;
            this.treeModeRadioButton.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.treeModeRadioButton.Location = new System.Drawing.Point(661, 137);
            this.treeModeRadioButton.Name = "treeModeRadioButton";
            this.treeModeRadioButton.Size = new System.Drawing.Size(99, 20);
            this.treeModeRadioButton.TabIndex = 7;
            this.treeModeRadioButton.TabStop = true;
            this.treeModeRadioButton.Text = "tree mode";
            this.treeModeRadioButton.UseVisualStyleBackColor = true;
            this.treeModeRadioButton.CheckedChanged += new System.EventHandler(this.ModeRadioButton_CheckedChanged);
            // 
            // editModeRadioButton
            // 
            this.editModeRadioButton.AutoSize = true;
            this.editModeRadioButton.Font = new System.Drawing.Font("굴림", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.editModeRadioButton.Location = new System.Drawing.Point(777, 137);
            this.editModeRadioButton.Name = "editModeRadioButton";
            this.editModeRadioButton.Size = new System.Drawing.Size(97, 20);
            this.editModeRadioButton.TabIndex = 8;
            this.editModeRadioButton.TabStop = true;
            this.editModeRadioButton.Text = "edit mode";
            this.editModeRadioButton.UseVisualStyleBackColor = true;
            this.editModeRadioButton.CheckedChanged += new System.EventHandler(this.ModeRadioButton_CheckedChanged);
            // 
            // dataTreeListView1
            // 
            this.dataTreeListView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dataTreeListView1.CellEditUseWholeCell = false;
            this.dataTreeListView1.DataSource = null;
            this.dataTreeListView1.Font = new System.Drawing.Font("굴림", 15.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.dataTreeListView1.FullRowSelect = true;
            this.dataTreeListView1.GridLines = true;
            this.dataTreeListView1.HeaderUsesThemes = true;
            this.dataTreeListView1.HeaderWordWrap = true;
            this.dataTreeListView1.HideSelection = false;
            this.dataTreeListView1.Location = new System.Drawing.Point(931, 163);
            this.dataTreeListView1.Name = "dataTreeListView1";
            this.dataTreeListView1.OwnerDrawnHeader = true;
            this.dataTreeListView1.RootKeyValueString = "";
            this.dataTreeListView1.ShowGroups = false;
            this.dataTreeListView1.Size = new System.Drawing.Size(851, 681);
            this.dataTreeListView1.TabIndex = 9;
            this.dataTreeListView1.UseCompatibleStateImageBehavior = false;
            this.dataTreeListView1.UseFiltering = true;
            this.dataTreeListView1.View = System.Windows.Forms.View.Details;
            this.dataTreeListView1.VirtualMode = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1869, 986);
            this.Controls.Add(this.dataTreeListView1);
            this.Controls.Add(this.editModeRadioButton);
            this.Controls.Add(this.treeModeRadioButton);
            this.Controls.Add(this.expandAllButton);
            this.Controls.Add(this.VariableDataTreeListView);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "ConfigFileAssistant";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.VariableDataTreeListView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dataTreeListView1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private BrightIdeasSoftware.DataTreeListView VariableDataTreeListView;
        private System.Windows.Forms.Button expandAllButton;
        private System.Windows.Forms.Button nextButton;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.RadioButton treeModeRadioButton;
        private System.Windows.Forms.RadioButton editModeRadioButton;
        private BrightIdeasSoftware.DataTreeListView dataTreeListView1;
    }
}

