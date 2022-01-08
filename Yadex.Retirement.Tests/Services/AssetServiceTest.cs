using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Yadex.Retirement.Tests.Services
{
    [TestClass]
    public class AssetServiceTest
    {
        private YadexRetirementSettingsService _target;

        [TestInitialize]
        public void TestInitialize()
        {
            _target = new YadexRetirementSettingsService();
        }

        [TestMethod]
        public void TestLocation()
        {
            Assert.AreEqual(1, 1 * 1);

            Assert.IsTrue(System.IO.Directory.Exists(_target.FolderPath));
            Console.WriteLine(_target.FolderPath);
        }
    }
}