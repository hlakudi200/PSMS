namespace psms.Admissions.ApplicationFees;

/// <summary>
/// Which payment gateway, if any, this deployment is wired to.
/// <para>
/// <see cref="Simulated"/> came first and still earns its place: it is how the
/// admissions journey was walked end to end before any real gateway existed,
/// and how it still is on a machine with no keys. It moves no money and says
/// so everywhere it appears.
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

    /// <summary>
    /// Yoco's hosted checkout. The payer is sent to Yoco's own page, so no card
    /// details reach this application, and settlement comes back as a signed
    /// webhook rather than on the redirect — a payer closing the tab after
    /// paying is ordinary, and the redirect cannot be relied on to happen.
    /// </summary>
    Yoco = 2,
}
