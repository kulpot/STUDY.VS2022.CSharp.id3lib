using Utils;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;

#if VSNET
    using Microsoft.VisualStudio.TestTools.UnitTesting;
#else
    using NUnit.Framework;
    using TestMethod = NUnit.Framework.TestAttribute;
    using TestClass = NUnit.Framework.TestFixtureAttribute;
    using TestInitialize = NUnit.Framework.SetUpAttribute;
    using TestCleanup = NUnit.Framework.TearDownAttribute;
#endif

namespace TestProject
{
    
    
    /// <summary>
    ///This is a test class for FileIteratorTest and is intended
    ///to contain all FileIteratorTest Unit Tests
    ///</summary>
    [TestClass()]
    public class FileIteratorTest
    {
        static string _path = @"c:\#testmusic"; // TODO: should point to a directory containing a few mp3 files
        string[] _expectedAllResults = Directory.GetFiles(_path, "*.*", SearchOption.AllDirectories);
        string[] _expectedMP3Results = Directory.GetFiles(_path, "*.mp3", SearchOption.AllDirectories);

        //private TestContext testContextInstance;

        ///// <summary>
        /////Gets or sets the test context which provides
        /////information about and functionality for the current test run.
        /////</summary>
        //public TestContext TestContext
        //{
        //    get
        //    {
        //        return testContextInstance;
        //    }
        //    set
        //    {
        //        testContextInstance = value;
        //    }
        //}

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion


        /// <summary>
        ///A test for GetFilesRecursive
        ///</summary>
        [TestMethod()]
        public void GetFilesRecursiveTest()
        {
            IEnumerable<string> actual;
            long count = 0;

            using (Win32.HiPerfTimerWrapper timer = new Win32.HiPerfTimerWrapper("GetFilesRecursive"))
            {
                actual = FileIterator_Accessor.GetFilesRecursive(_path);
                count = actual.Count(); // force traversal
            }
            TraceF.WriteLine("{0} has expected {1} actual {2} files", _path, _expectedAllResults.Length, count);
            Assert.AreEqual(_expectedAllResults.Length, count);
        }

        /// <summary>
        ///A test for GetFiles
        ///</summary>
        [TestMethod()]
        public void GetFilesTest()
        {
            IEnumerable<string> actual;
            long count = 0;

            using (Win32.HiPerfTimerWrapper timer = new Win32.HiPerfTimerWrapper("GetFiles"))
            {
                actual = FileIterator_Accessor.GetFiles(_path);
                count = actual.Count(); // force traversal
            }
            TraceF.WriteLine("{0} has expected {1} actual {2} files", _path, _expectedAllResults.Length, count);
            Assert.AreEqual(_expectedAllResults.Length, count);
        }

        /// <summary>
        ///A test for GetFileInfosRecursive
        ///</summary>
        [TestMethod()]
        public void GetFileInfosRecursiveTest()
        {
            string pattern = "*.mp3";
            DirectoryInfo dir = new DirectoryInfo(_path);
            IEnumerable<FileInfo> actual;
            long count = 0;

            using (Win32.HiPerfTimerWrapper timer = new Win32.HiPerfTimerWrapper("GetFileInfosRecursive"))
            {
                actual = FileIterator_Accessor.GetFileInfosRecursive(dir, pattern);
                count = actual.Count(); // force traversal
            }
            TraceF.WriteLine("{0}\\{1} has expected {2} actual {3} files", _path, pattern, _expectedMP3Results.Length, count);
            Assert.AreEqual(_expectedMP3Results.Length, count);
        }

        /// <summary>
        ///A test for GetFileInfos
        ///</summary>
        [TestMethod()]
        public void GetFileInfosTest()
        {
            string pattern = "*.mp3";
            IEnumerable<FileInfo> actual;
            long count = 0;

            using (Win32.HiPerfTimerWrapper timer = new Win32.HiPerfTimerWrapper("GetFileInfos"))
            {
                actual = FileIterator_Accessor.GetFileInfos(_path, pattern);
                count = actual.Count(); // force traversal
            }
            TraceF.WriteLine("{0}\\{1} has expected {2} actual {3} files", _path, pattern, _expectedMP3Results.Length, count);
            Assert.AreEqual(_expectedMP3Results.Length, count);
        }
    }
}
