using BankAccountAPI.Builders;
using BankAccountAPI.Models;
using Microsoft.AspNetCore.Mvc;
using NUnit.Framework;
using System.Collections.Generic;

namespace BankAccountAPI.Tests.Builders
{
    [TestFixture]
    public class ActionResultBuilderTest
    {
        [Test]
        public void Create_ReturnsNewInstance()
        {
            // Act
            var builder = ActionResultBuilder<BankAccount>.Create();

            // Assert
            Assert.IsNotNull(builder);
        }

        [Test]
        public void Build_WithDataAndAsOk_ReturnsOkObjectResult()
        {
            // Arrange
            var account = new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "Test", Balance = 1000 };
            
            // Act
            var result = ActionResultBuilder<BankAccount>
                .Create()
                .WithData(account)
                .AsOk()
                .Build();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(account, okResult.Value);
        }

        [Test]
        public void Build_AsNotFound_ReturnsNotFoundResult()
        {
            // Act
            var result = ActionResultBuilder<BankAccount>
                .Create()
                .AsNotFound()
                .Build();

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result.Result);
        }

        [Test]
        public void BuildCreatedAtAction_ReturnsCreatedAtActionResult()
        {
            // Arrange
            var account = new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "Test", Balance = 1000 };
            
            // Act
            var result = ActionResultBuilder<BankAccount>
                .Create()
                .BuildCreatedAtAction("GetAccountById", new { id = account.Id }, account);

            // Assert
            Assert.IsInstanceOf<CreatedAtActionResult>(result);
            Assert.AreEqual("GetAccountById", result.ActionName);
            Assert.AreEqual(account, result.Value);
        }

        [Test]
        public void Build_WithDataForCollection_ReturnsOkObjectResultWithCollection()
        {
            // Arrange
            var accounts = new List<BankAccount>
            {
                new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "Test1", Balance = 1000 },
                new BankAccount { Id = 2, AccountNumber = "456", AccountHolderName = "Test2", Balance = 2000 }
            };
            
            // Act
            var result = ActionResultBuilder<IEnumerable<BankAccount>>
                .Create()
                .WithData(accounts)
                .AsOk()
                .Build();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
            var okResult = result.Result as OkObjectResult;
            Assert.IsNotNull(okResult);
            Assert.AreEqual(accounts, okResult.Value);
        }

        [Test]
        public void Builder_SupportsMethodChaining()
        {
            // Arrange
            var account = new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "Test", Balance = 1000 };
            
            // Act
            var result = ActionResultBuilder<BankAccount>
                .Create()
                .WithData(account)
                .WithStatusCode(200)
                .AsOk()
                .Build();

            // Assert
            Assert.IsInstanceOf<OkObjectResult>(result.Result);
        }

        [Test]
        public void NonGenericBuilder_AsNoContent_ReturnsNoContentResult()
        {
            // Act
            var result = ActionResultBuilder
                .Create()
                .AsNoContent()
                .Build();

            // Assert
            Assert.IsInstanceOf<NoContentResult>(result);
        }

        [Test]
        public void NonGenericBuilder_AsBadRequest_ReturnsBadRequestResult()
        {
            // Act
            var result = ActionResultBuilder
                .Create()
                .AsBadRequest()
                .Build();

            // Assert
            Assert.IsInstanceOf<BadRequestResult>(result);
        }

        [Test]
        public void NonGenericBuilder_AsNotFound_ReturnsNotFoundResult()
        {
            // Act
            var result = ActionResultBuilder
                .Create()
                .AsNotFound()
                .Build();

            // Assert
            Assert.IsInstanceOf<NotFoundResult>(result);
        }
    }
}
