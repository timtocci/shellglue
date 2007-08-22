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
using System.Collections.Generic;

namespace GlueContextMenuExtension
{
    [Serializable]
    public class ActionItemList : KeyedCollection<string, ActionItem>
    {
        private static Dictionary<string,ActionItem> _FlatList;

        private static Dictionary<string, ActionItem> FlatList
        {
            get
            {
                return _FlatList;
            }
            set
            {
                _FlatList = value;
            }
        }

        protected override string GetKeyForItem(ActionItem item)
        {
            if (!FlatList.ContainsKey(item.Verb))
                FlatList.Add(item.Verb, item);
            return item.Verb;
        }

        public void AddMenuItems(ref ShellMenuItem parentMenuItem)
        {
            foreach (ActionItem Action in this)
                Action.AddMenuItems(ref parentMenuItem);
        }

        public ActionItem GetActionItem(string verb)
        {
            return FlatList[verb];
        }

    }
}
