﻿using ERHMS.EpiInfo;
using System;
using System.IO;
using System.Reflection;
using System.ServiceModel;
using System.Threading;
using System.Windows;

namespace ERHMS.Sandbox
{
    public partial class App : Application
    {
        public static string Name
        {
            get { return Assembly.GetExecutingAssembly().GetName().Name; }
        }

        [STAThread]
        public static void Main(string[] args)
        {
            bool created;
            using (Mutex mutex = new Mutex(true, string.Format("Global\\{0}", Name), out created))
            {
                if (!created)
                {
                    MessageBox.Show("An instance of this application is already running.");
                    return;
                }
                App app = new App();
                app.InitializeComponent();
                app.Run(new MainWindow());
            }
        }

        private Service service;
        private ServiceHost host = null;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            Log.Current.Debug("Starting up");
            string desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
            DirectoryInfo root = new DirectoryInfo(Path.Combine(desktopPath, Name));
            ConfigurationExtensions.CreateAndOrLoad(root);
            service = new Service();
            service.SayingHello += (sender, _e) =>
            {
                MessageBox.Show(string.Format("Hello, {0}", _e.Name));
            };
            try
            {
                host = service.OpenHost();
            }
            catch { }
        }

        protected override void OnExit(ExitEventArgs e)
        {
            Log.Current.Debug("Exiting");
            if (host != null)
            {
                host.Close();
            }
            base.OnExit(e);
        }
    }
}
