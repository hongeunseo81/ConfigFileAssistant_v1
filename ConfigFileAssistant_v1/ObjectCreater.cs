using ConfigTypeFinder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigFileAssistant_v1
{
    public partial class ObjectCreater : Form
    {
        private string variablePath; 
        public List<VariableInfo> CreatedVariables { get; private set; }

        public ObjectCreater(string path)
        {
            variablePath = path;
            CreatedVariables = new List<VariableInfo>();
            InitializeComponent();
        }

        private void ObjectCreater_Load(object sender, EventArgs e)
        {
            if(dataGridView != null)
            {
                dataGridView.Columns.Clear();
                DataGridViewTextBoxColumn nameColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Name",
                    HeaderText = "Name",
                    Width = 250,
                };

                var types = TypeHandler.GetAllTypes();
                DataGridViewComboBoxColumn typeColumn = new DataGridViewComboBoxColumn
                {
                    Name = "Type",
                    HeaderText = "Type",
                    Width = 250,
                    DataSource = new BindingSource(types.Keys, null),
                    DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton
                };

                DataGridViewTextBoxColumn valueColumn = new DataGridViewTextBoxColumn
                {
                    Name = "Value",
                    HeaderText = "Value",
                    Width = 250
                };
                dataGridView.Columns.Add(nameColumn);
                dataGridView.Columns.Add(typeColumn);
                dataGridView.Columns.Add(valueColumn);
            }
        }

        private void createButton_Click(object sender, EventArgs e)
        {
            foreach (DataGridViewRow row in dataGridView.Rows)
            {
                if (row.Cells["Name"].Value != null && row.Cells["Type"].Value != null)
                {
                    var value = row.Cells["Value"].Value == null ? string.Empty : row.Cells["Value"].Value.ToString();
                    var variableInfo = new VariableInfo(variablePath, row.Cells["Name"].Value.ToString(), row.Cells["Type"].Value.ToString(),value);
                    TypeHandler.ConvertTypeNameToType(variableInfo);
                    CreatedVariables.Add(variableInfo);
                    this.DialogResult = DialogResult.OK;
                    this.Close();

                }
            }

        }
    }

    public class VariableInfoEventArgs : EventArgs
    {
        public List<VariableInfo> Variables { get; }

        public VariableInfoEventArgs(List<VariableInfo> variables)
        {
            Variables = variables;
        }
    }
}
