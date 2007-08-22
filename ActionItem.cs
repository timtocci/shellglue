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

        public void AddMenuItems(ref ShellMenuItem parentMenuItem)
        {
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
                this.Actions.AddMenuItems(ref MenuItem);
            }
        }
    }
}
