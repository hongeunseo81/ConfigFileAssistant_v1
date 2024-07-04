using CalibrationTool;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using YamlDotNet.Serialization;
using System.IO;
using ConfigFileAssistant_v1;
using System.Drawing;

namespace ConfigFileAssistant_v1
{
    public partial class MainForm : Form
    {
        private string initFilePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
        private string filePath;
        
        private List <Variable> variables = new List<Variable>();
        private int columnCount;
        private Config conf;
        static private ISerializer _serializer = new SerializerBuilder().Build();
        public MainForm()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            variablesListView.View = View.Details;
            variablesListView.Columns.Add("Name", 150, HorizontalAlignment.Left);
            init();

        }

        private void init()
        {
            filePath = initFilePath;
            filePathTextBox.Text = filePath;
            conf = new Config(); 
            dataGridView.CellFormatting += DataGridView_CellFormatting;
            dataGridView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;  
            dataGridView.ReadOnly = true;
            DataBind();
        }
        private void DataBind ()
        {
            dataGridView.Columns.Clear();
            string csContent = _serializer.Serialize(conf);
            string ymlContent = File.ReadAllText(filePath);
            var csVariables = ConfigValidator.ExtractVariables(csContent);
            var ymlVariables = ConfigValidator.ExtractVariables(ymlContent);
            variables = ConfigValidator.CombineVariables(csVariables, ymlVariables);
            dataGridView.DataSource = variables;
            AddNumberColumn();
            columnCount = dataGridView.ColumnCount;
        }
        private void AddNumberColumn()
        {
            // 순서 컬럼 생성
            DataGridViewTextBoxColumn orderColumn = new DataGridViewTextBoxColumn
            {
                Name = "No",
                HeaderText = "No",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            };

            dataGridView.Columns.Insert(0, orderColumn);

            for (int i = 0; i < dataGridView.Rows.Count; ++i)
            {
                dataGridView.Rows[i].Cells["No"].Value = (i + 1).ToString();
            }
        }

        private void DataGridView_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (dataGridView.Columns[e.ColumnIndex].DataPropertyName == nameof(Variable.Note))
            {
                var variable = dataGridView.Rows[e.RowIndex].DataBoundItem as Variable;
                if (variable != null)
                {
                    switch (variable.Note)
                    {
                        case NoteMessage.YML_ONLY:
                            PaintCell(e,Color.Pink);
                            break;
                        case NoteMessage.CS_ONLY:
                            PaintCell(e, Color.Aquamarine);
                            break;
                        case NoteMessage.TYPE_MISMATCH:
                            PaintCell(e, Color.GreenYellow);
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        private void PaintCell (DataGridViewCellFormattingEventArgs e, Color color)
        {
            for(int i = columnCount-1; i>=0; --i)
            {
                dataGridView.Rows[e.RowIndex].Cells[e.ColumnIndex-i].Style.BackColor = color;
            }
        }

        private void browseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Config Files|*.yml;*.json;*.bin;...";
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                filePath = ofd.FileName;
                filePathTextBox.Text = filePath;
                DataBind();
            }
        }

        private void csOnlyFilter_CheckedChanged(object sender, EventArgs e)
        {
            ShowVariables(NoteMessage.CS_ONLY);
        }

        private void ymlOnlyFilter_CheckedChanged(object sender, EventArgs e)
        {
            ShowVariables(NoteMessage.YML_ONLY);
        }

        private void typeMismatchFilter_CheckedChanged(object sender, EventArgs e)
        {
            ShowVariables(NoteMessage.TYPE_MISMATCH);
        }

        private void okFilter_CheckedChanged(object sender, EventArgs e)
        {
            ShowVariables(NoteMessage.OK);
        }

        private void ShowVariables(NoteMessage noteMessage)
        {
            variablesListView.Items.Clear();
            List<Variable> foundVariables = variables.FindAll(v => v.Note.Equals(noteMessage));
            foreach (Variable v in foundVariables)
            {
                variablesListView.Items.Add(v.Name);
            }
        }

        private void NEXT_Click(object sender, EventArgs e)
        {
            if(!ConfigValidator.isValidatedVariable(variables))
            {
                MessageBox.Show("값이 검증되지 않았습니다. Editor에서 수정하세요.");
            }
            else
            {

            }
        }
    }
}
