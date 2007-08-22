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
using Microsoft.Win32;
using System.Resources;
/*
The following steps are only necessary if adding a new shell extension to an existing 
ClassLibrary project via the "Add New Item.." dialog box.
These are automatically performed when create a brand new shell extensions project is
created using the "New Project" dialog box.

STEP 1 
If the project's AssemblyInfo.cs file has the AssemblyVersion 
atrribute with a variable version as follows :
[assembly: AssemblyVersion("1.0.*")]
...change it to a constant version as follows :
[assembly: AssemblyVersion("1.0.0.0")]

STEP 2
This assembly must be given a strong name so that it can be installed in the GAC.
To do so, generate a strong name key pair using the sn.exe tool that comes with the .Net SDK as follows :
sn.exe -k keypair.kp 
Then copy the generated keypair.kp file to the project directory and add the following attribute
to the project's AssemblyInfo.cs file
[assembly: AssemblyKeyFile("..\\..\\keypair.kp")]
*/

/*
USING POST-BUILD STEPS 
Use the following Post-Build Steps to speed up testing and developing of your shell extension : 

"{Path To RegisterExtension.exe}" -i "$(TargetPath)"
"{Path To RestartExplorer.exe}"

The first step registers the shell extension on the system and installs the dll in the GAC after 
every build. The second step restarts Windows Explorer so that your newly built dll gets loaded 
by Windows Explorer.

Example : 
"C:\Program Files\Sky Software\EZShellExtensions.Net\RegisterExtension.exe" -i "$(TargetPath)"
"C:\Program Files\Sky Software\EZShellExtensions.Net\RestartExplorer.exe"

Note: Use RegisterExtensionDotNet20.exe instead of RegisterExtension.exe if using Visual Studio 2005/.Net 2.0

See the topic "Using Post-Build Steps to ease testing of shell extensions" in the EZShellExtensions.Net 
help file for more information.
*/

namespace GlueContextMenuExtension
{

	// The GuidAttribute has been applied to the extension class
	// with an automatically generated unique GUID.
	// Every different extension should have such a unique GUID
	[Guid("DFC9FE9D-5C00-4460-9130-C5627499B6D4")]
	[ComVisible(true)]
	// The TargetExtension attribute has been applied below to indicate 
	// the file types that your extension targets. 
	// TODO : Change the extension below to your desired extension.
	[TargetExtension("Folder", true)]
	public class GlueContextMenuExtension : ContextMenuExtension
	{
        private const string GlueShellMenuItemName = "Glue Shell";

		private Settings _Configuration;
		public Settings Configuration
		{
			get
			{
				return _Configuration;
			}
			set
			{
				_Configuration = value;
			}
		}

		public GlueContextMenuExtension()
        {
			using (StreamReader SettingsStream = File.OpenText(Path.Combine(Path.GetDirectoryName(this.GetType().Assembly.Location), "Settings.xml")))
			{
				this.Configuration = (Settings)new XmlSerializer(typeof(Settings)).Deserialize(SettingsStream);
			}
		}

		// Override this method to perform initialzation specific to your contextmenu extension. 
		protected override bool OnInitialize()
		{
			// TODO : Add your initialization code here

			return true;
		}

		// Override this method to add your menu items to the context menu
		protected override void OnGetMenuItems(SkySoftware.EZShellExtensions.GetMenuitemsEventArgs e)
		{
			ShellMenuItem parent = e.Menu.AddItem(GlueShellMenuItemName);
			parent.HasSubMenu = true;
			parent.OwnerDraw = true;

            this.Configuration.Actions.AddMenuItems(ref parent);
		}


		// Override this method to perform your own tasks when any of the 
		// menu items provided by your contextmenu extension is selected by the user.
		protected override bool OnExecuteMenuItem(SkySoftware.EZShellExtensions.ExecuteItemEventArgs e)
		{
			ProcessStartInfo ProcInfo = new ProcessStartInfo();
			ProcInfo.FileName = this.Configuration.Actions.GetActionItem(e.MenuItem.Verb).ProgramPath;
			ProcInfo.CreateNoWindow = false;
			ProcInfo.UseShellExecute = false;

			StringBuilder ArgBuilder = new StringBuilder();

			if (!String.IsNullOrEmpty(this.TargetFolder))
				ArgBuilder.Append(string.Format("\"{0}\" ", this.TargetFolder));

			if (this.TargetFiles != null && this.TargetFiles.Length > 0)
				foreach (string FilePath in this.TargetFiles)
					ArgBuilder.Append(string.Format("\"{0}\" ", FilePath));

			ProcInfo.Arguments = ArgBuilder.ToString();

			System.Diagnostics.Process process = new Process();
			process.StartInfo = ProcInfo;
			process.Start();

			// Return value is ignored.
			return true;
		}


		// Override this method to provide the dimensions for any owner drawn 
		// contextmenu items provided by your contextmenu extension.
		// TODO : UNCOMMENT THIS OVERRIDE IF YOU HAVE OWNER DRAWN MENU IETMS
		//protected override void OnMeasureMenuItem(SkySoftware.EZShellExtensions.EZSMeasureItemEventArgs e)
		//{
		//    e.ItemHeight = e.ItemWidth = 150;
		//}


		// Override this method to draw any owner-draw menu items
		// added by the contextmenu extension.
		// TODO : UNCOMMENT THIS OVERRIDE IF YOU HAVE OWNER DRAWN MENU IETMS
		protected override void OnDrawMenuItem(SkySoftware.EZShellExtensions.EZSDrawItemEventArgs e)
        {
            e.DrawBackground();
            Stream IconStream = null;

            if (e.MenuItem.Verb == GlueShellMenuItemName)
                IconStream = this.GetType().Assembly.GetManifestResourceStream("GlueContextMenuExtension.Glue.ico");
            else
                IconStream = File.OpenRead(this.Configuration.Actions.GetActionItem(e.MenuItem.Verb).IconFilePath);
            try
            {
	            using (Icon GlueImage = new Icon(IconStream))
                {
                    e.Graphics.DrawIconUnstretched(GlueImage, e.Bounds);
                }
            }
            finally
            {
	            if (IconStream != null)
	                IconStream.Dispose();
            }
            e.Graphics.DrawString(e.MenuItem.Caption, SystemInformation.MenuFont, Brushes.Black, 17.0F, (float)e.Bounds.Top + 2);
            e.DrawFocusRectangle();
		}

		// Your assembly should have one static method marked with the 
		// ComRegisterFunction attribute. The function should return void and take 
		// one parameter whose type is System.Type.
		// 
		// This method is used to register the extension on the system by calling the
		// RegisterExtension method.
		//
		[ComRegisterFunction]
		public static void Register(System.Type t)
		{
			ContextMenuExtension.RegisterExtension(typeof(GlueContextMenuExtension));
            RegistryKey Hive = Registry.ClassesRoot;
            RegistryKey NewKey = Hive.CreateSubKey(@"*\shellex\ContextMenuHandlers\GlueContextMenuExtension");
            NewKey.SetValue(null, "{dfc9fe9d-5c00-4460-9130-c5627499b6d4}");
		}

		// Your assembly should have one static method marked with the 
		// ComUnregisterFunction attribute. The function should return void and take 
		// one parameter whose type is System.Type.
		// 
		// This method is used to register the extension on the system by calling the
		// UnRegisterExtension method.
		//
		[ComUnregisterFunction]
		public static void UnRegister(System.Type t)
		{
			ContextMenuExtension.UnRegisterExtension(typeof(GlueContextMenuExtension));
            RegistryKey Hive = Registry.ClassesRoot;
            Hive.DeleteSubKeyTree(@"*\shellex\ContextMenuHandlers\GlueContextMenuExtension");
		}


	}
}