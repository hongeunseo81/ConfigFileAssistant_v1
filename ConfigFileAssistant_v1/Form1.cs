using BrightIdeasSoftware;
using CalibrationTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
namespace ConfigFileAssistant_v1
{
    public partial class MainForm  : Form
    {
        private Config Conf = new Config();
        private string BackupFilePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/";
        private string FilePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
        private List<VariableInfo> InitYmlVariables;

        private List<VariableInfo> CsVariables;
        private List<VariableInfo> YmlVariables;
        private List<VariableInfo> ResultVariables;

        private ImageList CommandImageList;
        private ImageList ExpandImageList;
        private ImageList EditModeImageList;
        private ImageList ButtonImageList;
        private ImageList ResultImageList;

        private Image ExpandImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/down.png");
        private Image CollapseImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/up.png");
       
        private Image PlusImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/plus_color.png");
        private Image MinusImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/minus_color.png");
        private Image CautionImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/caution.png");
        
        private Image EditImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/edit-on.png");
        private Image ReadImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/edit-off.png");
        
        private Image FixImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/fix.png");
        private Image BrowseImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/folder-open.png");
        private Image ResetImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/refresh.png");
        private Image SaveAsImageButton = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/save-as.png");
        
        private Image LogoImage = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/letter-c.png");
        private Image ResultFailImage = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/failed.png");
        private Image ResultSuccessImage = Image.FromFile("C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/icon/success.png");

        private bool IsExpanded = true;
        private bool IsEditMode = false;
        private ToolTip ToolTip;
        private ContextMenuStrip ContextMenuStrip;

        private Dictionary<string, object[]> EditedVariables = new Dictionary<string, object[]>();


        public MainForm()
        {
            InitializeComponent();
            SetupButtonImages();
            SetupMenuItems();
            SetupData();
        }

        public enum Status
        {
            Created,
            Deleted,
            Updated,
            Skipped,
            Failed
        }
        // Set Up
        private void SetupButtonImages()
        {
            ResultImageList = new ImageList();
            ResultImageList.ImageSize = new Size(200, 200);
            ResultImageList.Images.Add(ResultSuccessImage);
            ResultImageList.Images.Add(ResultFailImage);

            ExpandImageList = new ImageList();
            ExpandImageList.ImageSize = new Size(32, 32);
            ExpandImageList.Images.Add("collapse", CollapseImageButton);
            ExpandImageList.Images.Add("expand", ExpandImageButton);
            ExpandAllButton.ImageList = ExpandImageList;
            ExpandAllButton.Image = ExpandImageList.Images[0];
            RemoveButtonBorder(ExpandAllButton);

            EditModeImageList = new ImageList();
            EditModeImageList.ImageSize = new Size(32, 32);
            EditModeImageList.Images.Add("edit-off", ReadImageButton);
            EditModeImageList.Images.Add("edit-on", EditImageButton);
            ModeButton.ImageList = EditModeImageList;

            CommandImageList = new ImageList();
            CommandImageList.ImageSize = new Size(16, 16);
            CommandImageList.Images.Add("plus", PlusImageButton);
            CommandImageList.Images.Add("minus", MinusImageButton);
            CommandImageList.Images.Add("caution", CautionImageButton);

            ButtonImageList = new ImageList();
            ButtonImageList.ImageSize = new Size(25, 25);
            ButtonImageList.Images.Add("browse", BrowseImageButton);
            ButtonImageList.Images.Add("fix", FixImageButton);
            ButtonImageList.Images.Add("save-as", SaveAsImageButton);
            ButtonImageList.Images.Add("reset", ResetImageButton);
            ToolTip = new ToolTip();
            ModeButton.ImageList = ButtonImageList;


            ModeButton.Image = EditModeImageList.Images[0];
            ToolTip.SetToolTip(ModeButton, "Current View: Read-Only, Click to Edit");
            RemoveButtonBorder(ModeButton);

            FileBrowseButton.Image = ButtonImageList.Images[0];
            ToolTip.SetToolTip(FileBrowseButton, "Browse other config files");
            BackupBrowseButton.Image = ButtonImageList.Images[0];
            ToolTip.SetToolTip(BackupBrowseButton, "Change backup file path");

            FixAllButton.Image = ButtonImageList.Images[1];
            ToolTip.SetToolTip(FixAllButton, "Fix All Errors");

            SaveAsButton.Image = ButtonImageList.Images[2];
            ToolTip.SetToolTip(SaveAsButton, "Save As");

            ResetButton.Image = ButtonImageList.Images[3];
            ToolTip.SetToolTip(ResetButton, "reset");
        }
        private void RemoveButtonBorder(Button button)
        {
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderColor = Color.FromArgb(0, 255, 255, 255);
            button.FlatAppearance.BorderSize = 0;
        }
        private void SetupMenuItems()
        {
            ContextMenuStrip = new ContextMenuStrip();
            var addChildMenuItem = new ToolStripMenuItem("Add Child");
            var addRowMenuItem = new ToolStripMenuItem("Add Row");
            var deleteRowMenuItem = new ToolStripMenuItem("Delete Row");
            ContextMenuStrip.Items.AddRange(new ToolStripItem[] { addChildMenuItem,addRowMenuItem, deleteRowMenuItem });
            addChildMenuItem.Click += AddChildMenuItem_Click;
            addRowMenuItem.Click += AddRowMenuItem_Click;
            deleteRowMenuItem.Click += DeleteRowMenuItem_Click;
        }
       
        private void SetupData()
        {
            BackupPathTextBox.Text = BackupFilePath;
            ConfigFilePathTextBox.Text = FilePath;
            ConfigValidator.LoadYamlFile(FilePath);
            CsVariables = ConfigValidator.ExtractCsVariables();
            YmlVariables = ConfigValidator.ExtractYmlVariables();
            ResultVariables = ConfigValidator.CompareVariables(CsVariables, YmlVariables);
            AddVariablesToDataGridView(ResultVariables, VariableDataTreeListView);
            SetResultPicture();
        }

        // Form Load
        private void Form1_Load(object sender, EventArgs e)
        {
            logoPictureBox.Image = LogoImage;
            VariableDataTreeListView.CellEditStarting += DataTreeListView_CellEditStarting;
            VariableDataTreeListView.CellEditFinishing += DataTreeListView_CellEditFinishing;
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
                        if (variableInfo.Result == Result.OnlyInYml)
                        {
                            return "minus";
                        }
                        else if (variableInfo.Result == Result.OnlyInCs)
                        {
                            return "plus";
                        }
                        else if (variableInfo.Result == Result.WrongValue)
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
                        if (value is Result result && (result == Result.Ok))
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

            if (variableInfo.Result == Result.OnlyInCs)
            {
                e.Item.BackColor = Color.PaleGreen;

            }
            else if (variableInfo.Result == Result.OnlyInYml)
            {
                e.Item.BackColor = Color.LightPink;
            }
            else if (variableInfo.Result == Result.WrongValue)
            {
                e.Item.BackColor = Color.Gold;
            }
            else if(variableInfo.Result == Result.NoChild)
            {
                e.Item.BackColor = Color.Silver;
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
                VariableDataTreeListView.BackColor = System.Drawing.ColorTranslator.FromHtml("#CFCFEB");
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
            var selectedObject = VariableDataTreeListView.GetModelObject(e.RowIndex) as VariableInfo;
            bool isGenericType = selectedObject.Type.IsGenericType;

            foreach (ToolStripItem item in ContextMenuStrip.Items)
            {
                if (item is ToolStripMenuItem menuItem && menuItem.Text == "Add Child")
                {
                    menuItem.Visible = isGenericType;
                    break;
                }
            }
            var screenPosition = VariableDataTreeListView.PointToScreen(e.Location);
            ContextMenuStrip.Show(screenPosition);

            e.Handled = true;
        }
       
        // Browse files
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            using(OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "YAML files (*.yml)|*.yml";
                openFileDialog.Title = "Select a Config file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    FilePath = openFileDialog.FileName;
                    ConfigValidator.ClearAllData();
                    ConfigValidator.Init();
                    SetupData();
                    AddVariablesToDataGridView(ResultVariables, VariableDataTreeListView);
                }
            }
        }
        private void BackupBrowseButton_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                folderBrowserDialog.Description = "Select a folder to back up your data.";
                folderBrowserDialog.ShowNewFolderButton = true;

                DialogResult result = folderBrowserDialog.ShowDialog();

                if (result == DialogResult.OK && !string.IsNullOrWhiteSpace(folderBrowserDialog.SelectedPath))
                {
                    BackupFilePath = folderBrowserDialog.SelectedPath;
                }
            }
        }
        // Edit cells
        private void AddChildMenuItem_Click(object sender, EventArgs e)
        {
            var selectedObject = VariableDataTreeListView.SelectedObject as VariableInfo;
            string path = "";
            if (selectedObject != null)
            {
                string fullName = selectedObject.FullName;
                int lastIndex = fullName.LastIndexOf('.');
                if (lastIndex != -1)
                {
                    path = fullName;
                }

                using (ObjectCreater objectCreater = new ObjectCreater(path))
                {
                    if (objectCreater.ShowDialog() == DialogResult.OK)
                    {
                        List<VariableInfo> newObjects = objectCreater.CreatedVariables;
                        for (int i = newObjects.Count - 1; i >= 0; --i)
                        {
                            Status status = Status.Created;
                            var success = ConfigValidator.InsertChildToVariable(selectedObject,newObjects[i]);
                            if (!success)
                            {
                                MessageBox.Show("Adding row failed.");
                                status = Status.Failed;
                            }
                            else
                            {
                                selectedObject.Result = selectedObject.Result == Result.NoChild ? Result.Ok : selectedObject.Result;
                                VariableDataTreeListView.SetObjects(YmlVariables);
                                VariableDataTreeListView.SelectedObject = newObjects[i];
                            }
                            MakeCurrentLog(newObjects[i].FullName, newObjects[i].Value.ToString(), "", status);
                        }
                        VariableDataTreeListView.SelectedIndex = selectedObject.Children.Count + newObjects.Count;
                        RefreshResult();
                    }
                }
            }
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
                    for (int i = newObjects.Count - 1; i >= 0; --i)
                    {
                        Status status = Status.Created;
                        var success = ConfigValidator.InsertVariable(selectedObject, newObjects[i]);
                        if (!success)
                        {
                            MessageBox.Show("Adding row failed.");
                            status = Status.Failed;
                        }
                        else
                        {
                            VariableDataTreeListView.SetObjects(YmlVariables);
                            VariableDataTreeListView.SelectedObject = newObjects[i];
                        }
                        MakeCurrentLog(newObjects[i].FullName, newObjects[i].Value.ToString(), "", status);
                    }
                    VariableDataTreeListView.SelectedIndex = selectedIndex + newObjects.Count;
                    RefreshResult();
                }
            }
        }
        private void DeleteRowMenuItem_Click(object sender, EventArgs e)
        {
            var selectedObject = VariableDataTreeListView.SelectedObject as VariableInfo;
            if (selectedObject != null)
            {
                Status status = Status.Deleted;
                if (!ConfigValidator.RemoveVariable(selectedObject))
                {
                    MessageBox.Show("Row deletion failed.");
                    status = Status.Failed;
                }
                else
                {
                    VariableDataTreeListView.SetObjects(YmlVariables);
                    RefreshResult();
                }
                MakeCurrentLog(selectedObject.FullName,"", "", status);
            }
        }
        private void DataTreeListView_CellOver(object sender, CellOverEventArgs e)
        {
            if (e.Model == null || e.SubItem == null)
                return;

            var variableInfo = (VariableInfo)e.Model;
            if (variableInfo.Result != Result.Ok && variableInfo.Result != Result.NoChild)
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
            var oldValue = variableInfo.Value;
            if (variableInfo.Result != Result.Ok && variableInfo.Result != Result.NoChild)
            {
                Status status = FixError(variableInfo);
                if (status != Status.Failed && status != Status.Skipped)
                {
                    ConfigValidator.RemoveVariableFromErrorList(variableInfo.FullName);
                    variableInfo.Result = Result.Ok;
                    SetResultPicture();
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

               if(variableInfo.Type == typeof(bool))
                {
                    CheckBox cb = new CheckBox();
                    cb.Bounds = e.CellBounds;
                    cb.Checked = variableInfo.Value.ToString() == "True" ? true : false;
                    cb.CheckedChanged += (s, args) =>
                    {
                        e.NewValue = cb.Checked;
                    };
                    e.Control = cb;
                }
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
                if (e.Value != null && e.Value.ToString() != e.NewValue.ToString())
                {
                    var message = ConfigValidator.UpdateChild(variable.FullName, e.NewValue);
                    if (message != string.Empty)
                    {
                        e.Cancel = true;
                        MessageBox.Show(message);
                    }
                    else
                    {
                        MakeCurrentLog(variable.FullName, e.NewValue.ToString(), e.Value.ToString(), Status.Updated);
                    }
                }
            }
        }
       
        // Fix Error
        private Status FixError(VariableInfo variableInfo)
        {
            Status status = Status.Failed;
            var oldValue = variableInfo.Value;
            switch (variableInfo.Result)
            {
                case Result.OnlyInCs:
                    if(ConfigValidator.AddVariable(variableInfo))
                    {
                        status = Status.Created;
                    }
                    break;
                case Result.OnlyInYml:
                    if (ConfigValidator.RemoveVariable(variableInfo))
                    {
                        VariableDataTreeListView.RemoveObject(variableInfo);
                        status = Status.Deleted;
                    }
                    break;
                case Result.WrongValue:
                    if (variableInfo.HasChildren())
                    {
                        variableInfo.Children.Clear();
                        VariableDataTreeListView.RemoveObjects(variableInfo.Children);
                    }
                    if(ConfigValidator.ModifyChildFromVariable(variableInfo))
                    {
                        status = Status.Updated;
                    }
                    break;
                case Result.NoChild:
                    status = Status.Skipped;
                    break;
                default:
                    break;
            }
            MakeCurrentLog(variableInfo.FullName, variableInfo.Value.ToString(), oldValue.ToString(), status);
            return status;
        }
       
        // Toggle 
        private void ModeButton_Click(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                IsEditMode = false;
                ModeButton.Image = EditModeImageList.Images["edit-off"];
                ToolTip.SetToolTip(ModeButton, "Current View: Read View, Click to Edit View");
            }
            else
            {
                IsEditMode = true;
                ModeButton.Image = EditModeImageList.Images["edit-on"];
                ToolTip.SetToolTip(ModeButton, "Current View: Edit, Click to Read View");
            }
            modeLabel.Text = IsEditMode ? "On" : "Off";
            SetEditable(IsEditMode);
        }
        private void ExpandAllButton_Click(object sender, EventArgs e)
        {
            if (IsExpanded)
            {
                VariableDataTreeListView.CollapseAll();
                ExpandAllButton.Image = ExpandImageList.Images["expand"];
                IsExpanded = false;
            }
            else
            {
                VariableDataTreeListView.ExpandAll();
                ExpandAllButton.Image = ExpandImageList.Images["collapse"];
                IsExpanded = true;
            }
        }

        // Control Buttons
        private void ResetButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you really want to reset?");
            if(result == DialogResult.OK)
            {
                LogListBox.Items.Clear();
                YmlVariables.Clear();
                ConfigValidator.ClearAllData();
                YmlVariables = ConfigValidator.ExtractYmlVariables();
                var compareResult = ConfigValidator.CompareVariables(CsVariables, YmlVariables);
                AddVariablesToDataGridView(compareResult, VariableDataTreeListView);
                SetResultPicture();
            }
            
        }  
        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();

            saveFileDialog.Filter = "YAML files (*.yml)|*.yml";
            saveFileDialog.Title = "Save a Config file";

            saveFileDialog.FileName = "config.yml";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string filePath = saveFileDialog.FileName;
                ConfigValidator.MakeYamlFile(filePath);
                MessageBox.Show("File saved successfully at: " + filePath);
            }
        }
        private void FixAllButton_Click(object sender, EventArgs e)
        {
            if (ConfigValidator.HasErrors())
            {
                var errors = ConfigValidator.GetErrors();
                List<string> fixedErrorsList = new List<string>();
                foreach (var key in errors.Keys)
                {
                    var status = FixError(errors[key]);
                    if (status == Status.Failed)
                    {
                        MessageBox.Show("A problem occurred while fixing the error.");
                        return;
                    }
                    else if(status != Status.Skipped)
                    {
                        fixedErrorsList.Add(key);
                    }
                }

                foreach (var fixedError in fixedErrorsList)
                {
                    ConfigValidator.RemoveVariableFromErrorList(fixedError);
                }
                VariableDataTreeListView.Refresh();
                SetResultPicture();
            }
            else
            {
                MessageBox.Show("There are no errors to fix.");
            }
        }
       
        private void RefreshResult()
        {
            var compareResult = ConfigValidator.CompareVariables(CsVariables, YmlVariables);
            AddVariablesToDataGridView(compareResult, VariableDataTreeListView);
            SetResultPicture();
        }
        private void SetResultPicture()
        {
            if(ConfigValidator.HasErrors()) 
            {
                resultPictureBox.Image = ResultImageList.Images[1];
            }
            else
            {
                resultPictureBox.Image = ResultImageList.Images[0];
            }
        }
        private void NextButton_Click(object sender, EventArgs e)
        {
            var compareResult = ConfigValidator.CompareVariables(CsVariables, YmlVariables);
            AddVariablesToDataGridView(compareResult, VariableDataTreeListView);
            if (ConfigValidator.HasErrors())
            {
                MessageBox.Show("There is an error.");
                SetResultPicture() ;    
            }
            else
            {
                using (var dialog = new BackupDialogForm())
                {
                    var result = dialog.ShowDialog(this);
                    if (result == DialogResult.OK)
                    {
                        if (dialog.BackupChecked)
                        {
                            ConfigValidator.MakeBackup(BackupFilePath, FilePath);
                        }

                        // Yaml 파일 생성
                        ConfigValidator.MakeYamlFile(FilePath);
                        this.Close();
                    }
                }

            }
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        private void MakeCurrentLog(string fullname, string newValue, string oldValue, Status status)
        {
            StringBuilder logMessage = new StringBuilder();
            var name = fullname.Split('.').Last();
            logMessage.Append($"[{DateTime.Now}]: {name} is ");
            if (status == Status.Created)
            {
                newValue = newValue == string.Empty ? "Default Value" : newValue;
                logMessage.Append($"{status} : value:{newValue}");
            }
            else if (status == Status.Updated)
            {
                newValue = newValue == string.Empty ? "Default Value" : newValue;
                logMessage.Append($"{status} {oldValue} -> {newValue}");
            }
            else if(status == Status.Skipped)
            {
                logMessage.Append($"{status}, You should add child.");
            }
            else
            {
                logMessage.Append($"{status}");
            }
            LogListBox.Items.Add(logMessage);
            LogListBox.SelectedIndex = LogListBox.Items.Count - 1;

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



