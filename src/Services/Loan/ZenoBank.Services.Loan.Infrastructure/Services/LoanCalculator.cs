using ZenoBank.Services.Loan.Application.Abstractions.Services;

namespace ZenoBank.Services.Loan.Infrastructure.Services;

public class LoanCalculator : ILoanCalculator
{
    public decimal CalculateMonthlyPayment(decimal principal, decimal annualInterestRate, int termInMonths)
    {
        if (principal <= 0 || annualInterestRate < 0 || termInMonths <= 0)
            return 0m;

        if (annualInterestRate == 0)
            return Math.Round(principal / termInMonths, 2);

        var monthlyRate = (double)(annualInterestRate / 100m / 12m);
        var p = (double)principal;
        var n = termInMonths;

        var monthlyPayment = p * monthlyRate / (1 - Math.Pow(1 + monthlyRate, -n));

        return Math.Round((decimal)monthlyPayment, 2);
    }

    public decimal CalculateTotalRepayment(decimal monthlyPayment, int termInMonths)
    {
        return Math.Round(monthlyPayment * termInMonths, 2);
    }
}
