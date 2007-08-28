using System;
using System.Collections.Generic;
using System.Text;
using MbUnit.Framework;
using SkySoftware.EZShellExtensions;
using SkySoftware.EZShellExtensions.Wrapped.TestDoubles;
using ShellGlue.TestDoubles;
using System.Xml.Serialization;
using System.IO;

namespace ShellGlue.Tests
{
    [TestsOn(typeof(ActionItem))]
    [TestFixture]
    public class ActionItemTests
    {
        
        [RowTest]
        [Row("name", "verb", "help", "name:verb:help")]
        [Row("name", null, "help", "name:name:help")]
        [Row("name", "verb", null, "name:verb:name")]
        [Row("name", null, null, "name:name:name")]
        public void GetKeyFromActionItem(string name, string verb, string help, string expectedKey)
        {
            ActionItem TestSubject = new ActionItem();
            TestSubject.Name = name;
            TestSubject.Verb = verb;
            TestSubject.Help = help;

            string ActualKey = TestSubject.GetKey();

            Assert.AreEqual(expectedKey, ActualKey);
        }

        [RowTest]
        [Row("name", "verb", "help", "name:verb:help")]
        [Row("name", null, "help", "name:name:help")]
        [Row("name", "verb", null, "name:verb:name")]
        [Row("name", null, null, "name:name:name")]
        public void GetKeyFromShellMenuItem(string name, string verb, string help, string expectedKey)
        {
            
            FakeIWrapperShellMenuItem MenuItemFake = new FakeIWrapperShellMenuItem();
            MenuItemFake.Caption = name;
            MenuItemFake.Verb = verb;
            MenuItemFake.HelpString = help;

            string ActualKey = ActionItem.GetKey(MenuItemFake);

            Assert.AreEqual(expectedKey, ActualKey);
        }

        [RowTest]
        [Row(null, @"C:\Temp\This This is a test\test.bat", null, @"C:\Temp\This This is a test")]
        [Row(@"C:\Temp\This This is a test", null, null, @"C:\Temp")]
        [Row(@"C:\Temp\This This is a test", @"C:\Temp\This This is a test\test.bat", null, @"C:\Temp")]
        public void GetWorkingDirectory(string targetFolder, string targetFile1, string targetFile2, string expectedPath)
        {
            string[] targetFiles = new string[] { targetFile1, targetFile2 };

            ActionItem TestSubject = new ActionItem();

            string ActualPath = TestSubject.GetWorkingDirectory(targetFolder, targetFiles);

            Assert.AreEqual(expectedPath, ActualPath);
        }

        [RowTest]
        [Row(null, @"C:\Temp\Test.bat", @"C:\Temp\Test.xml", null, @"""C:\Temp\Test.bat"" ""C:\Temp\Test.xml"" ")]
        [Row(null, @"C:\Temp\Test.bat", null, "add {0} /recursive", @"add ""C:\Temp\Test.bat""  /recursive")]
        [Row(@"C:\Temp\This is a test", @"C:\Temp\Test.bat", @"C:\Temp\Test.xml", null, @"""C:\Temp\This is a test"" ""C:\Temp\Test.bat"" ""C:\Temp\Test.xml"" ")]
        public void GetArgumentsForCommandLineMode(
            string targetFolder, string targetFile1, 
            string targetFile2, string format,
            string expectedArguements)
        {
            string[] targetFiles = null;
            if (targetFile2 != null)
                targetFiles = new string[] { targetFile1, targetFile2 };
            else if (targetFile1 != null)
                targetFiles = new string[] { targetFile1, };

            ActionItem TestSubject = new ActionItem();
            TestSubject.TargetListMode = TargetMode.CommandLine;
            TestSubject.ArgumentsFormat = format;

            string ActualArguments = TestSubject.GetArguments(targetFolder, targetFiles);

            Assert.AreEqual(expectedArguements, ActualArguments);
        }

        [RowTest]
        [Row(null, @"C:\Temp\Test.bat", @"C:\Temp\Test.xml", null)]
        [Row(null, @"C:\Temp\Test.bat", null, "add {0} /recursive")]
        [Row(@"C:\Temp\This is a test", @"C:\Temp\Test.bat", @"C:\Temp\Test.xml", null)]
        public void GetArgumentsForFileMode(
            string targetFolder, string targetFile1,
            string targetFile2, string format)
        {
            int ExpectedWriteLineCallCount = 0;
            List<string> ExpectedWriteLineCallValues = new List<string>();
            
            if (targetFolder != null)
            {
                ++ExpectedWriteLineCallCount;
                ExpectedWriteLineCallValues.Add(targetFolder);
            }

            string[] targetFiles = null;
            if (targetFile2 != null)
            {
                targetFiles = new string[] { targetFile1, targetFile2 };
                ExpectedWriteLineCallCount += 2;
            }
            else if (targetFile1 != null)
            {
                targetFiles = new string[] { targetFile1, };
                ++ExpectedWriteLineCallCount;
            }

            ExpectedWriteLineCallValues.AddRange(targetFiles);

            ActionItem TestSubject = new ActionItem();
            TestSubject.TargetListMode = TargetMode.File;
            TestSubject.ArgumentsFormat = format;

            RecorderWriterBase WriterRecorder = new RecorderWriterBase("bogas $%@#!");
            TestSubject.Writer = WriterRecorder;

            string ActualArguments = TestSubject.GetArguments(targetFolder, targetFiles);
            
            Assert.AreEqual(ExpectedWriteLineCallCount, WriterRecorder.Recordings.WriteLineStringRecording.CallCount, "Writeline was not called the correct number of times.");

            for (int i = 0; i < ExpectedWriteLineCallCount; i++)
            {
                Assert.In(ExpectedWriteLineCallValues[i], 
                    WriterRecorder.Recordings.WriteLineStringRecording.AllPassedStringline, 
                    string.Format("This value was not written: {0}", ExpectedWriteLineCallValues[i]));
            }

            string ExpectedArguements;
            
            if (format != null)
            {
                format = format.Replace("{0}", @"""{0}"" ");
                ExpectedArguements = string.Format(format, TestSubject.TempFilePath);
            }
            else
                ExpectedArguements = string.Format(@"""{0}"" ", TestSubject.TempFilePath);

            Assert.AreEqual(ExpectedArguements, ActualArguments);
            Assert.IsTrue(WriterRecorder.Recordings.DisposeRecording.Called, "The writer was not disposed!");
        }

        [Test]
        public void Serializable()
        {
            ActionItem TestSubject = new ActionItem();
            TestSubject.Name = "Name";
            TestSubject.Verb = "Verb";
            TestSubject.Help = "Help";
            TestSubject.IconFilePath = "Path";
            TestSubject.ArgumentsFormat = "something {0} /something";
            TestSubject.TargetListMode = TargetMode.CommandLine;
            TestSubject.ProgramPath = "path";

            TestSubject.ExtentionFilter = new string[] { "Regex Pattern", @"^.*\.txt$" };

            TestSubject.Actions = new ActionItemList();

            ActionItem Child = new ActionItem();
            Child.Name = "Child";
            TestSubject.Actions.Add(Child);

            StringBuilder Builder = new StringBuilder();
            StringWriter SWriter = new StringWriter(Builder);
            XmlSerializer Serializer = new XmlSerializer(TestSubject.GetType());
            Serializer.Serialize(SWriter, TestSubject);

            ActionItem ClonedTestSubject = (ActionItem)Serializer.Deserialize(new StringReader(Builder.ToString()));

            Assert.AreEqual(TestSubject.Name, ClonedTestSubject.Name);
            Assert.AreEqual(TestSubject.Verb, ClonedTestSubject.Verb);

        }
    }
}
