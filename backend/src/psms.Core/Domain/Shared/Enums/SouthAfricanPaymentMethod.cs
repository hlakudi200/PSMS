namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// South African payment methods
    /// </summary>
    public enum SouthAfricanPaymentMethod
    {
        EFT = 1,              // Electronic Funds Transfer
        DebitOrder = 2,       // Recurring debit order
        CreditCard = 3,
        DebitCard = 4,
        Cash = 5,
        Cheque = 6,
        PayFast = 7,          // SA payment gateway
        SnapScan = 8,         // Mobile payment
        Zapper = 9,           // Mobile payment
        Ozow = 10,            // Instant EFT
        BankDeposit = 11
    }
}
