namespace psms.Admissions.ApplicationFees;

/// <summary>
/// Which payment gateway, if any, this deployment is wired to.
/// <para>
/// There is no real one yet. Until there is, an applicant has to be able to get
/// past the fee step somehow, so the school can run the rest of the journey end
/// to end and see what it does. That is what <see cref="Simulated"/> is for, and
/// it is the reason the mode exists at all rather than being assumed.
/// </para>
/// </summary>
public enum PaymentGatewayMode
{
    /// <summary>
    /// No online payment. The fee is paid by EFT or at the school, and the
    /// office records it. The applicant is shown what is owed and its status.
    /// </summary>
    OfflineOnly = 0,

    /// <summary>
    /// A stand-in that moves no money.
    /// <para>
    /// It exists so the admissions journey can be walked from end to end before
    /// a real gateway is chosen. Everything it touches says so, in those words,
    /// on the screen and on the receipt — a simulated payment that a parent
    /// could mistake for a real one would be worse than no payment step at all.
    /// </para>
    /// </summary>
    Simulated = 1,
}
