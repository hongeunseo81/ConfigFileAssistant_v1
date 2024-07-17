using BrightIdeasSoftware;
using CalibrationTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;
using System.Diagnostics;
namespace ConfigFileAssistant_v1
{
    public partial class MainForm : Form
    {
        private string initFilePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
        private string filePath;
        private List<VariableInfo> csVariables;
        private List<VariableInfo> ymlVariables;
        private Dictionary<string,string> editedValues;

        private Config conf;
        private ImageList imageList;
        private List<PictureBox> pictureBoxes;
        private ImageList buttonImageList;
        private Image ExpandImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/expand.png");
        private Image CollapseImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/collapse.png");
        private Image PlusImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/plus_color.png");
        private Image MinusImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/minus_color.png");
        private Image CautionImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/caution.png");
        private bool IsExpanded = true;
        private bool isEditMode = false;
        public MainForm()
        {
            InitializeComponent();
            SetupRadioButtons();
            SetupButtonImages();
        }

        private void SetupButtonImages()
        {
            buttonImageList = new ImageList();
            buttonImageList.ImageSize = new Size(30, 30);
            buttonImageList.Images.Add("collapse", CollapseImageButton);
            buttonImageList.Images.Add("expand",ExpandImageButton);
            expandAllButton.ImageList = buttonImageList;
            expandAllButton.Image = buttonImageList.Images[0];
            
            imageList = new ImageList();
            imageList.ImageSize = new Size(16, 16);
            imageList.Images.Add("plus", PlusImageButton);
            imageList.Images.Add("minus", MinusImageButton);
            imageList.Images.Add("caution", CautionImageButton);
           
        }
        

        private void SetupRadioButtons()
        {
            editedValues = new Dictionary<string, string>();
            treeModeRadioButton.Checked = true;
        }
       

        private void Form1_Load(object sender, EventArgs e)
        {
            filePath = initFilePath;
            ConfigValidator.LoadYamlFile(filePath);
            conf = new Config();
            csVariables = ConfigValidator.ExtractCsVariables();
            ymlVariables = ConfigValidator.ExtractYmlVariables();
            ConfigValidator.CompareVariables(csVariables, ymlVariables);
            ymlDataTreeListView.CellEditStarting += DataTreeListView_CellEditStarting;
            ymlDataTreeListView.CellEditFinishing += DataTreeListView_CellEditFinishing;
            AddVariablesToDataGridView(ymlVariables, ymlDataTreeListView);
        }

        private void AddVariablesToDataGridView(List<VariableInfo> variables, DataTreeListView objectListView)
        {
            objectListView.SmallImageList = imageList;
            objectListView.SetObjects(variables);
            objectListView.CanExpandGetter = x => ((VariableInfo)x).HasChildren();
            objectListView.ChildrenGetter = x => ((VariableInfo)x).Children;

            if (objectListView.AllColumns.Count == 0)
            {
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
                        else if (variableInfo.Result == Result.TYPE_MISMATCH)
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

                objectListView.FormatRow += paintRows;
                objectListView.RebuildColumns();
            }

            objectListView.ExpandAll();
        }
        private void paintRows (object sender, FormatRowEventArgs e)
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
                e.Item.BackColor = Color.Orange;
                e.Item.ForeColor = Color.Black;
            }
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

        private void DataTreeListView_CellEditStarting(object sender, CellEditEventArgs e)
        {
            VariableInfo variable = (VariableInfo)e.RowObject;
            if (variable.TypeName == typeof(string).Name)
            {
                e.Cancel = false;
                e.Control.Bounds = e.CellBounds;
                e.Control.Width = e.CellBounds.Width;
                e.Control.Height = e.CellBounds.Height;
            }
            else
            {
                e.Cancel = true;
            }
            
        }
        private void DataTreeListView_CellEditFinishing(object sender, CellEditEventArgs e)
        {
            VariableInfo variable = (VariableInfo)e.RowObject;

            if (e.NewValue != null)
            {
                if (e.Value == null)
                {
                    if (!string.IsNullOrEmpty(e.NewValue as string))
                    {
                        if (editedValues.ContainsKey(variable.FullName))
                        {
                            editedValues[variable.FullName] = e.NewValue as string;
                        }
                        else
                        {
                            editedValues.Add(variable.FullName, e.NewValue as string);
                        }
                    }
                }
                else if (!e.Value.Equals(e.NewValue))
                {
                    if (editedValues.ContainsKey(variable.FullName))
                    {
                        editedValues[variable.FullName] = e.NewValue as string;
                    }
                    else
                    {
                        editedValues.Add(variable.FullName, e.NewValue as string);
                    }
                }
            }
        }
        private void SetEditable(bool isEditable)
        {
            foreach (OLVColumn column in ymlDataTreeListView.Columns)
            {
                if (column.AspectName == "Value")
                {
                    column.IsEditable = isEditable;
                }
                else
                {
                    column.IsEditable = false;
                }
            }

            if (isEditable)
            {
                ymlDataTreeListView.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
                ymlDataTreeListView.BackColor = Color.PowderBlue;
                ymlDataTreeListView.ForeColor = Color.Black;
                ymlDataTreeListView.CellClick += YmlDataTreeListView_CellClick;
                ymlDataTreeListView.CellOver += YmlDataTreeListView_CellOver;
            }
            else
            {
                ymlDataTreeListView.CellEditActivation = ObjectListView.CellEditActivateMode.None;
                ymlDataTreeListView.BackColor = SystemColors.Window;
                ymlDataTreeListView.ForeColor = SystemColors.ControlText;
                ymlDataTreeListView.CellClick -= YmlDataTreeListView_CellClick;
                ymlDataTreeListView.CellOver -= YmlDataTreeListView_CellOver;
            }
        }

        private void YmlDataTreeListView_CellOver(object sender, CellOverEventArgs e)
        {
            if (e.Model == null || e.SubItem == null)
                return;

            var variableInfo = (VariableInfo)e.Model;
            if (variableInfo.Result != Result.OK && e.ColumnIndex==0)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        
        private void YmlDataTreeListView_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.Model == null)
                return;

            var variableInfo = (VariableInfo)e.Model;
            if (variableInfo.Result != Result.OK && e.ColumnIndex == 0)
            {
                switch(variableInfo.Result) 
                { 
                    case Result.ONLY_IN_CS:
                        ymlDataTreeListView.RemoveObject(variableInfo);
                        variableInfo.Result = Result.OK;
                        ymlDataTreeListView.AddObject(variableInfo);
                        break;
                    case Result.ONLY_IN_YML:
                        ConfigValidator.RemoveChild(ymlVariables, variableInfo.FullName);
                        ymlDataTreeListView.RemoveObject(variableInfo);
                        break;
                    case Result.TYPE_MISMATCH:
                        break;
                }
            }
        }

        private void ModeRadioButton_CheckedChanged(object sender, EventArgs e)
        {
            if (this.treeModeRadioButton.Checked)
            {
                if (editedValues.Count>0)
                {
                    var result = MessageBox.Show("변경사항을 저장하시겠습니까?", "Log Message", MessageBoxButtons.OK);
                    if (result == DialogResult.OK)
                    {
                        ConfigValidator.UpdateCellValueToNode(editedValues, filePath);
                        var newYamlVaraibles = ConfigValidator.ExtractYmlVariables();
                        ConfigValidator.CompareVariables(csVariables, newYamlVaraibles);
                        AddVariablesToDataGridView(newYamlVaraibles, ymlDataTreeListView);
                        editedValues.Clear();
                    }
                }

                isEditMode = false; 
            }
            else
            {
                isEditMode = true;  
                editedValues.Clear();
            }
            SetEditable(isEditMode);
        }

        private void nextButton_Click(object sender, EventArgs e)
        {
            if(isEditMode)
            {
                ConfigValidator.UpdateCellValueToNode(editedValues,filePath);
                ymlVariables = ConfigValidator.ExtractYmlVariables();
                ConfigValidator.CompareVariables(csVariables,ymlVariables);
            }

            if (ConfigValidator.HasError())
            {
                var result = MessageBox.Show("이대로 마이그레이션 하시겠습니까?", "Log Message", MessageBoxButtons.OK);
                if (result == DialogResult.OK)
                {
                    ConfigValidator.MigrateVariables(csVariables, ymlVariables, filePath);
                    this.Close();
                }
            }
            else
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




