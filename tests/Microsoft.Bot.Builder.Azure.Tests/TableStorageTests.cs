// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Core.Extensions;
using Microsoft.Bot.Builder.Core.Extensions.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Microsoft.Bot.Builder.Azure.Tests
{
    [TestClass]
    [TestCategory("Storage")]
    [TestCategory("Storage - Azure Tables")]
    public class TableStorageTests : StorageBaseTests
    {
        private IStorage storage;

        public TestContext TestContext { get; set; }

        private static TestContext _testContext;

        private static string emulatorPath = Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft SDKs\Azure\Storage Emulator\azurestorageemulator.exe");
        private const string noEmulatorMessage = "This test requires Azure Storage Emulator! go to https://go.microsoft.com/fwlink/?LinkId=717179 to download and install.";

        private static Lazy<bool> hasStorageEmulator = new Lazy<bool>(() =>
        {
            if (File.Exists(emulatorPath))
            {
                Process p = new Process();
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.RedirectStandardOutput = true;
                p.StartInfo.FileName = emulatorPath;
                p.StartInfo.Arguments = "status";
                p.Start();
                var output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();
                return output.Contains("IsRunning: True");
            }
            return false;
        });

        [ClassInitialize]
        public static void SetupTests(TestContext testContext)
        {
            _testContext = testContext;
        }

        [TestInitialize]
        public void TestInit()
        {
            if (hasStorageEmulator.Value)
            {
                //storage = new AzureTableStorage("UseDevelopmentStorage=true", TestContext.TestName.Replace("_","") + TestContext.GetHashCode().ToString("x"));
                storage = new AzureTableStorage("UseDevelopmentStorage=true", "storagetestsdotnet");
            }
        }

        ////[TestCleanup]
        ////public async Task TableStorage_TestCleanUp()
        ////{
        ////    if (storage != null)
        ////    {
        ////        AzureTableStorage store = (AzureTableStorage)storage;
        ////        await store.Table.DeleteIfExistsAsync();
        ////    }
        ////}

        public bool CheckStorageEmulator()
        {
            if (!hasStorageEmulator.Value)
                Debug.WriteLine(noEmulatorMessage);
            if (Debugger.IsAttached)
                Assert.IsTrue(hasStorageEmulator.Value, noEmulatorMessage);
            return hasStorageEmulator.Value;
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_CreateObjectTest()
        {
            if (CheckStorageEmulator())
                await base._createObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_ReadUnknownTest()
        {
            if (CheckStorageEmulator())
                await base._readUnknownTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_UpdateObjectTest()
        {
            if (CheckStorageEmulator())
                await base._updateObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_DeleteObjectTest()
        {
            if (CheckStorageEmulator())
                await base._deleteObjectTest(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_HandleCrazyKeys()
        {
            if (CheckStorageEmulator())
                await base._handleCrazyKeys(storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        [TestMethod]
        public async Task TableStorage_TypedSerialization()
        {
            if (CheckStorageEmulator())
                await base._typedSerialization(this.storage);
        }

        // NOTE: THESE TESTS REQUIRE THAT THE AZURE STORAGE EMULATOR IS INSTALLED AND STARTED !!!!!!!!!!!!!!!!!
        // Save a larger than 64KB object into Table Storage
        // </summary>
        [TestMethod]
        public async Task TableStorage_CreateLargerObjectTest()
        {
            if (CheckStorageEmulator())
            {
                var bigString = RandomString(30000);
                var storeItems = new StoreItems();
                storeItems.Add("BigObject", new
                {
                    Text1 = "1" + bigString,
                    Text2 = "2" + bigString,
                    Text3 = "3" + bigString,
                    Text4 = "4" + bigString
                });

                await storage.Write(storeItems);

                var storedItems = await storage.Read("BigObject");
                Assert.AreEqual("1" + bigString, storedItems.Get<dynamic>("BigObject").Text1);
                Assert.AreEqual("2" + bigString, storedItems.Get<dynamic>("BigObject").Text2);
                Assert.AreEqual("3" + bigString, storedItems.Get<dynamic>("BigObject").Text3);
                Assert.AreEqual("4" + bigString, storedItems.Get<dynamic>("BigObject").Text4);
            }
        }

        private static string RandomString(int length)
        {
            var random = new Random();
            const string pool = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Range(0, length)
                .Select(x => pool[random.Next(0, pool.Length)]);
            return new string(chars.ToArray());
        }

        public class BigPocoItem : IStoreItem
        {
            public string eTag { get; set; }

            public string Text { get; set; }
        }
    }
}
