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
            var variables1 = ConfigValidator.ExtractCsVariables();
            var filepath = "C:/Users/HONGEUNSEO/source/repos/ConfigFileAssistant_v1/ConfigFileAssistant_v1/bin/Debug/config.yml";
            var variables2 = ConfigValidator.ExtractYmlVariables(filepath);

            /*Debug.WriteLine("====================CS==================");
            foreach (var variable in variables1)
            {
                Debug.WriteLine($"{variable.Key}: {variable.Value}");
            }
            Debug.WriteLine("===================YML===================");
            foreach (var variable in variables2) 
            {
                Debug.WriteLine($"{variable.Key}: {variable.Value.Type}");
            }*/
        }
    }
}
