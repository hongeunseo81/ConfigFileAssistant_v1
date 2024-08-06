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
        public static string CodeFile { get; set; }
        public  static string ConfigFile { get; set; }
        public static string BackupFilePath { get; set; }

        public static void Init(string basePath)
        {
            string parentDirectoryPath = Directory.GetParent(basePath)?.Parent?.FullName;
            CodeFile = Path.Combine(parentDirectoryPath, "Config.cs");
            ConfigFile = Path.Combine(Path.GetDirectoryName(basePath),"config.yml");
            BackupFilePath = Path.Combine(Path.GetDirectoryName(basePath), "configbackup");
        }
        
        public static void SetCodeFilePath(string filePath, TextBox filePathTextBox)
        {
            CodeFile = filePath;
            filePathTextBox.Text = filePath;
        }
        public static void SetConfigFilePath(string filePath, TextBox filePathTextBox)
        {
            ConfigFile = filePath;
            filePathTextBox.Text = filePath;
        }
        public static void SetBackupPath(string backupFolder, TextBox backupPathTextBox)
        {
            BackupFilePath = backupFolder;
            backupPathTextBox.Text = backupFolder;
        }
        public static void MakeBackup()
        {
            string backupFolder = Path.Combine(BackupFilePath, "configbackup");
            Directory.CreateDirectory(backupFolder);
            string backupFilePath = Path.Combine(backupFolder, $"config_{DateTime.Now:yyyyMMddHHmmss}.yml");
            File.Copy(ConfigFile, backupFilePath, true);
        }
        public static void Save(YamlMappingNode root) 
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
