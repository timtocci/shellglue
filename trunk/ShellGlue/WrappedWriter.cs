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
    public class WrappedWriter : WriterBase
    {
        private bool _IsDisposed;
        public override bool IsDisposed
        {
            get { return _IsDisposed; }
            set
            {
                _IsDisposed = value;
            }
        }
        private TextWriter _WrappedSubject;
        private TextWriter WrappedSubject
        {
            get
            {
                return _WrappedSubject;
            }
            set
            {
                _WrappedSubject = value;
            }
        }

        public WrappedWriter(string filePath)
            : base(filePath)
        {
            this.WrappedSubject = File.CreateText(filePath);
        }

        public override void WriteLine(string line)
        {
            this.WrappedSubject.WriteLine(line);
        }

        public override void Dispose()
        {
            this.WrappedSubject.Dispose();
            this.IsDisposed = true;
        }
    }
}
