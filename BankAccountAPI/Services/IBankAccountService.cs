using System.Collections.Generic;
using BankAccountAPI.Models;

namespace BankAccountAPI.Services
{
    public interface IBankAccountService
    {
        void InitializeAccounts(List<BankAccount> accounts);
        IEnumerable<BankAccount> GetAllAccounts();
        BankAccount GetAccountById(int id);
        void AddAccount(BankAccount account);
        void DeleteAccount(int id);
        void CreateAccount(BankAccount account);
        void UpdateAccount(BankAccount account);
        void TransferFunds(int fromAccountId, int toAccountId, decimal amount);
    }
}