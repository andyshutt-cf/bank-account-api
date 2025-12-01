using Microsoft.AspNetCore.Mvc;
using BankAccountAPI.Models;
using BankAccountAPI.Services;
using BankAccountAPI.Builders;
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
            return ActionResultBuilder<IEnumerable<BankAccount>>
                .Create()
                .WithData(accounts)
                .AsOk()
                .Build();
        }

        [HttpGet("{id}")]
        public ActionResult<BankAccount> GetAccountById(int id)
        {
            var account = _bankAccountService.GetAccountById(id);
            if (account == null)
            {
                return ActionResultBuilder<BankAccount>
                    .Create()
                    .AsNotFound()
                    .Build();
            }
            return ActionResultBuilder<BankAccount>
                .Create()
                .WithData(account)
                .AsOk()
                .Build();
        }

        [HttpPost]
        public IActionResult CreateAccount(BankAccount account)
        {
            _bankAccountService.CreateAccount(account);
            return ActionResultBuilder<BankAccount>
                .Create()
                .BuildCreatedAtAction(nameof(GetAccountById), new { id = account.Id }, account);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateAccount(int id, BankAccount account)
        {
            if (id != account.Id)
            {
                return ActionResultBuilder<BankAccount>
                    .Create()
                    .AsBadRequest()
                    .BuildWithoutValue();
            }

            _bankAccountService.UpdateAccount(account);
            return ActionResultBuilder<BankAccount>
                .Create()
                .AsNoContent()
                .BuildWithoutValue();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteAccount(int id)
        {
            _bankAccountService.DeleteAccount(id);
            return ActionResultBuilder<BankAccount>
                .Create()
                .AsNoContent()
                .BuildWithoutValue();
        }
    }
}