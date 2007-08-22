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
    public class Settings
    {
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
    }
}
