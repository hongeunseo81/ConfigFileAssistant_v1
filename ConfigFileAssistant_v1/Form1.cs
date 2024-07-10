using BrightIdeasSoftware;
using CalibrationTool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace ConfigFileAssistant_v1
{
    public partial class MainForm : Form
    {
        private string initFilePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
        private string filePath;
        private List<VariableInfo> csVariables;
        private List<VariableInfo> ymlVariables;
        private Config conf;

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
            csVariables = ConfigValidator.ExtractCsVariables();
            ymlVariables = ConfigValidator.ExtractYmlVariables(filePath);
            var result = ConfigValidator.CompareVariables(csVariables, ymlVariables);
            AddVariablesToDataGridView(result, ymlDataTreeListView);
        }
        
        private static void AddVariablesToDataGridView(List<VariableInfo> variables, DataTreeListView objectListView)
        {
            objectListView.SetObjects(variables);
            objectListView.CanExpandGetter = x => ((VariableInfo)x).HasChildren();
            objectListView.ChildrenGetter = x => ((VariableInfo)x).Children;

            objectListView.AllColumns.Add(new OLVColumn("Name", "Name") { AspectName = "Name", Width = 300 });
            objectListView.AllColumns.Add(new OLVColumn("Type", "Type") { AspectName = "TypeName", Width = 300 });
            objectListView.AllColumns.Add(new OLVColumn("Value", "Value") { AspectName = "Value", Width = 300 });

            OLVColumn noteColumn = new OLVColumn("Note", "Note")
            {
                AspectName = "Note",
                Width = 300,
                AspectToStringConverter = delegate (object x)
                {
                    return x.ToString();
                },
            };
           
            objectListView.AllColumns.Add(noteColumn);
            
            objectListView.UseCellFormatEvents = true;
            
            objectListView.FormatCell += (sender, e) =>
            {
                var variableInfo = (VariableInfo)e.Model;
               
                if (e.ColumnIndex == noteColumn.Index)
                {
                    if (variableInfo.Note == Note.OK)
                    {
                        e.SubItem.BackColor = Color.Green;
                        e.SubItem.ForeColor = Color.White;
                    }
                    else if(variableInfo.Note == Note.ERROR)
                    {
                        e.SubItem.BackColor = Color.Gray;
                        e.SubItem.ForeColor = Color.White;
                    }
                    else
                    {
                        e.SubItem.BackColor = Color.Red;
                        e.SubItem.ForeColor = Color.White;
                    }
                }
            };
            objectListView.RebuildColumns();
        }
        
    }
}