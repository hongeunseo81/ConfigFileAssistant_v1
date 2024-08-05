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
        public string FilePath { get; set; }
        public string BackupFilePath { get; set; }
        public string CodePath { get; set; }

        public FileManager(string basePath, TextBox filePathTextBox, TextBox backupPathTextBox)
        {
            FilePath = Path.Combine(basePath, "config.yml");
            BackupFilePath = Path.Combine(basePath, "");
            filePathTextBox.Text = FilePath;
            backupPathTextBox.Text = BackupFilePath;
        }
        public void SetFilePath(string filePath, TextBox filePathTextBox)
        {
            FilePath = filePath;
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
            File.Copy(FilePath, backupFilePath, true);
        }
        public void Save(YamlMappingNode root) 
        {
            YamlStream yaml = new YamlStream();
            yaml.Documents.Add(new YamlDocument(root));
            using (var writer = new StreamWriter(FilePath))
            {
                yaml.Save(writer, false);
            }
        }
    }

}
