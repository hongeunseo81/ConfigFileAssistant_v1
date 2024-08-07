﻿using ConfigFileAssistant.Manager;
using ConfigTypeFinder;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using WheelHetergeneousInspectionSystem.Models;

namespace ConfigFileAssistant
{
    internal static class Program
    {
        /// <summary>
        /// 해당 애플리케이션의 주 진입점입니다.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Config config = new Config();
            FileHandler.Init(Assembly.GetExecutingAssembly().Location, config);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainForm());
        }
    }
}
