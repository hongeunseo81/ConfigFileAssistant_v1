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
            this.browseButton = new System.Windows.Forms.Button();
            this.filePathTextBox = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.NEXT = new System.Windows.Forms.Button();
            this.cancelButton = new System.Windows.Forms.Button();
            this.ymlDataTreeListView = new BrightIdeasSoftware.DataTreeListView();
            this.csDataTreeView = new BrightIdeasSoftware.DataTreeListView();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ymlDataTreeListView)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.csDataTreeView)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.browseButton);
            this.panel1.Controls.Add(this.filePathTextBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1991, 69);
            this.panel1.TabIndex = 2;
            // 
            // browseButton
            // 
            this.browseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.browseButton.Location = new System.Drawing.Point(1862, 26);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(103, 21);
            this.browseButton.TabIndex = 3;
            this.browseButton.Text = "BROWSE";
            this.browseButton.UseVisualStyleBackColor = true;
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.filePathTextBox.Location = new System.Drawing.Point(1262, 26);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.Size = new System.Drawing.Size(594, 21);
            this.filePathTextBox.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.NEXT);
            this.panel2.Controls.Add(this.cancelButton);
            this.panel2.Location = new System.Drawing.Point(0, 971);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1999, 93);
            this.panel2.TabIndex = 3;
            // 
            // NEXT
            // 
            this.NEXT.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.NEXT.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.NEXT.Location = new System.Drawing.Point(1828, 25);
            this.NEXT.Name = "NEXT";
            this.NEXT.Size = new System.Drawing.Size(137, 36);
            this.NEXT.TabIndex = 6;
            this.NEXT.Text = "NEXT";
            this.NEXT.UseVisualStyleBackColor = true;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.cancelButton.Font = new System.Drawing.Font("Arial Rounded MT Bold", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.cancelButton.Location = new System.Drawing.Point(1662, 25);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(137, 36);
            this.cancelButton.TabIndex = 5;
            this.cancelButton.Text = "CANCEL";
            this.cancelButton.UseVisualStyleBackColor = true;
            // 
            // ymlDataTreeListView
            // 
            this.ymlDataTreeListView.CellEditUseWholeCell = false;
            this.ymlDataTreeListView.DataSource = null;
            this.ymlDataTreeListView.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.ymlDataTreeListView.FullRowSelect = true;
            this.ymlDataTreeListView.GridLines = true;
            this.ymlDataTreeListView.HeaderUsesThemes = true;
            this.ymlDataTreeListView.HeaderWordWrap = true;
            this.ymlDataTreeListView.HideSelection = false;
            this.ymlDataTreeListView.Location = new System.Drawing.Point(877, 88);
            this.ymlDataTreeListView.Name = "ymlDataTreeListView";
            this.ymlDataTreeListView.RootKeyValueString = "";
            this.ymlDataTreeListView.ShowGroups = false;
            this.ymlDataTreeListView.Size = new System.Drawing.Size(1088, 877);
            this.ymlDataTreeListView.TabIndex = 5;
            this.ymlDataTreeListView.UseCompatibleStateImageBehavior = false;
            this.ymlDataTreeListView.UseFiltering = true;
            this.ymlDataTreeListView.View = System.Windows.Forms.View.Details;
            this.ymlDataTreeListView.VirtualMode = true;
            // 
            // csDataTreeView
            // 
            this.csDataTreeView.CellEditUseWholeCell = false;
            this.csDataTreeView.DataSource = null;
            this.csDataTreeView.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(129)));
            this.csDataTreeView.HideSelection = false;
            this.csDataTreeView.Location = new System.Drawing.Point(14, 87);
            this.csDataTreeView.Name = "csDataTreeView";
            this.csDataTreeView.RootKeyValueString = "";
            this.csDataTreeView.ShowGroups = false;
            this.csDataTreeView.Size = new System.Drawing.Size(825, 877);
            this.csDataTreeView.TabIndex = 6;
            this.csDataTreeView.UseCompatibleStateImageBehavior = false;
            this.csDataTreeView.View = System.Windows.Forms.View.Details;
            this.csDataTreeView.VirtualMode = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1991, 1062);
            this.Controls.Add(this.csDataTreeView);
            this.Controls.Add(this.ymlDataTreeListView);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "ConfigFileAssistant";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ymlDataTreeListView)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.csDataTreeView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button NEXT;
        private System.Windows.Forms.Button cancelButton;
        private BrightIdeasSoftware.DataTreeListView ymlDataTreeListView;
        private BrightIdeasSoftware.DataTreeListView csDataTreeView;
    }
}

