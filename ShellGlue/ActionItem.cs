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
using SkySoftware.EZShellExtensions.Wrapped;

namespace ShellGlue
{
    public enum TargetMode
    {
        CommandLine,
        File
    }

    [Serializable]
    public class ActionItem
    {
        #region Properties
        
        private string _TempFilePath;
        
        [XmlIgnoreAttribute]
        public string TempFilePath
        {
            get
            {
                if (_TempFilePath == null)
                    _TempFilePath = Path.GetTempFileName();
                return _TempFilePath;
            }
            set
            {
                _TempFilePath = value;
            }
        }

        private WriterBase _Writer;
        
        [XmlIgnoreAttribute]
        public WriterBase Writer
        {
            get
            {
                if (_Writer == null || this._Writer.IsDisposed)
                    _Writer = new WrappedWriter(TempFilePath);
                return _Writer;
            }
            set
            {
                _Writer = value;
            }
        }

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

        private string _ArgumentsFormat;
        public string ArgumentsFormat
        {
            get { return _ArgumentsFormat; }
            set
            {
                _ArgumentsFormat = value;
            }
        }

        private TargetMode _TargetListMode = TargetMode.CommandLine;
        public TargetMode TargetListMode
        {
            get { return _TargetListMode; }
            set
            {
                _TargetListMode = value;
            }
        }

        #endregion
        
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

        public void WriteTargetsToFile(string targetFolder, string[] targetFiles)
        {
            try
            {
                if (!String.IsNullOrEmpty(targetFolder))
                {
                    Writer.WriteLine(targetFolder);
                }

                if (targetFiles != null && targetFiles.Length > 0)
                {
                    foreach (string FilePath in targetFiles)
                    {
                        Writer.WriteLine(FilePath);
                    }
                }
            }
            finally
            {
                if (this.Writer != null)
                    ((IDisposable)Writer).Dispose();
            }
        }

        public void TargetsToCommandLine(string targetFolder, string[] targetFiles, StringBuilder ArgBuilder)
        {
            if (!String.IsNullOrEmpty(targetFolder))
            {
                ArgBuilder.Append(string.Format("\"{0}\" ", targetFolder));
            }

            if (targetFiles != null && targetFiles.Length > 0)
            {
                foreach (string FilePath in targetFiles)
                {
                    ArgBuilder.Append(string.Format("\"{0}\" ", FilePath));
                }
            }
        }

        public string GetArguments(string targetFolder, string[] targetFiles)
        {
            StringBuilder ArgBuilder = new StringBuilder();

            if (this.TargetListMode == TargetMode.CommandLine)
            {
                this.TargetsToCommandLine(targetFolder, targetFiles, ArgBuilder);
            }
            else if (this.TargetListMode == TargetMode.File)
            {
                this.WriteTargetsToFile(targetFolder, targetFiles);
                ArgBuilder.Append(string.Format("\"{0}\" ", TempFilePath));
            }

            string Arguments;
            if (!String.IsNullOrEmpty(this.ArgumentsFormat))
                Arguments = string.Format(this.ArgumentsFormat, ArgBuilder.ToString());
            else
                Arguments = ArgBuilder.ToString();
            return Arguments;
        }

        public void Execute(string targetFolder, string[] targetFiles)
        {
            ProcessStartInfo ProcInfo = new ProcessStartInfo();
            ProcInfo.FileName = this.ProgramPath;
            ProcInfo.CreateNoWindow = false;
            ProcInfo.UseShellExecute = false;

            string WorkingDirectory = this.GetWorkingDirectory(targetFolder, targetFiles);
            ProcInfo.WorkingDirectory = WorkingDirectory;

            string Arguments = GetArguments(targetFolder, targetFiles);
            ProcInfo.Arguments = Arguments;

            System.Diagnostics.Process process = new Process();
            process.StartInfo = ProcInfo;
            process.Start();
        }

        public string GetWorkingDirectory(string targetFolder, string[] targetFiles)
        {
            if (!String.IsNullOrEmpty(targetFolder))
            {
                return Path.GetDirectoryName(targetFolder);
            }

            if (targetFiles != null && targetFiles.Length > 0)
            {
                return Path.GetDirectoryName(targetFiles[0]);
            }

            throw new InvalidOperationException("WTF: there is no target folder nor target files!");
        }

        public string GetKey()
        {
            string verb = !string.IsNullOrEmpty(this.Verb) ? this.Verb : this.Name;
            string help = !string.IsNullOrEmpty(this.Help) ? this.Help : this.Name;
            return string.Format("{0}:{1}:{2}", this.Name, verb, help);
        }

        public static string GetKey(IWrapperShellMenuItem menu)
        {
            string verb = !string.IsNullOrEmpty(menu.Verb) ? menu.Verb : menu.Caption;
            string help = !string.IsNullOrEmpty(menu.HelpString) ? menu.HelpString : menu.Caption;
            return string.Format("{0}:{1}:{2}", menu.Caption, verb, help);
        }

        public void PrintSampleXml()
        {
            this.Name = "Name";
            this.Verb = "Verb";
            this.Help = "Help";
            this.IconFilePath = "Path";
            this.ArgumentsFormat = "something {0} /something";
            this.TargetListMode = TargetMode.CommandLine;
            this.ProgramPath = "path";

            this.ExtentionFilter = new string[] { "Regex Pattern", @"^.*\.txt$" };

            this.Actions = new ActionItemList();

            ActionItem Child = new ActionItem();
            Child.Name = "Child";
            this.Actions.Add(Child);

            StringBuilder Builder = new StringBuilder();
            StringWriter SWriter = new StringWriter(Builder);
            XmlSerializer Serializer = new XmlSerializer(this.GetType());
            Serializer.Serialize(SWriter, this);
            Console.WriteLine(Builder.ToString());
        }
    }
}
