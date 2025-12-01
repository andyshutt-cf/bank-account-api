using Microsoft.AspNetCore.Mvc;
using BankAccountAPI.Models;
using System.Collections.Generic;

namespace BankAccountAPI.Controllers
{
    public class BankAccountControllerResponseBuilder
    {
        private BankAccount? _account;
        private IEnumerable<BankAccount>? _accounts;
        private int _accountId;
        private bool _isNotFound;
        private bool _isBadRequest;
        private bool _isCreated;
        private bool _isNoContent;
        private string? _actionName;

        public BankAccountControllerResponseBuilder WithAccount(BankAccount? account)
        {
            _account = account;
            return this;
        }

        public BankAccountControllerResponseBuilder WithAccounts(IEnumerable<BankAccount> accounts)
        {
            _accounts = accounts;
            return this;
        }

        public BankAccountControllerResponseBuilder WithAccountId(int accountId)
        {
            _accountId = accountId;
            return this;
        }

        public BankAccountControllerResponseBuilder AsNotFound()
        {
            _isNotFound = true;
            return this;
        }

        public BankAccountControllerResponseBuilder AsBadRequest()
        {
            _isBadRequest = true;
            return this;
        }

        public BankAccountControllerResponseBuilder AsCreated(string actionName)
        {
            _isCreated = true;
            _actionName = actionName;
            return this;
        }

        public BankAccountControllerResponseBuilder AsNoContent()
        {
            _isNoContent = true;
            return this;
        }

        public IActionResult Build(ControllerBase controller)
        {
            if (_isBadRequest)
            {
                return controller.BadRequest();
            }

            if (_isNotFound)
            {
                return controller.NotFound();
            }

            if (_isNoContent)
            {
                return controller.NoContent();
            }

            if (_isCreated && _account != null && _actionName != null)
            {
                return controller.CreatedAtAction(_actionName, new { id = _account.Id }, _account);
            }

            if (_accounts != null)
            {
                return controller.Ok(_accounts);
            }

            if (_account != null)
            {
                return controller.Ok(_account);
            }

            return controller.Ok();
        }

        public ActionResult<IEnumerable<BankAccount>> BuildCollection(ControllerBase controller)
        {
            if (_accounts != null)
            {
                return controller.Ok(_accounts);
            }

            return controller.Ok(new List<BankAccount>());
        }

        public ActionResult<BankAccount> BuildSingle(ControllerBase controller)
        {
            if (_isNotFound)
            {
                return controller.NotFound();
            }

            if (_account != null)
            {
                return controller.Ok(_account);
            }

            return controller.NotFound();
        }
    }
}
