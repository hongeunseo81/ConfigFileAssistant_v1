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
        public static string s_basePath;
        public  static string s_configFile { get; set; }
        public static string s_backupFilePath { get; set; }
        public static object s_config { get; set; }
        public static YamlMappingNode s_root { get; set; }
        public static void Init(string basePath, Object config)
        {
            string directoryPath = Path.GetDirectoryName(basePath);
            s_configFile = Path.Combine(directoryPath, "config.yml");
            s_backupFilePath = Path.Combine(directoryPath, "configbackup");
            s_config = config;
            s_root = LoadYamlFile();
        }
        
        public static YamlMappingNode LoadYamlFile()
        {
            YamlStream Yaml = new YamlStream();
            using (var reader = new StreamReader(s_configFile))
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
            s_configFile = filePath;
            filePathTextBox.Text = filePath;
            s_root = LoadYamlFile();
        }
        public static void SetBackupPath(string backupFolder, TextBox backupPathTextBox)
        {
            s_backupFilePath = backupFolder;
            backupPathTextBox.Text = backupFolder;
        }
        public static void MakeBackup()
        {
            string backupFolder = Path.Combine(s_backupFilePath, "configbackup");
            Directory.CreateDirectory(backupFolder);
            string backupFilePath = Path.Combine(backupFolder, $"config_{DateTime.Now:yyyyMMddHHmmss}.yml");
            File.Copy(s_configFile, backupFilePath, true);
        }
        public static void Save(YamlMappingNode root, string filepath) 
        {
            YamlStream yaml = new YamlStream();
            yaml.Documents.Add(new YamlDocument(root));
            filepath = filepath == null ? s_configFile : filepath;
            using (var writer = new StreamWriter(filepath))
            {
                yaml.Save(writer, false);
            }
        }
    }

}
