using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConfigFileAssistant_v1
{
    public class ConfigManager
    {
        public string BackupFilePath { get; }
        public string FilePath { get; }

        public ConfigManager(string basePath)
        {
            BackupFilePath = Path.Combine(basePath, "bin/Debug/");
            FilePath = Path.Combine(basePath, "bin/Debug/config.yml");
        }
    }

}
