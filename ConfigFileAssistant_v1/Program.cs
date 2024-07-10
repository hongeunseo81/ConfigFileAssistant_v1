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
           Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());

            var filePath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
            var csVariables = ConfigValidator.ExtractCsVariables();
            var ymlVariables = ConfigValidator.ExtractYmlVariables(filePath);
            //ConfigValidator.CompareVariables(csVariables, ymlVariables);
            

        }
    }
}
