using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YamlDotNet.RepresentationModel;

namespace ConfigFileAssistant_v1.Manager
{
    public class FileManager
    {
        public string CodeFile { get; set; }
        public string ConfigFile { get; set; }
        public string BackupFilePath { get; set; }

        public FileManager(string basePath, TextBox filePathTextBox, TextBox backupPathTextBox)
        {
            ConfigFile = Path.Combine(basePath, "config.yml");
            BackupFilePath = Path.Combine(basePath, "");
            filePathTextBox.Text = ConfigFile;
            backupPathTextBox.Text = BackupFilePath;
        }
        public void SetCodeFilePath(string filePath, TextBox filePathTextBox)
        {
            CodeFile = filePath;
            filePathTextBox.Text = filePath;
        }
        public void SetConfigFilePath(string filePath, TextBox filePathTextBox)
        {
            ConfigFile = filePath;
            filePathTextBox.Text = filePath;
        }
        public void SetBackupPath(string backupFolder, TextBox backupPathTextBox)
        {
            BackupFilePath = backupFolder;
            backupPathTextBox.Text = backupFolder;
        }
        public void MakeBackup()
        {
            string backupFolder = Path.Combine(BackupFilePath, "configbackup");
            Directory.CreateDirectory(backupFolder);
            string backupFilePath = Path.Combine(backupFolder, $"config_{DateTime.Now:yyyyMMddHHmmss}.yml");
            File.Copy(ConfigFile, backupFilePath, true);
        }
        public void Save(YamlMappingNode root) 
        {
            YamlStream yaml = new YamlStream();
            yaml.Documents.Add(new YamlDocument(root));
            using (var writer = new StreamWriter(ConfigFile))
            {
                yaml.Save(writer, false);
            }
        }
    }

}
