namespace ZenoBank.Services.Loan.Application.Abstractions.Services;

public interface ILoanCalculator
{
    decimal CalculateMonthlyPayment(decimal principal, decimal annualInterestRate, int termInMonths);
    decimal CalculateTotalRepayment(decimal monthlyPayment, int termInMonths);
}
