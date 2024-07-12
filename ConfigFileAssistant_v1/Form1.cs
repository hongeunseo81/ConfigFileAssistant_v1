using BrightIdeasSoftware;
using CalibrationTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
namespace ConfigFileAssistant_v1
{
    public partial class MainForm : Form
    {
        private string initFilePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
        private string filePath;
        private List<VariableInfo> csVariables;
        private List<VariableInfo> ymlVariables;
        private Config conf;
        private ImageList imageList;
        private ImageList buttonImageList;
        private Image ExpandImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/expand.png");
        private Image CollapseImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/collapse.png");
        private Image PlusImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/plus_color.png");
        private Image MinusImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/minus_color.png");
        private Image CautionImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/caution.png");
        private bool IsExpanded = true;

        public MainForm()
        {
            InitializeComponent();
            SetupButtonImages();
            expandAllButton.Image = buttonImageList.Images[0];
            treeModeRadioButton.Checked = true; 
        }

        private void SetupButtonImages()
        {
            buttonImageList = new ImageList();
            buttonImageList.ImageSize = new Size(30, 30);
            buttonImageList.Images.Add("collapse", CollapseImageButton);
            buttonImageList.Images.Add("expand",ExpandImageButton);
            expandAllButton.ImageList = buttonImageList;

            imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            imageList.Images.Add("plus", PlusImageButton);
            imageList.Images.Add("minus", MinusImageButton);
            imageList.Images.Add("caution", CautionImageButton);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            init();
        }

        private void init()
        {
            filePath = initFilePath;
            filePathTextBox.Text = filePath;
            conf = new Config();
            csVariables = ConfigValidator.ExtractCsVariables();
            ymlVariables = ConfigValidator.ExtractYmlVariables(filePath);
            var result = ConfigValidator.CompareVariables(csVariables, ymlVariables);

            AddVariablesToDataGridView(result, ymlDataTreeListView);
        }

        private void AddVariablesToDataGridView(List<VariableInfo> variables, DataTreeListView objectListView)
        {
            objectListView.SmallImageList = imageList;
            objectListView.SetObjects(variables);
            objectListView.CanExpandGetter = x => ((VariableInfo)x).HasChildren();
            objectListView.ChildrenGetter = x => ((VariableInfo)x).Children;

            OLVColumn nameColumn = new OLVColumn("Name", "Name")
            {
                AspectName = "Name",
                Width = 300,
                AspectGetter = delegate (object rowObject)
                {
                    var variableInfo = (VariableInfo)rowObject;
                    return variableInfo.Name;
                },
                ImageGetter = delegate (object rowObject)
                {
                    var variableInfo = (VariableInfo)rowObject;
                    if (variableInfo.Result == Result.ONLY_IN_YML)
                    {
                        return "minus";
                    }
                    else if (variableInfo.Result == Result.ONLY_IN_CS)
                    {
                        return "plus";
                    }
                    else if(variableInfo.Result == Result.TYPE_MISMATCH) 
                    {
                        return "caution";
                    }
                    return null;
                },
                Renderer = new MultiImageRenderer()
            };

            objectListView.AllColumns.Add(nameColumn);
            objectListView.AllColumns.Add(new OLVColumn("Type", "Type") { AspectName = "TypeName", Width = 300 });
            objectListView.AllColumns.Add(new OLVColumn("Value", "Value") { AspectName = "Value", Width = 300 });


            objectListView.UseCellFormatEvents = true;

            objectListView.FormatRow += (sender, e) =>
            {
                var variableInfo = (VariableInfo)e.Model;

                if (variableInfo.Result == Result.ONLY_IN_CS)
                {
                    e.Item.BackColor = Color.PaleGreen;
                    e.Item.ForeColor = Color.Black;
                }
                else if (variableInfo.Result == Result.ONLY_IN_YML)
                {
                    e.Item.BackColor = Color.LightPink;
                    e.Item.ForeColor = Color.Black;
                }
                else if (variableInfo.Result == Result.TYPE_MISMATCH)
                {
                    e.Item.BackColor = Color.LemonChiffon;
                    e.Item.ForeColor = Color.Black;
                }

            };

            objectListView.RebuildColumns();
            ymlDataTreeListView.ExpandAll();
        }

        private void expandAllButton_Click(object sender, EventArgs e)
        {
            if (IsExpanded)
            {
                ymlDataTreeListView.CollapseAll();
                expandAllButton.Image = buttonImageList.Images["expand"];
                IsExpanded = false;
            }
            else
            {
                ymlDataTreeListView.ExpandAll();
                expandAllButton.Image = buttonImageList.Images["collapse"];
                IsExpanded = true;
            }
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            List<VariableInfo> wrongVariables = ymlVariables.Where(v => v.Result != Result.OK).ToList();
            List<VariableInfo> filteredCsVariables = csVariables.Where(v => wrongVariables.Any(o => o.Name == v.Name)).ToList();
            var Log = ConfigValidator.MigrateVariables(filteredCsVariables, filePath);
            var result = MessageBox.Show(Log.Message.ToString(),"Log Message", MessageBoxButtons.OK);
            if(result == DialogResult.OK)
            {
                this.Close();
            }
            
        }

        private void cancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }

    public class MultiImageRenderer : BaseRenderer
    {
        public override void Render(Graphics g, Rectangle r)
        {
            int imageWidth = 16;
            int imageHeight = 16;

            if (this.ImageList != null)
            {
                int imageIndex = this.Aspect as int? ?? -1;
                if (imageIndex >= 0 && imageIndex < this.ImageList.Images.Count)
                {
                    g.DrawImage(this.ImageList.Images[imageIndex], r.X, r.Y, imageWidth, imageHeight);
                    r.X += imageWidth + 2;
                }
            }

            if (this.Aspect != null)
            {
                string text = this.Aspect.ToString();
                using (Brush brush = new SolidBrush(this.GetForegroundColor()))
                {
                    g.DrawString(text, this.Font, brush, r);
                }
            }
        }
    }
}




