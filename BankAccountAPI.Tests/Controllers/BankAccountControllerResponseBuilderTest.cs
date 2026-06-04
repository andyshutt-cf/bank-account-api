using BankAccountAPI.Controllers;
using BankAccountAPI.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Collections.Generic;
using System.Linq;

namespace BankAccountAPI.Tests.Controllers
{
    [TestFixture]
    public class BankAccountControllerResponseBuilderTest
    {
        private TestController _controller;

        // Test controller to use the builder with
        private class TestController : ControllerBase
        {
        }

        [SetUp]
        public void Setup()
        {
            _controller = new TestController();
        }

        [Test]
        public void BuildCollection_WithAccounts_ReturnsOkResultWithAccounts()
        {
            // Arrange
            var accounts = new List<BankAccount>
            {
                new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "John Doe", Balance = 1000 },
                new BankAccount { Id = 2, AccountNumber = "456", AccountHolderName = "Jane Doe", Balance = 2000 }
            };
            var builder = new BankAccountControllerResponseBuilder()
                .WithAccounts(accounts);

            // Act
            var result = builder.BuildCollection(_controller);

            // Assert
            Assert.IsInstanceOf<ActionResult<IEnumerable<BankAccount>>>(result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(accounts, okResult.Value);
        }

        [Test]
        public void BuildSingle_WithAccount_ReturnsOkResultWithAccount()
        {
            // Arrange
            var account = new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "John Doe", Balance = 1000 };
            var builder = new BankAccountControllerResponseBuilder()
                .WithAccount(account);

            // Act
            var result = builder.BuildSingle(_controller);

            // Assert
            Assert.IsInstanceOf<ActionResult<BankAccount>>(result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(account, okResult.Value);
        }

        [Test]
        public void BuildSingle_WithNullAccountAndNotFound_ReturnsNotFoundResult()
        {
            // Arrange
            var builder = new BankAccountControllerResponseBuilder()
                .WithAccount(null)
                .AsNotFound();

            // Act
            var result = builder.BuildSingle(_controller);

            // Assert
            Assert.IsInstanceOf<ActionResult<BankAccount>>(result);
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public void Build_AsCreated_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var account = new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "John Doe", Balance = 1000 };
            var builder = new BankAccountControllerResponseBuilder()
                .WithAccount(account)
                .AsCreated("GetAccountById");

            // Act
            var result = builder.Build(_controller);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            var createdResult = result as CreatedAtActionResult;
            Assert.IsNotNull(createdResult);
            Assert.AreEqual("GetAccountById", createdResult.ActionName);
            Assert.AreEqual(account, createdResult.Value);
        }

        [Test]
        public void Build_AsBadRequest_ReturnsBadRequestResult()
        {
            // Arrange
            var builder = new BankAccountControllerResponseBuilder()
                .AsBadRequest();

            // Act
            var result = builder.Build(_controller);

            // Assert
            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public void Build_AsNoContent_ReturnsNoContentResult()
        {
            // Arrange
            var builder = new BankAccountControllerResponseBuilder()
                .AsNoContent();

            // Act
            var result = builder.Build(_controller);

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public void Build_WithAccounts_ReturnsOkResultWithAccounts()
        {
            // Arrange
            var accounts = new List<BankAccount>
            {
                new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "John Doe", Balance = 1000 }
            };
            var builder = new BankAccountControllerResponseBuilder()
                .WithAccounts(accounts);

            // Act
            var result = builder.Build(_controller);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(accounts, okResult.Value);
        }

        [Test]
        public void Build_WithSingleAccount_ReturnsOkResultWithAccount()
        {
            // Arrange
            var account = new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "John Doe", Balance = 1000 };
            var builder = new BankAccountControllerResponseBuilder()
                .WithAccount(account);

            // Act
            var result = builder.Build(_controller);

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result);
            var okResult = result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(account, okResult.Value);
        }
    }
}
