using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yadex.Retirement.Tests.Services
{
    [TestClass]
    public class AssetServiceTest
    {
        [TestMethod]
        public void TestLocation()
        {
            var target = new YadexRetirementSettingsService();
            Assert.AreEqual(1, 1 * 1);

            Assert.IsTrue(System.IO.Directory.Exists(target.FolderPath));
            Console.WriteLine(target.FolderPath);
        }
    }
}