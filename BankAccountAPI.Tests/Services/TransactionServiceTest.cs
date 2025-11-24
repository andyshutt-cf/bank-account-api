using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using NUnit.Framework;
using FluentAssertions;
using BankAccountAPI.Models;

namespace BankAccountAPI.Tests.Services
{
    [TestFixture]
    public class TransactionServiceTest
    {
        private BankAccount _account;
        private BankAccount _toAccount;

        [SetUp]
        public void Setup()
        {
            _account = new BankAccount
            {
                Id = 1,
                AccountNumber = "123456",
                AccountHolderName = "John Doe",
                Balance = 1000.0m
            };

            _toAccount = new BankAccount
            {
                Id = 2,
                AccountNumber = "654321",
                AccountHolderName = "Jane Doe",
                Balance = 500.0m
            };
        }

        #region Successful Transaction Tests

        [Test]
        public void Deposit_SuccessfulTransaction_ShouldIncreaseBalance()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal depositAmount = 250.0m;

            // Act
            _account.Deposit(depositAmount, "ATM Credit");

            // Assert
            _account.Balance.Should().Be(initialBalance + depositAmount);
        }

        [Test]
        public void Withdraw_SuccessfulTransaction_ShouldDecreaseBalance()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal withdrawAmount = 250.0m;

            // Act
            _account.Withdraw(withdrawAmount, "ATM Debit");

            // Assert
            _account.Balance.Should().Be(initialBalance - withdrawAmount);
        }

        [Test]
        public void Transfer_SuccessfulTransaction_ShouldUpdateBothBalances()
        {
            // Arrange
            decimal initialFromBalance = _account.Balance;
            decimal initialToBalance = _toAccount.Balance;
            decimal transferAmount = 300.0m;

            // Act
            _account.Transfer(_toAccount, transferAmount);

            // Assert
            _account.Balance.Should().Be(initialFromBalance - transferAmount);
            _toAccount.Balance.Should().Be(initialToBalance + transferAmount);
        }

        [Test]
        public void Deposit_MultipleSuccessfulTransactions_ShouldCumulativelyIncreaseBalance()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal firstDeposit = 100.0m;
            decimal secondDeposit = 200.0m;
            decimal thirdDeposit = 150.0m;

            // Act
            _account.Deposit(firstDeposit, "ATM Credit");
            _account.Deposit(secondDeposit, "Cheque Credit");
            _account.Deposit(thirdDeposit, "ATM Credit");

            // Assert
            _account.Balance.Should().Be(initialBalance + firstDeposit + secondDeposit + thirdDeposit);
        }

        [Test]
        public void Withdraw_MultipleSuccessfulTransactions_ShouldCumulativelyDecreaseBalance()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal firstWithdraw = 100.0m;
            decimal secondWithdraw = 150.0m;

            // Act
            _account.Withdraw(firstWithdraw, "ATM Debit");
            _account.Withdraw(secondWithdraw, "Direct Debit");

            // Assert
            _account.Balance.Should().Be(initialBalance - firstWithdraw - secondWithdraw);
        }

        #endregion

        #region Insufficient Funds Tests

        [Test]
        public void Withdraw_InsufficientFunds_ShouldThrowInvalidOperationException()
        {
            // Arrange
            decimal withdrawAmount = 1500.0m; // More than available balance

            // Act & Assert
            Action act = () => _account.Withdraw(withdrawAmount, "ATM Debit");
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Insufficient funds.");
        }

        [Test]
        public void Transfer_InsufficientFunds_ShouldThrowInvalidOperationException()
        {
            // Arrange
            decimal transferAmount = 1500.0m; // More than available balance

            // Act & Assert
            Action act = () => _account.Transfer(_toAccount, transferAmount);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("Insufficient funds.");
        }

        [Test]
        public void Withdraw_InsufficientFunds_ShouldNotModifyBalance()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal withdrawAmount = 1500.0m;

            // Act
            try
            {
                _account.Withdraw(withdrawAmount, "ATM Debit");
            }
            catch (InvalidOperationException)
            {
                // Expected exception
            }

            // Assert
            _account.Balance.Should().Be(initialBalance);
        }

        [Test]
        public void Transfer_InsufficientFunds_ShouldNotModifyEitherBalance()
        {
            // Arrange
            decimal initialFromBalance = _account.Balance;
            decimal initialToBalance = _toAccount.Balance;
            decimal transferAmount = 1500.0m;

            // Act
            try
            {
                _account.Transfer(_toAccount, transferAmount);
            }
            catch (InvalidOperationException)
            {
                // Expected exception
            }

            // Assert
            _account.Balance.Should().Be(initialFromBalance);
            _toAccount.Balance.Should().Be(initialToBalance);
        }

        #endregion

        #region Invalid Amount Tests

        [Test]
        public void Deposit_NegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            decimal negativeAmount = -100.0m;

            // Act & Assert
            Action act = () => _account.Deposit(negativeAmount, "ATM Credit");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Deposit amount must be positive.");
        }

        [Test]
        public void Deposit_ZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            decimal zeroAmount = 0.0m;

            // Act & Assert
            Action act = () => _account.Deposit(zeroAmount, "ATM Credit");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Deposit amount must be positive.");
        }

        [Test]
        public void Withdraw_NegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            decimal negativeAmount = -100.0m;

            // Act & Assert
            Action act = () => _account.Withdraw(negativeAmount, "ATM Debit");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Withdrawal amount must be positive.");
        }

        [Test]
        public void Withdraw_ZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            decimal zeroAmount = 0.0m;

            // Act & Assert
            Action act = () => _account.Withdraw(zeroAmount, "ATM Debit");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Withdrawal amount must be positive.");
        }

        [Test]
        public void Transfer_NegativeAmount_ShouldThrowArgumentException()
        {
            // Arrange
            decimal negativeAmount = -100.0m;

            // Act & Assert
            Action act = () => _account.Transfer(_toAccount, negativeAmount);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Transfer amount must be positive.");
        }

        [Test]
        public void Transfer_ZeroAmount_ShouldThrowArgumentException()
        {
            // Arrange
            decimal zeroAmount = 0.0m;

            // Act & Assert
            Action act = () => _account.Transfer(_toAccount, zeroAmount);
            act.Should().Throw<ArgumentException>()
                .WithMessage("Transfer amount must be positive.");
        }

        #endregion

        #region Concurrent Transaction Tests

        [Test]
        public void ConcurrentDeposits_ShouldAllSucceed()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal depositAmount = 50.0m;
            int numberOfThreads = 10;
            var exceptions = new List<Exception>();

            // Act
            var tasks = new Task[numberOfThreads];
            for (int i = 0; i < numberOfThreads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        _account.Deposit(depositAmount, "ATM Credit");
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }
            Task.WaitAll(tasks);

            // Assert
            exceptions.Should().BeEmpty("no exceptions should be thrown during concurrent deposits");
            _account.Balance.Should().Be(initialBalance + (depositAmount * numberOfThreads));
        }

        [Test]
        public void ConcurrentWithdrawals_WithSufficientFunds_ShouldAllSucceed()
        {
            // Arrange
            _account.Balance = 1000.0m;
            decimal withdrawAmount = 50.0m;
            int numberOfThreads = 10;
            var exceptions = new List<Exception>();

            // Act
            var tasks = new Task[numberOfThreads];
            for (int i = 0; i < numberOfThreads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        _account.Withdraw(withdrawAmount, "ATM Debit");
                    }
                    catch (Exception ex)
                    {
                        lock (exceptions)
                        {
                            exceptions.Add(ex);
                        }
                    }
                });
            }
            Task.WaitAll(tasks);

            // Assert
            exceptions.Should().BeEmpty("no exceptions should be thrown when sufficient funds");
            _account.Balance.Should().Be(1000.0m - (withdrawAmount * numberOfThreads));
        }

        [Test]
        public void ConcurrentTransfers_ShouldMaintainTotalBalance()
        {
            // Arrange
            _account.Balance = 1000.0m;
            _toAccount.Balance = 1000.0m;
            decimal transferAmount = 10.0m;
            int numberOfThreads = 10;
            decimal initialTotalBalance = _account.Balance + _toAccount.Balance;

            // Act
            var tasks = new Task[numberOfThreads];
            for (int i = 0; i < numberOfThreads; i++)
            {
                tasks[i] = Task.Run(() =>
                {
                    try
                    {
                        _account.Transfer(_toAccount, transferAmount);
                    }
                    catch (Exception)
                    {
                        // Some transfers may fail due to insufficient funds, which is expected
                    }
                });
            }
            Task.WaitAll(tasks);

            // Assert
            decimal finalTotalBalance = _account.Balance + _toAccount.Balance;
            finalTotalBalance.Should().Be(initialTotalBalance, "total balance should be preserved");
        }

        #endregion

        #region Edge Case Tests

        [Test]
        public void Withdraw_ExactBalance_ShouldNotDecreaseBalance()
        {
            // Arrange
            _account.Balance = 100.0m;
            decimal withdrawAmount = 100.0m;

            // Act
            _account.Withdraw(withdrawAmount, "ATM Debit");

            // Assert
            _account.Balance.Should().Be(100.0m, "withdrawing exact balance should not change balance");
        }

        [Test]
        public void Transfer_ExactBalance_ShouldSucceed()
        {
            // Arrange
            _account.Balance = 500.0m;
            decimal initialToBalance = _toAccount.Balance;
            decimal transferAmount = 500.0m;

            // Act
            _account.Transfer(_toAccount, transferAmount);

            // Assert
            _account.Balance.Should().Be(0.0m);
            _toAccount.Balance.Should().Be(initialToBalance + transferAmount);
        }

        [Test]
        public void Deposit_VeryLargeAmount_ShouldSucceed()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal largeAmount = 999999999.99m;

            // Act
            _account.Deposit(largeAmount, "Cheque Credit");

            // Assert
            _account.Balance.Should().Be(initialBalance + largeAmount);
        }

        [Test]
        public void Deposit_VerySmallAmount_ShouldSucceed()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal smallAmount = 0.01m;

            // Act
            _account.Deposit(smallAmount, "ATM Credit");

            // Assert
            _account.Balance.Should().Be(initialBalance + smallAmount);
        }

        [Test]
        public void Withdraw_VerySmallAmount_ShouldSucceed()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal smallAmount = 0.01m;

            // Act
            _account.Withdraw(smallAmount, "ATM Debit");

            // Assert
            _account.Balance.Should().Be(initialBalance - smallAmount);
        }

        [Test]
        public void Transfer_VerySmallAmount_ShouldSucceed()
        {
            // Arrange
            decimal initialFromBalance = _account.Balance;
            decimal initialToBalance = _toAccount.Balance;
            decimal smallAmount = 0.01m;

            // Act
            _account.Transfer(_toAccount, smallAmount);

            // Assert
            _account.Balance.Should().Be(initialFromBalance - smallAmount);
            _toAccount.Balance.Should().Be(initialToBalance + smallAmount);
        }

        [Test]
        public void SequentialTransactions_MixedOperations_ShouldMaintainCorrectBalance()
        {
            // Arrange
            _account.Balance = 1000.0m;

            // Act
            _account.Deposit(500.0m, "Cheque Credit");    // Balance: 1500
            _account.Withdraw(200.0m, "ATM Debit");       // Balance: 1300
            _account.Deposit(100.0m, "ATM Credit");       // Balance: 1400
            _account.Withdraw(300.0m, "Direct Debit");    // Balance: 1100
            _account.Transfer(_toAccount, 100.0m);        // Balance: 1000

            // Assert
            _account.Balance.Should().Be(1000.0m);
        }

        [Test]
        public void Withdraw_AlmostExactBalance_ShouldSucceed()
        {
            // Arrange
            _account.Balance = 100.0m;
            decimal withdrawAmount = 99.99m;

            // Act
            _account.Withdraw(withdrawAmount, "ATM Debit");

            // Assert
            _account.Balance.Should().Be(0.01m);
        }

        [Test]
        public void Deposit_WithDecimalPrecision_ShouldMaintainPrecision()
        {
            // Arrange
            _account.Balance = 100.50m;
            decimal depositAmount = 50.25m;

            // Act
            _account.Deposit(depositAmount, "ATM Credit");

            // Assert
            _account.Balance.Should().Be(150.75m);
        }

        [Test]
        public void Transfer_ToSameAccount_ShouldStillWork()
        {
            // Arrange
            decimal initialBalance = _account.Balance;
            decimal transferAmount = 100.0m;

            // Act
            _account.Transfer(_account, transferAmount);

            // Assert - Balance should remain the same (subtract and add same amount)
            _account.Balance.Should().Be(initialBalance);
        }

        [Test]
        public void Transaction_InvalidTransactionType_ForDeposit_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Action act = () => _account.Deposit(100.0m, "Invalid Type");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Transaction type must be Credit.");
        }

        [Test]
        public void Transaction_InvalidTransactionType_ForWithdraw_ShouldThrowArgumentException()
        {
            // Arrange & Act & Assert
            Action act = () => _account.Withdraw(100.0m, "Invalid Type");
            act.Should().Throw<ArgumentException>()
                .WithMessage("Transaction type must be Debit.");
        }

        #endregion
    }
}
