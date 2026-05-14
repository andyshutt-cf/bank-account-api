using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BankAccountAPI;
using BankAccountAPI.Models;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using BankAccountAPI.Services;

namespace BankAccountAPI.Tests.EndToEndTests
{
    [TestFixture]
    public class BankAccountApiTests
    {
        private HttpClient _client;
        private IServiceScope _scope;
        private IBankAccountService _bankAccountService;

        [SetUp]
        public void SetUp()
        {
            var appFactory = new WebApplicationFactory<Program>();
            _client = appFactory.CreateClient();
            _scope = appFactory.Services.CreateScope();
            _bankAccountService = _scope.ServiceProvider.GetRequiredService<IBankAccountService>();

            // Initialise test data
            var newAccount1 = new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "John Doe", Balance = 1000 };
            var newAccount2 = new BankAccount { Id = 2, AccountNumber = "456", AccountHolderName = "Jane Doe", Balance = 2000 };
            _bankAccountService.InitializeAccounts(new List<BankAccount> { newAccount1, newAccount2 });
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
            _scope.Dispose();
        }

        /// <summary>
        /// Scenario 1: Retrieve all bank accounts
        /// Given the bank account API is running
        /// When I request all bank accounts
        /// Then I should receive a list of all accounts with a 200 OK response
        /// </summary>
        [Test]
        public async Task GetAllAccounts_ReturnsOkResponse()
        {
            var response = await _client.GetAsync("/api/BankAccount");
            response.EnsureSuccessStatusCode();
            var accounts = await response.Content.ReadFromJsonAsync<List<BankAccount>>();
            Assert.IsNotNull(accounts);
        }

        /// <summary>
        /// Scenario 2: Retrieve a bank account by ID
        /// Given the bank account API is running
        /// When I request a bank account by ID
        /// Then I should receive the account details with a 200 OK response
        /// </summary>
        [Test]
        public async Task GetAccountById_ReturnsOkResponse()
        {
            var response = await _client.GetAsync("/api/BankAccount/1");
            response.EnsureSuccessStatusCode();
            var account = await response.Content.ReadFromJsonAsync<BankAccount>();
            Assert.IsNotNull(account);
        }

        /// <summary>
        /// Scenario 3: Create a new bank account
        /// Given the bank account API is running
        /// When I create a new bank account
        /// Then I should receive a 201 Created response with the created account details
        /// </summary>
        [Test]
        public async Task CreateAccount_ReturnsCreatedResponse()
        {
            var newAccount = new BankAccount { Id = 3, AccountNumber = "789", AccountHolderName = "Alice Doe", Balance = 3000 };
            var response = await _client.PostAsJsonAsync("/api/BankAccount", newAccount);
            response.EnsureSuccessStatusCode();
            var createdAccount = await response.Content.ReadFromJsonAsync<BankAccount>();
            Assert.IsNotNull(createdAccount);
        }

        /// <summary>
        /// Scenario 4: Update an existing bank account
        /// Given the bank account API is running
        /// When I update an existing bank account
        /// Then I should receive a 204 No Content response
        /// </summary>
        [Test]
        public async Task UpdateAccount_ReturnsNoContentResponse()
        {
            var updatedAccount = new BankAccount { Id = 1, AccountNumber = "123", AccountHolderName = "John Doe Updated", Balance = 1500 };
            var response = await _client.PutAsJsonAsync("/api/BankAccount/1", updatedAccount);
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// Scenario 5: Delete an existing bank account
        /// Given the bank account API is running
        /// When I delete an existing bank account
        /// Then I should receive a 204 No Content response
        /// </summary>
        [Test]
        public async Task DeleteAccount_ReturnsNoContentResponse()
        {
            var response = await _client.DeleteAsync("/api/BankAccount/1");
            response.EnsureSuccessStatusCode();
        }

        /// <summary>
        /// TODO: Scenario 6: Transfer funds between two bank accounts
        /// Given two bank accounts exist in the system
        /// When I transfer a valid amount from one account to another
        /// Then the source account balance should decrease, the destination account balance should increase, and I should receive a 200 OK response
        /// </summary>
        
    }
}