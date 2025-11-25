using System.Collections.Generic;
using BankAccountAPI.Models;
using System;

namespace BankAccountAPI.Services
{
    public class BankAccountService : IBankAccountService
    {
        private static List<BankAccount> _accounts = new List<BankAccount>();

        public IEnumerable<BankAccount> GetAllAccounts()
        {
            return _accounts;
        }

        public BankAccount GetAccountById(int id)
        {
            var account = _accounts.Find(account => account.Id == id);
            return account ?? throw new InvalidOperationException("Account not found");
        }

        public void AddAccount(BankAccount account)
        {
            _accounts.Add(account);
        }

        public void DeleteAccount(int id)
        {
            try
            {
                var account = _accounts.Find(account => account.Id == id);
                if (account == null)
                {
                    throw new KeyNotFoundException($"Account with ID {id} not found.");
                }
                _accounts.Remove(account);
            }
            catch (InvalidOperationException)
            {
                throw new KeyNotFoundException($"Account with ID {id} not found.");
            }
        }

        public void CreateAccount(BankAccount account)
        {
            _accounts.Add(account);
        }

        public void UpdateAccount(BankAccount account)
        {
            var existingAccount = _accounts.Find(a => a.Id == account.Id);
            if (existingAccount == null)
            {
                throw new KeyNotFoundException($"Account with ID {account.Id} not found.");
            }
            existingAccount.AccountNumber = account.AccountNumber;
            existingAccount.AccountHolderName = account.AccountHolderName;
            existingAccount.Balance = account.Balance;
        }

        public void InitializeAccounts(List<BankAccount> accounts)
        {
            _accounts = accounts;
        }

        public void TransferFunds(int fromAccountId, int toAccountId, decimal amount)
        {
            var fromAccount = GetAccountById(fromAccountId);
            var toAccount = GetAccountById(toAccountId);
            fromAccount.Transfer(toAccount, amount);
        }
    }
}