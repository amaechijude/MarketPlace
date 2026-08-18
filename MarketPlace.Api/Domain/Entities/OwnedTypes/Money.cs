using System.Net;
using MarketPlace.Api.Common.ExceptionHandler;
using MarketPlace.Api.Domain.Entities.Enums;

namespace MarketPlace.Api.Domain.Entities.OwnedTypes;

public sealed record Money
{
    public Currency Currency { get; init; }
    public decimal Amount { get; private set; }

    private Money(Currency currency, decimal amount)
    {
        Currency = currency;
        Amount = amount;
    }

    public static Money Zero(Currency currency) => new(currency, 0);

    public static Money Create(Currency currency, decimal amount) => new(currency, amount);

    public void Add(Money money)
    {
        if (Currency != money.Currency)
            throw new CurrencyMisMatchException($"Cannot Add {Currency} and {money.Currency}");

        if (money.Amount <= 0)
            throw new NegativeMoneyAdditionExecption();

        Amount += money.Amount;
    }
}

public sealed class CurrencyMisMatchException(string message)
    : CustomAppExceptions(message, HttpStatusCode.Conflict);

public sealed class NegativeMoneyAdditionExecption(string message = "Cannot add negative value")
    : CustomAppExceptions(message, HttpStatusCode.Conflict);
