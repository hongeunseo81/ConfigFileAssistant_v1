using BrightIdeasSoftware;
using CalibrationTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YamlDotNet.Serialization;
namespace ConfigFileAssistant_v1
{
    public partial class MainForm : Form
    {
        private string initFilePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
        private string filePath;

        private Config conf;
        static private ISerializer _serializer = new SerializerBuilder().Build();


        public MainForm()
        {
            InitializeComponent();
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
            var csVariables = ConfigValidator.ExtractCsVariables();
            var ymlVariables = ConfigValidator.ExtractYmlVariables(filePath);
            AddCsVariablesToTreeView(csVariables,treeView);
            AddYmlYariablesToDataGridView(ymlVariables, dataTreeListView1);
        }
        private static void AddCsVariablesToTreeView(Dictionary<string, string> variables, TreeView treeView)
        {
            treeView.BeginUpdate();
            treeView.Nodes.Clear();

            foreach (var kvp in variables)
            {
                string[] parts = kvp.Key.Split('.');
                TreeNode currentNode = null;
            foreach (var part in parts)
            {
                if (currentNode == null)
                {
                    if (treeView.Nodes.ContainsKey(part))
                    {
                        currentNode = treeView.Nodes[part];
                    }
                    else
                    {
                        currentNode = treeView.Nodes.Add(part, part);
                    }
                }
                else
                {
                    if (currentNode.Nodes.ContainsKey(part))
                    {
                        currentNode = currentNode.Nodes[part];
                    }
                        else
                        {
                        currentNode = currentNode.Nodes.Add(part, part);
                        }
                    }
                }
                if (currentNode != null)
                {
                    currentNode.Tag = kvp.Value;
                }
            }

            treeView.EndUpdate();
        }
        private static void AddYmlYariablesToDataGridView(List<VariableInfo> variables, DataTreeListView objectListView)
        {
            objectListView.SetObjects(variables);

            objectListView.DrawColumnHeader += ObjectListView_DrawColumnHeader;
            objectListView.CanExpandGetter = x => ((VariableInfo)x).HasChildren();
            objectListView.ChildrenGetter = x => ((VariableInfo)x).Children;

            objectListView.AllColumns.Add(new OLVColumn("Name", "Name") { AspectName = "Name", Width = 300 });
            objectListView.AllColumns.Add(new OLVColumn("Type", "Type") { AspectName = "Type", Width = 300 });
            objectListView.AllColumns.Add(new OLVColumn("Value", "Value") { AspectName = "Value", Width = 300 });


            // 컬럼 활성화
            objectListView.RebuildColumns();

        }

        private static void ObjectListView_DrawColumnHeader(object sender, DrawListViewColumnHeaderEventArgs e)
        {
            using (SolidBrush backBrush = new SolidBrush(Color.LightBlue)) // 헤더 배경색
            {
                e.Graphics.FillRectangle(backBrush, e.Bounds);
            }

            TextRenderer.DrawText(e.Graphics, e.Header.Text, e.Font, e.Bounds, Color.Black, TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);

        }
    }
}
