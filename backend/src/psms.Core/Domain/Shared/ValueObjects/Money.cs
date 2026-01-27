using System;

namespace psms.Domain.Shared.ValueObjects
{
    /// <summary>
    /// Represents a monetary value with currency (primarily ZAR for South African schools).
    /// Configured as owned type in DbContext.
    /// </summary>
    public class Money : IEquatable<Money>
    {
        /// <summary>
        /// The monetary amount.
        /// </summary>
        public decimal Amount { get; private set; }

        /// <summary>
        /// ISO 4217 currency code (default: ZAR - South African Rand).
        /// </summary>
        public string Currency { get; private set; }

        /// <summary>
        /// Parameterless constructor for EF Core.
        /// </summary>
        private Money()
        {
        }

        /// <summary>
        /// Creates a new Money value object.
        /// </summary>
        public Money(decimal amount, string currency = "ZAR")
        {
            if (amount < 0)
                throw new ArgumentException("Amount cannot be negative", nameof(amount));

            Amount = Math.Round(amount, 2);
            Currency = currency ?? "ZAR";
        }

        /// <summary>
        /// Adds two Money values with the same currency.
        /// </summary>
        public static Money operator +(Money left, Money right)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));

            if (left.Currency != right.Currency)
                throw new InvalidOperationException(
                    $"Cannot add money with different currencies: {left.Currency} and {right.Currency}");

            return new Money(left.Amount + right.Amount, left.Currency);
        }

        /// <summary>
        /// Subtracts two Money values with the same currency.
        /// </summary>
        public static Money operator -(Money left, Money right)
        {
            if (left is null) throw new ArgumentNullException(nameof(left));
            if (right is null) throw new ArgumentNullException(nameof(right));

            if (left.Currency != right.Currency)
                throw new InvalidOperationException(
                    $"Cannot subtract money with different currencies: {left.Currency} and {right.Currency}");

            var result = left.Amount - right.Amount;
            if (result < 0)
                throw new InvalidOperationException("Result cannot be negative");

            return new Money(result, left.Currency);
        }

        /// <summary>
        /// Multiplies money by a scalar value.
        /// </summary>
        public static Money operator *(Money money, decimal multiplier)
        {
            if (money is null) throw new ArgumentNullException(nameof(money));
            return new Money(money.Amount * multiplier, money.Currency);
        }

        /// <summary>
        /// Returns formatted money string (e.g., "R 1,250.50").
        /// </summary>
        public string GetFormattedAmount()
        {
            return Currency switch
            {
                "ZAR" => $"R {Amount:N2}",
                "USD" => $"${Amount:N2}",
                "EUR" => $"€{Amount:N2}",
                "GBP" => $"£{Amount:N2}",
                _ => $"{Currency} {Amount:N2}"
            };
        }

        public bool Equals(Money other)
        {
            if (other is null) return false;
            return Amount == other.Amount && Currency == other.Currency;
        }

        public override bool Equals(object obj) => Equals(obj as Money);

        public override int GetHashCode() => HashCode.Combine(Amount, Currency);

        public static bool operator ==(Money left, Money right)
        {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(Money left, Money right) => !(left == right);
    }
}
