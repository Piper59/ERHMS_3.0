﻿using ERHMS.Utility;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace ERHMS.EpiInfo.Wrappers
{
    public partial class Wrapper
    {
        protected static TextReader In { get; private set; }
        protected static TextWriter Out { get; private set; }
        private static TextWriter Error { get; set; }

        protected static void MainBase(Type type, string[] args)
        {
            try
            {
                Log.Logger.Debug("Starting up");
                Application.ThreadException += (sender, e) =>
                {
                    HandleError(e.Exception);
                };
                Application.EnableVisualStyles();
                ConfigurationExtensions.Load();
                In = Console.In;
                Out = Console.Out;
                Error = Console.Error;
                Console.SetIn(new StreamReader(Stream.Null));
                Console.SetOut(new StreamWriter(Stream.Null));
                Console.SetError(new StreamWriter(Stream.Null));
                MethodInfo method = type.GetMethod(args[0], BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                ParameterInfo parameter = method.GetParameters().FirstOrDefault();
                if (parameter == null)
                {
                    method.Invoke(null, null);
                }
                else
                {
                    method.Invoke(null, new object[] { WrapperArgsBase.Parse(parameter.ParameterType, In.ReadLine()) });
                }
                Log.Logger.Debug("Exiting");
            }
            catch (Exception ex)
            {
                HandleError(ex);
            }
        }

        protected static void HandleError(Exception ex)
        {
            Log.Logger.Fatal("Fatal error", ex);
            MessageBox.Show("Epi Info\u2122 encountered an error and must shut down.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        protected static void RaiseEvent(WrapperEventType type, object properties = null)
        {
            string line;
            if (properties == null)
            {
                line = type.ToString();
            }
            else
            {
                QueryString queryString = new QueryString();
                foreach (PropertyInfo property in properties.GetType().GetProperties())
                {
                    queryString.Set(property.Name, property.GetValue(properties, null));
                }
                line = string.Format("{0} {1}", type, queryString);
            }
            Log.Logger.DebugFormat("Raising event: {0}", line);
            Error.WriteLine(line);
        }
    }
}
