using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using YamlDotNet.RepresentationModel;

namespace ConfigFileAssistant.Manager
{
    public class FileHandler
    {
        public static string BasePath;
        public  static string ConfigFile { get; set; }
        public static string BackupFilePath { get; set; }
        public static object Config { get; set; }
        public static YamlMappingNode Root { get; set; }
        public static void Init(string basePath, Object config)
        {
            string directoryPath = Path.GetDirectoryName(basePath);
            ConfigFile = Path.Combine(directoryPath, "config.yml");
            BackupFilePath = Path.Combine(directoryPath, "configbackup");
            Config = config;
            Root = LoadYamlFile();
        }
        
        public static YamlMappingNode LoadYamlFile()
        {
            YamlStream Yaml = new YamlStream();
            using (var reader = new StreamReader(ConfigFile))
            {
                Yaml.Load(reader);
            }
            if (Yaml.Documents.Count != 0)
            {
                return (YamlMappingNode)Yaml.Documents[0].RootNode;
            }
            return new YamlMappingNode();
        }

        public static void SetConfigFilePath(string filePath, TextBox filePathTextBox)
        {
            ConfigFile = filePath;
            filePathTextBox.Text = filePath;
            Root = LoadYamlFile();
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
        public static void Save(YamlMappingNode root, string filepath) 
        {
            YamlStream yaml = new YamlStream();
            yaml.Documents.Add(new YamlDocument(root));
            filepath = filepath == null ? ConfigFile : filepath;
            using (var writer = new StreamWriter(filepath))
            {
                yaml.Save(writer, false);
            }
        }
    }

}
