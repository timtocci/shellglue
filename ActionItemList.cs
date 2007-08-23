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
                if (_FlatList == null)
                    _FlatList = new Dictionary<string, ActionItem>();
                return _FlatList;
            }
            set
            {
                _FlatList = value;
            }
        }

        protected override string GetKeyForItem(ActionItem item)
        {
            if (!FlatList.ContainsKey(item.GetKey()))
                FlatList.Add(item.GetKey(), item);
            return item.GetKey();
        }

        public void AddMenuItems(ref ShellMenuItem parentMenuItem, string targetFolder, string[] targetFiles)
        {
            foreach (ActionItem Action in this)
                Action.AddMenuItems(ref parentMenuItem, targetFolder, targetFiles);
        }

        public ActionItem GetActionItem(ShellMenuItem menu)
        {
            return FlatList[ActionItem.GetKey(menu)];
        }

        //public ActionItem GetActionItem(string key)
        //{
        //    return FlatList[key];
        //}

    }
}
