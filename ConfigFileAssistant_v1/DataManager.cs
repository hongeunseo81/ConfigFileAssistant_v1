using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigFileAssistant_v1
{
    public class DataManager
    {
        private readonly ConfigManager _configManager;
        private readonly DataGridView _dataGridView;

        public DataManager(ConfigManager configManager, DataGridView dataGridView)
        {
            _configManager = configManager;
            _dataGridView = dataGridView;
        }

        public void SetupData()
        {
            FilePathTextBox.Text = _configManager.BackupFilePath;
            CodePathTextBox.Text = _configManager.FilePath;
            ConfigValidator.LoadYamlFile(_configManager.FilePath);
            var csVariables = ConfigValidator.ExtractCsVariables();
            var ymlVariables = ConfigValidator.ExtractYmlVariables();
            var resultVariables = ConfigValidator.CompareVariables(csVariables, ymlVariables);
            AddVariablesToDataGridView(resultVariables, _dataGridView);
            SetResultPicture();
        }
    }

}
