using BrightIdeasSoftware;
using CalibrationTool;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
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
            // var result = ConfigValidator.CompareVariables(csVariables, ymlVariables);
            AddVariablesToDataGridView(csVariables, csDataTreeView);
            AddVariablesToDataGridView(ymlVariables, ymlDataTreeListView);
        }
        
        private static void AddVariablesToDataGridView(List<VariableInfo> variables, DataTreeListView objectListView)
        {
            objectListView.SetObjects(variables);
            objectListView.CanExpandGetter = x => ((VariableInfo)x).HasChildren();
            objectListView.ChildrenGetter = x => ((VariableInfo)x).Children;

            objectListView.AllColumns.Add(new OLVColumn("Name", "Name") { AspectName = "Name", Width = 300 });
            objectListView.AllColumns.Add(new OLVColumn("Type", "Type") { AspectName = "TypeName", Width = 300 });
            objectListView.AllColumns.Add(new OLVColumn("Value", "Value") { AspectName = "Value", Width = 300 });


            // 컬럼 활성화
            objectListView.RebuildColumns();

        }
   }
}