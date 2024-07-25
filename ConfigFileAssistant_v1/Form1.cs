using BrightIdeasSoftware;
using CalibrationTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
namespace ConfigFileAssistant_v1
{
    public partial class MainForm : Form
    {
        private string initFilePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
        private string filePath;
        private List<VariableInfo> csVariables;
        private List<VariableInfo> ymlVariables;
        private List<VariableInfo> resultVariables;

        private Config conf;
        private ImageList CommandImageList;
        private ImageList ExpandImageList;
        private ImageList EditImageList;
        private Image ExpandImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/expand.png");
        private Image CollapseImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/collapse.png");
        private Image PlusImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/plus_color.png");
        private Image MinusImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/minus_color.png");
        private Image CautionImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/caution.png");
        private Image EditImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/edit.png");
        private Image BookImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/book.png");
        private Image FixImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/technics.png");
        private bool IsExpanded = true;
        private bool isEditMode = false;
        private ToolTip tooltip;
        private ContextMenuStrip contextMenuStrip;
        public MainForm()
        {
            InitializeComponent();
            SetupButtonImages();
            SetupMenuItems();
        }

        private void SetupButtonImages()
        {
            ExpandImageList = new ImageList();
            ExpandImageList.ImageSize = new Size(32, 32);
            ExpandImageList.Images.Add("collapse", CollapseImageButton);
            ExpandImageList.Images.Add("expand", ExpandImageButton);
            expandAllButton.ImageList = ExpandImageList;
            expandAllButton.Image = ExpandImageList.Images[0];

            CommandImageList = new ImageList();
            CommandImageList.ImageSize = new Size(16, 16);
            CommandImageList.Images.Add("plus", PlusImageButton);
            CommandImageList.Images.Add("minus", MinusImageButton);
            CommandImageList.Images.Add("caution", CautionImageButton);

            EditImageList = new ImageList();
            EditImageList.ImageSize = new Size(25, 25);
            EditImageList.Images.Add("edit", EditImageButton);
            EditImageList.Images.Add("book", BookImageButton);
            EditImageList.Images.Add("fix", FixImageButton);

            tooltip = new ToolTip();
            editButton.ImageList = EditImageList;
            editButton.Image = EditImageList.Images[0];
            tooltip.SetToolTip(editButton, "Current View: Read-Only, Click to Edit");
            RemoveButtonBorder(editButton);

            fixButton.Image = EditImageList.Images[2];
            tooltip.SetToolTip(fixButton, "Fix All Errors");
            RemoveButtonBorder(fixButton);

        }

        private void RemoveButtonBorder(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            button.FlatAppearance.BorderSize = 0;
        }

        private void SetupMenuItems()
        {
            contextMenuStrip = new ContextMenuStrip();
            var addRowMenuItem = new ToolStripMenuItem("Add Row");
            var deleteRowMenuItem = new ToolStripMenuItem("Delete Row");
            contextMenuStrip.Items.AddRange(new ToolStripItem[] { addRowMenuItem, deleteRowMenuItem });
            addRowMenuItem.Click += AddRowMenuItem_Click;
            deleteRowMenuItem.Click += DeleteRowMenuItem_Click;
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            filePath = initFilePath;
            ConfigValidator.LoadYamlFile(filePath);
            conf = new Config();
            csVariables = ConfigValidator.ExtractCsVariables();
            ymlVariables = ConfigValidator.ExtractYmlVariables();
            resultVariables = ConfigValidator.CompareVariables(csVariables, ymlVariables);
            VariableDataTreeListView.CellEditStarting += DataTreeListView_CellEditStarting;
            VariableDataTreeListView.CellEditFinishing += DataTreeListView_CellEditFinishing;
            AddVariablesToDataGridView(resultVariables, VariableDataTreeListView);
        }

        private void AddVariablesToDataGridView(List<VariableInfo> variables, DataTreeListView objectListView)
        {
            objectListView.SmallImageList = CommandImageList;
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
                        else if (variableInfo.Result == Result.WRONG_VALUE)
                        {
                            return "caution";
                        }
                        return null;
                    },
                    Renderer = new MultiImageRenderer()
                };

                objectListView.AllColumns.Add(nameColumn);
                objectListView.AllColumns.Add(new OLVColumn("Type", "Type") { AspectName = "TypeName", Width = 200 });
                objectListView.AllColumns.Add(new OLVColumn("Value", "Value") { AspectName = "Value", Width = 200 });

                objectListView.AllColumns.Add(new OLVColumn("Message", "Message")
                {
                    AspectName = "Result",
                    Width = 200,
                    AspectToStringConverter = value =>
                    {
                        if (value is Result result && (result == Result.OK))
                        {
                            return string.Empty;
                        }
                        return value.ToString();
                    }
                });

                objectListView.FormatRow += PaintRows;
                objectListView.RebuildColumns();
            }

            objectListView.ExpandAll();
        }

        private void PaintRows(object sender, FormatRowEventArgs e)
        {
            var variableInfo = (VariableInfo)e.Model;

            if (variableInfo.Result == Result.ONLY_IN_CS)
            {
                e.Item.BackColor = Color.PaleGreen;

            }
            else if (variableInfo.Result == Result.ONLY_IN_YML)
            {
                e.Item.BackColor = Color.LightPink;
            }
            else if (variableInfo.Result == Result.WRONG_VALUE)
            {
                e.Item.BackColor = Color.Gold;
            }
        }

        private void ExpandAllButton_Click(object sender, EventArgs e)
        {
            if (IsExpanded)
            {
                VariableDataTreeListView.CollapseAll();
                expandAllButton.Image = ExpandImageList.Images["expand"];
                IsExpanded = false;
            }
            else
            {
                VariableDataTreeListView.ExpandAll();
                expandAllButton.Image = ExpandImageList.Images["collapse"];
                IsExpanded = true;
            }
        }

        
        private void SetEditable(bool isEditable)
        {
            foreach (OLVColumn column in VariableDataTreeListView.Columns)
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
                VariableDataTreeListView.CellEditActivation = ObjectListView.CellEditActivateMode.SingleClick;
                VariableDataTreeListView.BackColor = Color.PowderBlue;
                VariableDataTreeListView.ForeColor = Color.Black;
                VariableDataTreeListView.CellClick += DataTreeListView_CellClick;
                VariableDataTreeListView.CellRightClick += VariableDataTreeListView_CellRightClick;
                VariableDataTreeListView.CellOver += DataTreeListView_CellOver;
            }
            else
            {
                VariableDataTreeListView.CellEditActivation = ObjectListView.CellEditActivateMode.None;
                VariableDataTreeListView.BackColor = SystemColors.Window;
                VariableDataTreeListView.ForeColor = SystemColors.ControlText;
                VariableDataTreeListView.CellClick -= DataTreeListView_CellClick;
                VariableDataTreeListView.CellRightClick -= VariableDataTreeListView_CellRightClick;
                VariableDataTreeListView.CellOver -= DataTreeListView_CellOver;
            }
        }

        private void VariableDataTreeListView_CellRightClick(object sender, CellRightClickEventArgs e)
        {
            contextMenuStrip.Tag = e.RowIndex;
            var screenPosition = VariableDataTreeListView.PointToScreen(e.Location);

            contextMenuStrip.Show(screenPosition);

            e.Handled = true;

        }
        private void AddRowMenuItem_Click(object sender, EventArgs e)
        {
            var selectedObject = VariableDataTreeListView.SelectedObject as VariableInfo;
            var selectedIndex = VariableDataTreeListView.SelectedIndex;
            string path = "";
            if (selectedObject != null)
            {
                string fullName = selectedObject.FullName;
                int lastIndex = fullName.LastIndexOf('.');
                if (lastIndex != -1)
                {
                    path = fullName.Substring(0, lastIndex);
                }
                
            }
            using (ObjectCreater objectCreater = new ObjectCreater(path))
            {
                if (objectCreater.ShowDialog() == DialogResult.OK)
                {
                    List<VariableInfo> newObjects = objectCreater.CreatedVariables;
                    for (int i = newObjects.Count-1; i>=0; --i)
                    {
                        var success = ConfigValidator.InsertChildToParent(selectedObject, newObjects[i]);
                        if (!success)
                        {
                            MessageBox.Show("Adding row failed.");
                        }
                        else
                        {
                            VariableDataTreeListView.SetObjects(ymlVariables);
                            VariableDataTreeListView.SelectedObject = newObjects[i];
                        }
                    }
                    VariableDataTreeListView.SelectedIndex = selectedIndex + newObjects.Count;
                }
            }

        }


        private void DeleteRowMenuItem_Click(object sender, EventArgs e)
        {
            var selectedObject = VariableDataTreeListView.SelectedObject as VariableInfo;
            if (selectedObject != null)
            {
                if (!ConfigValidator.RemoveChildFromVariable(selectedObject))
                {
                    MessageBox.Show("Row deletion failed.");
                }
                else
                {
                    VariableDataTreeListView.SetObjects(ymlVariables);
                }
            }
        }

        private void DataTreeListView_CellOver(object sender, CellOverEventArgs e)
        {
            if (e.Model == null || e.SubItem == null)
                return;

            var variableInfo = (VariableInfo)e.Model;
            if (variableInfo.Result != Result.OK  && e.ColumnIndex == 0)
            {
                Cursor = Cursors.Hand;
            }
            else
            {
                Cursor = Cursors.Default;
            }
        }

        private void DataTreeListView_CellClick(object sender, CellClickEventArgs e)
        {
            if (e.Model == null)
                return;

            var variableInfo = (VariableInfo)e.Model;

            if (variableInfo.Result != Result.OK && e.ColumnIndex == 0)
            {
                bool isSuccess = FixErrors(variableInfo);
                if (isSuccess)
                {
                    variableInfo.Result = Result.OK;
                    ConfigValidator.RemoveVariableFromErrorList(variableInfo.FullName);
                }
            }
        }
        private void DataTreeListView_CellEditStarting(object sender, CellEditEventArgs e)
        {
            VariableInfo variableInfo = (VariableInfo)e.RowObject;

            if (variableInfo.TypeName != typeof(Dictionary<,>).Name && variableInfo.TypeName != typeof(List<>).Name)
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
                if (e.Value != null && !e.Value.Equals(e.NewValue))
                {
                    var message = ConfigValidator.UpdateChild(variable.FullName, e.NewValue);
                    if (message != string.Empty)
                    {
                        e.Cancel = true;
                        MessageBox.Show(message);

                    }
                }
            }
        }
        private bool FixErrors(VariableInfo variableInfo)
        {
            var success = false;
            switch (variableInfo.Result)
            {
                case Result.ONLY_IN_CS:
                    success = ConfigValidator.AddChildToVariable(variableInfo);
                    break;
                case Result.ONLY_IN_YML:
                    success = ConfigValidator.RemoveChildFromVariable(variableInfo);
                    VariableDataTreeListView.RemoveObject(variableInfo);
                    break;
                case Result.WRONG_VALUE:
                    if (variableInfo.HasChildren())
                    {
                        variableInfo.Children.Clear();
                        VariableDataTreeListView.RemoveObjects(variableInfo.Children);
                    }
                    success = ConfigValidator.ModifyChildFromVariable(variableInfo);
                    break;
                default:
                    break;
            }
            return success;
        }

        private void NextButton_Click(object sender, EventArgs e)
        {
            var compareResult = ConfigValidator.CompareVariables(csVariables, ymlVariables);
            AddVariablesToDataGridView(compareResult, VariableDataTreeListView);
            if (ConfigValidator.HasErrors())
            {
                MessageBox.Show("There is an error.");
            }
            else
            {
                var result = MessageBox.Show("Do you want to proceed?", "Log Message", MessageBoxButtons.OK);
                if (result == DialogResult.OK)
                {
                    ConfigValidator.MakeYamlFile(ymlVariables, filePath);
                    this.Close();
                }
            }

        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void EditButton_Click(object sender, EventArgs e)
        {
            if (isEditMode)
            {
                isEditMode = false;
                editButton.Image = EditImageList.Images["edit"];
                tooltip.SetToolTip(editButton, "Current View: Read View, Click to Edit View");
            }
            else
            {
                isEditMode = true;
                editButton.Image = EditImageList.Images["book"];
                tooltip.SetToolTip(editButton, "Current View: Edit, Click to Read View");
            }
            SetEditable(isEditMode);
        }


        private void fixButton_Click_1(object sender, EventArgs e)
        {

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



