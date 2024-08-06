using BrightIdeasSoftware;
using ConfigFileAssistant_v1.Manager;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml.Linq;
using YamlDotNet.RepresentationModel;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;
namespace ConfigFileAssistant_v1
{
    public partial class MainForm : Form
    {
        private string BasePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug";
        private  ImageManager _imageManager;

        private List<ConfigVariable> CsVariables;
        private List<ConfigVariable> YmlVariables;
        private List<ConfigVariable> ResultVariables;

        private bool IsExpanded = true;
        private bool IsEditMode = false;
        private ToolTip ToolTip;
        private ContextMenuStrip ContextMenuStrip;



        public enum Status
        {
            Created,
            Deleted,
            Updated,
            Skipped,
            Failed
        }
        public MainForm()
        {
            InitializeComponent();
            Init();
            SetupButtonImages();
            SetupMenuItems();
            SetupData();
        }
        private void Init()
        {
            _imageManager = new ImageManager(BasePath);
            CodeFileTextBox.Text = FileManager.CodeFile;
            ConfigFileTextBox.Text = FileManager.ConfigFile;
            BackupPathTextBox.Text = FileManager.BackupFilePath;
            this.Icon = _imageManager.LogoIcon;
            LogoPictureBox.Image = _imageManager.LogoImage;
        }
        // Set Up
        private void SetupButtonImages()
        {
            ToolTip = new ToolTip();
            _imageManager.SetButtonImage(ExpandAllButton, "expand", _imageManager.ExpandImageList,false);
            _imageManager.SetButtonImage(ModeButton, "edit-off", _imageManager.EditModeImageList, false);
            ToolTip.SetToolTip(ModeButton, "Current View: Read-Only, Click to Edit");

            _imageManager.SetButtonImage(CodeBrowseButton, "browse", _imageManager.ButtonImageList, false);
            ToolTip.SetToolTip(CodeBrowseButton, "Browse other code files");

            _imageManager.SetButtonImage(ConfigBrowseButton, "browse", _imageManager.ButtonImageList, false);
            ToolTip.SetToolTip(ConfigBrowseButton, "Browse other config files");

            _imageManager.SetButtonImage(BackupBrowseButton, "browse", _imageManager.ButtonImageList, false);
            ToolTip.SetToolTip(BackupBrowseButton, "Change backup file path");

            _imageManager.SetButtonImage(FixAllButton, "fix", _imageManager.ButtonImageList);
            ToolTip.SetToolTip(FixAllButton, "Fix All Errors");

            _imageManager.SetButtonImage(SaveAsButton, "save-as", _imageManager.ButtonImageList);
            ToolTip.SetToolTip(SaveAsButton, "Save As");

            _imageManager.SetButtonImage(ResetButton, "reset", _imageManager.ButtonImageList);
            ToolTip.SetToolTip(ResetButton, "Reset");
        }
        private void SetupMenuItems()
        {
            ContextMenuStrip = new ContextMenuStrip();
            var addChildMenuItem = new ToolStripMenuItem("Add Child");
            var addRowMenuItem = new ToolStripMenuItem("Add Row");
            var deleteRowMenuItem = new ToolStripMenuItem("Delete Row");
            ContextMenuStrip.Items.AddRange(new ToolStripItem[] { addChildMenuItem, addRowMenuItem, deleteRowMenuItem });
            addChildMenuItem.Click += AddChildMenuItem_Click;
            addRowMenuItem.Click += AddRowMenuItem_Click;
            deleteRowMenuItem.Click += DeleteRowMenuItem_Click;
        }

        private void SetupData()
        {
            // ConfigValidator.LoadYamlFile(_fileManager.ConfigFile);
            CsVariables = ConfigValidator.ExtractCsVariables();
            YmlVariables = ConfigValidator.ExtractYmlVariables();
            ResultVariables = ConfigValidator.CompareVariables(CsVariables, YmlVariables);
            AddVariablesToDataGridView(ResultVariables, VariableDataTreeListView);
            SetResultPicture();
        }

        // Form Load
        private void Form1_Load(object sender, EventArgs e)
        {
            VariableDataTreeListView.CellEditStarting += DataTreeListView_CellEditStarting;
            VariableDataTreeListView.CellEditFinishing += DataTreeListView_CellEditFinishing;
        }
        private void AddVariablesToDataGridView(List<ConfigVariable> variables, DataTreeListView objectListView)
        {
            objectListView.SmallImageList = _imageManager.CommandImageList;
            objectListView.SetObjects(variables);
            objectListView.CanExpandGetter = x => ((ConfigVariable)x).HasChildren();
            objectListView.ChildrenGetter = x => ((ConfigVariable)x).Children;

            if (objectListView.AllColumns.Count == 0)
            {
                OLVColumn nameColumn = new OLVColumn("Name", "Name")
                {
                    AspectName = "Name",
                    Width = 300,
                    AspectGetter = delegate (object rowObject)
                    {
                        var ConfigVariable = (ConfigVariable)rowObject;
                        return ConfigVariable.Name;
                    },
                    ImageGetter = delegate (object rowObject)
                    {
                        var ConfigVariable = (ConfigVariable)rowObject;
                        if (ConfigVariable.Result == Result.OnlyInYml)
                        {
                            return "minus";
                        }
                        else if (ConfigVariable.Result == Result.OnlyInCs)
                        {
                            return "plus";
                        }
                        else if (ConfigVariable.Result == Result.WrongValue)
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
            var ConfigVariable = (ConfigVariable)e.Model;

            if (ConfigVariable.Result == Result.OnlyInCs)
            {
                e.Item.BackColor = Color.PaleGreen;

            }
            else if (ConfigVariable.Result == Result.OnlyInYml)
            {
                e.Item.BackColor = Color.LightPink;
            }
            else if (ConfigVariable.Result == Result.WrongValue)
            {
                e.Item.BackColor = Color.Gold;
            }
            else if (ConfigVariable.Result == Result.NoChild)
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
            var selectedObject = VariableDataTreeListView.GetModelObject(e.RowIndex) as ConfigVariable;
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

        // Edit cells
        private void AddChildMenuItem_Click(object sender, EventArgs e)
        {
            var selectedObject = VariableDataTreeListView.SelectedObject as ConfigVariable;
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
                        List<ConfigVariable> newObjects = objectCreater.CreatedVariables;
                        for (int i = newObjects.Count - 1; i >= 0; --i)
                        {
                            Status status = Status.Created;
                            var success = ConfigValidator.InsertChildToVariable(selectedObject, newObjects[i]);
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
                            MakeVariableLog(newObjects[i].FullName, newObjects[i].Value.ToString(), "", status);
                        }
                        VariableDataTreeListView.SelectedIndex = selectedObject.Children.Count + newObjects.Count;
                        RefreshResult();
                    }
                }
            }
        }
        private void AddRowMenuItem_Click(object sender, EventArgs e)
        {
            var selectedObject = VariableDataTreeListView.SelectedObject as ConfigVariable;
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
                    List<ConfigVariable> newObjects = objectCreater.CreatedVariables;
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
                        MakeVariableLog(newObjects[i].FullName, newObjects[i].Value.ToString(), "", status);
                    }
                    VariableDataTreeListView.SelectedIndex = selectedIndex + newObjects.Count;
                    RefreshResult();
                }
            }
        }
        private void DeleteRowMenuItem_Click(object sender, EventArgs e)
        {
            var selectedObject = VariableDataTreeListView.SelectedObject as ConfigVariable;
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
                MakeVariableLog(selectedObject.FullName, "", "", status);
            }
        }
        private void DataTreeListView_CellOver(object sender, CellOverEventArgs e)
        {
            if (e.Model == null || e.SubItem == null)
                return;

            var ConfigVariable = (ConfigVariable)e.Model;
            if (ConfigVariable.Result != Result.Ok && ConfigVariable.Result != Result.NoChild)
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

            var ConfigVariable = (ConfigVariable)e.Model;
            var oldValue = ConfigVariable.Value;
            if (ConfigVariable.Result != Result.Ok && ConfigVariable.Result != Result.NoChild)
            {
                Status status = FixError(ConfigVariable);
                if (status != Status.Failed && status != Status.Skipped)
                {
                    ConfigValidator.RemoveVariableFromErrorList(ConfigVariable.FullName);
                    ConfigVariable.Result = Result.Ok;
                    SetResultPicture();
                }
            }
        }
        private void DataTreeListView_CellEditStarting(object sender, CellEditEventArgs e)
        {
            ConfigVariable ConfigVariable = (ConfigVariable)e.RowObject;

            if (ConfigVariable.TypeName != typeof(Dictionary<,>).Name && ConfigVariable.TypeName != typeof(List<>).Name)
            {
                e.Cancel = false;
                e.Control.Bounds = e.CellBounds;
                e.Control.Width = e.CellBounds.Width;
                e.Control.Height = e.CellBounds.Height;

                if (ConfigVariable.Type == typeof(bool))
                {
                    CheckBox cb = new CheckBox();
                    cb.Bounds = e.CellBounds;
                    cb.Checked = ConfigVariable.Value.ToString() == "True" ? true : false;
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
            ConfigVariable variable = (ConfigVariable)e.RowObject;

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
                        MakeVariableLog(variable.FullName, e.NewValue.ToString(), e.Value.ToString(), Status.Updated);
                    }
                }
            }
        }

        // Fix Error
        private Status FixError(ConfigVariable ConfigVariable)
        {
            Status status = Status.Failed;
            var oldValue = ConfigVariable.Value;
            switch (ConfigVariable.Result)
            {
                case Result.OnlyInCs:
                    if (ConfigValidator.AddVariable(ConfigVariable))
                    {
                        status = Status.Created;
                    }
                    break;
                case Result.OnlyInYml:
                    if (ConfigValidator.RemoveVariable(ConfigVariable))
                    {
                        VariableDataTreeListView.RemoveObject(ConfigVariable);
                        status = Status.Deleted;
                    }
                    break;
                case Result.WrongValue:
                    if (ConfigVariable.HasChildren())
                    {
                        ConfigVariable.Children.Clear();
                        VariableDataTreeListView.RemoveObjects(ConfigVariable.Children);
                    }
                    if (ConfigValidator.ModifyChildFromVariable(ConfigVariable))
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
            MakeVariableLog(ConfigVariable.FullName, ConfigVariable.Value.ToString(), oldValue.ToString(), status);
            return status;
        }

        // Toggle 
        private void ModeButton_Click(object sender, EventArgs e)
        {
            if (IsEditMode)
            {
                IsEditMode = false;
                ModeButton.Image = _imageManager.EditModeImageList.Images["edit-off"];
                ToolTip.SetToolTip(ModeButton, "Current View: Read View, Click to Edit View");
            }
            else
            {
                IsEditMode = true;
                ModeButton.Image = _imageManager.EditModeImageList.Images["edit-on"];
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
                ExpandAllButton.Image = _imageManager.ExpandImageList.Images["expand"];
                IsExpanded = false;
            }
            else
            {
                VariableDataTreeListView.ExpandAll();
                ExpandAllButton.Image = _imageManager.ExpandImageList.Images["collapse"];
                IsExpanded = true;
            }
        }

        // Control Buttons
        private void ResetButton_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Do you really want to reset?");
            if (result == DialogResult.OK)
            {
                YmlVariables.Clear();
                ConfigValidator.ClearAllData();
                YmlVariables = ConfigValidator.ExtractYmlVariables();
                var compareResult = ConfigValidator.CompareVariables(CsVariables, YmlVariables);
                AddVariablesToDataGridView(compareResult, VariableDataTreeListView);
                SetResultPicture();
                //MakeResetLog(_fileManager.ConfigFile);
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
                YamlMappingNode root = ConfigValidator.ConvertYamlFromCode();
                //_fileManager.Save(root);
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
                    else if (status != Status.Skipped)
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
            if (ConfigValidator.HasErrors())
            {
                resultPictureBox.Image = _imageManager.ResultImageList.Images[1];
            }
            else
            {
                resultPictureBox.Image = _imageManager.ResultImageList.Images[0];
            }
        }
        private void NextButton_Click(object sender, EventArgs e)
        {
            var compareResult = ConfigValidator.CompareVariables(CsVariables, YmlVariables);
            AddVariablesToDataGridView(compareResult, VariableDataTreeListView);
            if (ConfigValidator.HasErrors())
            {
                MessageBox.Show("There is an error.");
                SetResultPicture();
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
                            //_fileManager.MakeBackup();
                        }
                        YamlMappingNode rootDocument = ConfigValidator.ConvertYamlFromCode();
                        //_fileManager.Save(rootDocument);
                        this.Close();
                    }
                }

            }
        }
        private void CancelButton_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        
        
        private void CodeBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "C# files (*.cs)|*.cs";
                openFileDialog.Title = "Select a Code file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    /*var oldFile = _fileManager.ConfigFile;
                    var newFile = openFileDialog.FileName; 
                    _fileManager.SetCodeFilePath(openFileDialog.FileName, CodeFileTextBox);*/
                   // MakeFileLog("Code", oldFile,newFile);
                    ConfigValidator.ClearAllData();
                    ConfigValidator.Init();
                    SetupData();
                }
            }

        }

        private void ConfigBrowseButton_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "YAML files (*.yml)|*.yml";
                openFileDialog.Title = "Select a Config file";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    /*var oldFile = _fileManager.ConfigFile;
                    var newFile = openFileDialog.FileName;
                    _fileManager.SetCodeFilePath(openFileDialog.FileName, CodeFileTextBox);
                    MakeFileLog("Code", oldFile, newFile);*/
                    ConfigValidator.ClearAllData();
                    ConfigValidator.Init();
                    SetupData();
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
                    /*var oldFolder = _fileManager.ConfigFile;
                    var newFolder = folderBrowserDialog.SelectedPath;
                    _fileManager.SetBackupPath(folderBrowserDialog.SelectedPath, BackupPathTextBox);*/
                    //MakeFileLog("Backup path", oldFolder, newFolder);
                }
            }
        }

        private void MakeResetLog(string fileName)
        {
            StringBuilder logMessage = new StringBuilder($"[{DateTime.Now}]: ").Append($"{fileName} has been reset.");
            LogListBox.Items.Add(logMessage);
            LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
        }
        private void MakeFileLog(string fileType, string oldFile, string newFile)
        {
            StringBuilder logMessage = new StringBuilder($"[{DateTime.Now}]: ").Append($"{fileType} has been changed. {oldFile} -> {newFile}");
            LogListBox.Items.Add(logMessage);
            LogListBox.SelectedIndex = LogListBox.Items.Count - 1;
        }
        private void MakeVariableLog(string fullname, string newValue, string oldValue, Status status)
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
            else if (status == Status.Skipped)
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


