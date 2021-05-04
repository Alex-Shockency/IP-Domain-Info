using IP_Domain_API.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace IP_Domain_API.Tests
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void BadNameOrAddressInput1()
        {
            var controller = new IPDomainInfoController();
            var actionResult = controller.GetAsync("blargh");

            Assert.IsInstanceOfType(actionResult.Result.Result, typeof(BadRequestObjectResult));
        }
        [TestMethod]
        public void BadNameOrAddressInput2()
        {
            var controller = new IPDomainInfoController();
            var actionResult = controller.GetAsync("1293.812asdfa98213");

            Assert.IsInstanceOfType(actionResult.Result.Result, typeof(BadRequestObjectResult));
        }
        [TestMethod]
        public void ServiceThatIsNotSupported()
        {
            var controller = new IPDomainInfoController();
            var actionResult = controller.GetAsync("google.com", "test");

            Assert.IsInstanceOfType(actionResult.Result.Result, typeof(BadRequestObjectResult));
        }
    }
}
