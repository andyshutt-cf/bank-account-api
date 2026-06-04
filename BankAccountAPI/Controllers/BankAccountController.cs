using Microsoft.AspNetCore.Mvc;
using BankAccountAPI.Models;
using BankAccountAPI.Services;
using System.Collections.Generic;

namespace BankAccountAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BankAccountController : ControllerBase
    {
        private readonly IBankAccountService _bankAccountService;

        public BankAccountController(IBankAccountService bankAccountService)
        {
            _bankAccountService = bankAccountService;
        }

        [HttpGet]
        public ActionResult<IEnumerable<BankAccount>> GetAllAccounts()
        {
            var accounts = _bankAccountService.GetAllAccounts();
            return new BankAccountControllerResponseBuilder()
                .WithAccounts(accounts)
                .BuildCollection(this);
        }

        [HttpGet("{id}")]
        public ActionResult<BankAccount> GetAccountById(int id)
        {
            var account = _bankAccountService.GetAccountById(id);
            var builder = new BankAccountControllerResponseBuilder()
                .WithAccount(account);
            
            if (account == null)
            {
                builder.AsNotFound();
            }
            
            return builder.BuildSingle(this);
        }

        [HttpPost]
        public IActionResult CreateAccount(BankAccount account)
        {
            _bankAccountService.CreateAccount(account);
            return new BankAccountControllerResponseBuilder()
                .WithAccount(account)
                .AsCreated(nameof(GetAccountById))
                .Build(this);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAccount(int id, BankAccount account)
        {
            if (id != account.Id)
            {
                return new BankAccountControllerResponseBuilder()
                    .AsBadRequest()
                    .Build(this);
            }

            _bankAccountService.UpdateAccount(account);
            return new BankAccountControllerResponseBuilder()
                .AsNoContent()
                .Build(this);
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(int id)
        {
            _bankAccountService.DeleteAccount(id);
            return new BankAccountControllerResponseBuilder()
                .AsNoContent()
                .Build(this);
        }
    }
}