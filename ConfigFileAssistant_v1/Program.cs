using ConfigTypeFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ConfigFileAssistant_v1
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            ConfigValidator.Init();
            TypeHandler.init();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
            /*var types = TypeHandler.GetAllTypes();
            foreach (var key in types.Keys) 
            {
                Debug.WriteLine(key +" - " + types[key]);
            }*/
            /*var filePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
            ConfigValidator.Init();
            ConfigValidator.LoadYamlFile(filePath);
            var csVariables = ConfigValidator.ExtractCsVariables();
            var ymlVariables = ConfigValidator.ExtractYmlVariables();
            ConfigValidator.CompareVariables(csVariables, ymlVariables);
            */
            // ConfigValidator.MigrateVariables(csVariables, result, filePath);

        }
    }
}
