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
            this.ymlDataTreeListView = new BrightIdeasSoftware.DataTreeListView();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ymlDataTreeListView)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.browseButton);
            this.panel1.Controls.Add(this.filePathTextBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1413, 69);
            this.panel1.TabIndex = 2;
            // 
            // browseButton
            // 
            this.browseButton.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.browseButton.Font = new System.Drawing.Font("Arial Narrow", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.browseButton.Location = new System.Drawing.Point(790, 25);
            this.browseButton.Name = "browseButton";
            this.browseButton.Size = new System.Drawing.Size(103, 29);
            this.browseButton.TabIndex = 3;
            this.browseButton.Text = "BROWSE";
            this.browseButton.UseVisualStyleBackColor = true;
            // 
            // filePathTextBox
            // 
            this.filePathTextBox.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.filePathTextBox.Font = new System.Drawing.Font("굴림", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.filePathTextBox.Location = new System.Drawing.Point(109, 30);
            this.filePathTextBox.Name = "filePathTextBox";
            this.filePathTextBox.Size = new System.Drawing.Size(661, 22);
            this.filePathTextBox.TabIndex = 4;
            // 
            // panel2
            // 
            this.panel2.Location = new System.Drawing.Point(0, 894);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1413, 93);
            this.panel2.TabIndex = 3;
            // 
            // ymlDataTreeListView
            // 
            this.ymlDataTreeListView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ymlDataTreeListView.CellEditUseWholeCell = false;
            this.ymlDataTreeListView.DataSource = null;
            this.ymlDataTreeListView.Font = new System.Drawing.Font("굴림", 14.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ymlDataTreeListView.FullRowSelect = true;
            this.ymlDataTreeListView.GridLines = true;
            this.ymlDataTreeListView.HeaderUsesThemes = true;
            this.ymlDataTreeListView.HeaderWordWrap = true;
            this.ymlDataTreeListView.HideSelection = false;
            this.ymlDataTreeListView.Location = new System.Drawing.Point(76, 86);
            this.ymlDataTreeListView.Name = "ymlDataTreeListView";
            this.ymlDataTreeListView.OwnerDrawnHeader = true;
            this.ymlDataTreeListView.RootKeyValueString = "";
            this.ymlDataTreeListView.ShowGroups = false;
            this.ymlDataTreeListView.Size = new System.Drawing.Size(1213, 744);
            this.ymlDataTreeListView.TabIndex = 5;
            this.ymlDataTreeListView.UseCompatibleStateImageBehavior = false;
            this.ymlDataTreeListView.UseFiltering = true;
            this.ymlDataTreeListView.View = System.Windows.Forms.View.Details;
            this.ymlDataTreeListView.VirtualMode = true;
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(1413, 986);
            this.Controls.Add(this.ymlDataTreeListView);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "ConfigFileAssistant";
            this.Load += new System.EventHandler(this.Form1_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ymlDataTreeListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button browseButton;
        private System.Windows.Forms.TextBox filePathTextBox;
        private System.Windows.Forms.Panel panel2;
        private BrightIdeasSoftware.DataTreeListView ymlDataTreeListView;
    }
}

