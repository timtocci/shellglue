using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;
using SkySoftware.EZShellExtensions;
using System.IO;
using System.Reflection;
using System.Globalization;
using System.Configuration;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Collections.ObjectModel;

namespace GlueContextMenuExtension
{
    [Serializable]
    public class ActionItem
    {
        private string _Name;

        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        private string _Verb;

        public string Verb
        {
            get
            {
                return _Verb;
            }
            set
            {
                _Verb = value;
            }
        }

        private string _Help;

        public string Help
        {
            get
            {
                return _Help;
            }
            set
            {
                _Help = value;
            }
        }

        private string _ProgramPath;

        public string ProgramPath
        {
            get
            {
                return _ProgramPath;
            }
            set
            {
                _ProgramPath = value;
            }
        }

        private string[] _ExtentionFilter;
        public string[] ExtentionFilter
        {
            get { return _ExtentionFilter; }
            set
            {
                _ExtentionFilter = value;
            }
        }

        private ActionItemList _Actions;

        public ActionItemList Actions
        {
            get
            {
                return _Actions;
            }
            set
            {
                _Actions = value;
            }
        }

        private string _IconFilePath;
        public string IconFilePath
        {
            get { return _IconFilePath; }
            set
            {
                _IconFilePath = value;
            }
        }

        public void AddMenuItems(ref ShellMenuItem parentMenuItem, string targetFolder, string[] targetFiles)
        {
            if (this.ExtentionFilter != null && this.ExtentionFilter.Length > 0)
            {
                foreach (string TargetFile in targetFiles)
                {
                    Boolean FoundMatch = false;
                    foreach (string Filter in this.ExtentionFilter)                    
                    {
                        if (!Regex.IsMatch(TargetFile, Filter))
                            FoundMatch = false;
                        else
                        {
                            FoundMatch = true;
                            break;
                        }
                    }
                    if (!FoundMatch)
                        return;
                }
            }

            ShellMenuItem MenuItem;

            if (!String.IsNullOrEmpty(this.Verb) && !String.IsNullOrEmpty(this.Help))
                MenuItem = parentMenuItem.SubMenu.AddItem(this.Name, this.Verb, this.Help);
            else
                MenuItem = parentMenuItem.SubMenu.AddItem(this.Name);
            
            if (!String.IsNullOrEmpty(this.IconFilePath) && File.Exists(this.IconFilePath))
            {
                MenuItem.OwnerDraw = true;
            }

            if (this.Actions != null && this.Actions.Count > 0)
            {
                MenuItem.HasSubMenu = true;
                this.Actions.AddMenuItems(ref MenuItem, targetFolder, targetFiles);
            }
        }

        public void Execute(string targetFolder, string[] targetFiles)
        {
            ProcessStartInfo ProcInfo = new ProcessStartInfo();
            ProcInfo.FileName = this.ProgramPath;
            ProcInfo.CreateNoWindow = false;
            ProcInfo.UseShellExecute = false;

            StringBuilder ArgBuilder = new StringBuilder();

            if (!String.IsNullOrEmpty(targetFolder))
                ArgBuilder.Append(string.Format("\"{0}\" ", targetFolder));

            if (targetFiles != null && targetFiles.Length > 0)
            {
                string TempFilePath = Path.GetTempFileName();
                using (TextWriter Writer = File.CreateText(TempFilePath))
                {
                    foreach (string FilePath in targetFiles)
                        Writer.WriteLine(FilePath);
                }
                ArgBuilder.Append(string.Format("\"{0}\" ", TempFilePath));
            }

            ProcInfo.Arguments = ArgBuilder.ToString();

            System.Diagnostics.Process process = new Process();
            process.StartInfo = ProcInfo;
            process.Start();
        }

        public string GetKey()
        {
            string verb = !string.IsNullOrEmpty(this.Verb) ? this.Verb : this.Name;
            string help = !string.IsNullOrEmpty(this.Help) ? this.Help : this.Name;
            return string.Format("{0}:{1}:{2}", this.Name, verb, help);
        }

        public static string GetKey(ShellMenuItem menu)
        {
            return string.Format("{0}:{1}:{2}", menu.Caption, menu.Verb, menu.HelpString);
        }
    }
}
