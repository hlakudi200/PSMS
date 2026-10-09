using Abp.Application.Services;
using psms.Admissions.ApplicationFees.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Admissions.ApplicationFees;

/// <summary>
/// Service for managing application fees and payments.
/// Implements ADM-006, ADM-007.
/// </summary>
public interface IApplicationFeeAppService : IApplicationService
{
    /// <summary>
    /// Gets the fee for an application.
    /// </summary>
    Task<ApplicationFeeDto> GetByApplicationAsync(Guid applicationId);

    /// <summary>
    /// Records a manual payment (EFT, Cash, etc.).
    /// </summary>
    Task<PaymentResultDto> RecordPaymentAsync(Guid applicationId, RecordPaymentDto input);

    /// <summary>
    /// Processes a payment callback from payment gateway.
    /// </summary>
    Task<PaymentResultDto> ProcessPaymentCallbackAsync(PaymentCallbackDto input);

    /// <summary>
    /// Gets payment status for an application.
    /// </summary>
    Task<PaymentResultDto> GetPaymentStatusAsync(Guid applicationId);

    /// <summary>
    /// What the applicant needs in order to deal with the fee: how much, what
    /// state it is in, and whether there is anything online to click.
    /// </summary>
    Task<ApplicationFeeCheckoutDto> GetCheckoutAsync(Guid applicationId);

    /// <summary>
    /// Settles the fee through the stand-in gateway. No money moves; refused
    /// unless this deployment is configured for the simulated gateway.
    /// </summary>
    Task<PaymentResultDto> SimulatePaymentAsync(Guid applicationId);
}
