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
    public abstract class WriterBase : IDisposable
    {
        public WriterBase(string filePath) { }

        public abstract void WriteLine(string line);
        public abstract void Dispose();

        public abstract bool IsDisposed { get; set;}
    }
}
