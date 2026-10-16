using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using psms.Admissions.ApplicationFees.Yoco;
using psms.Controllers;
using psms.Domain.Admissions.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace psms.Web.Host.Controllers
{
    /// <summary>
    /// Receives Yoco's payment webhooks.
    /// <para>
    /// Anonymous, because Yoco has no account here — authenticated instead by
    /// the signature over the raw body. <b>That check is the whole of the
    /// security on this endpoint.</b> Without it, anyone on the internet could
    /// mark any application's fee paid by posting a JSON body at it, which is
    /// buying a place at the school for nothing.
    /// </para>
    /// <para>
    /// This is also where payment is actually settled, rather than on the
    /// redirect back: a payer who reaches the success page has not necessarily
    /// paid, and one who paid and closed the tab has.
    /// </para>
    /// <para>
    /// Runs outside any tenant or user context, so the lookup disables the
    /// tenant filter and finds the application by the reference carried in the
    /// payment's metadata.
    /// </para>
    /// </summary>
    [AllowAnonymous]
    [Route("api/yoco")]
    public class YocoWebhookController : psmsControllerBase
    {
        private readonly YocoPaymentGateway _yoco;
        private readonly IRepository<ApplicationFee, Guid> _feeRepository;
        private readonly IRepository<Application, Guid> _applicationRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public YocoWebhookController(
            YocoPaymentGateway yoco,
            IRepository<ApplicationFee, Guid> feeRepository,
            IRepository<Application, Guid> applicationRepository,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _yoco = yoco;
            _feeRepository = feeRepository;
            _applicationRepository = applicationRepository;
            _unitOfWorkManager = unitOfWorkManager;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Webhook()
        {
            // The raw body, byte for byte: the signature is over what was sent,
            // not over anything a serialiser has been round.
            string body;
            using (var reader = new StreamReader(Request.Body))
                body = await reader.ReadToEndAsync();

            var signed = _yoco.IsSignedByYoco(
                Request.Headers["webhook-id"].FirstOrDefault(),
                Request.Headers["webhook-timestamp"].FirstOrDefault(),
                Request.Headers["webhook-signature"].FirstOrDefault(),
                body);

            if (!signed)
            {
                Logger.Warn("Rejected a Yoco webhook: the signature did not verify.");
                return Unauthorized();
            }

            string type, reference, applicationId, paymentId;
            try
            {
                using (var document = JsonDocument.Parse(body))
                {
                    var root = document.RootElement;

                    type = Text(root, "type");
                    var payload = root.TryGetProperty("payload", out var p) ? p : root;
                    paymentId = Text(payload, "id");

                    var metadata = payload.TryGetProperty("metadata", out var m) ? m : default(JsonElement);
                    reference = metadata.ValueKind == JsonValueKind.Object ? Text(metadata, "reference") : null;
                    applicationId = metadata.ValueKind == JsonValueKind.Object ? Text(metadata, "applicationId") : null;
                }
            }
            catch (JsonException ex)
            {
                Logger.Warn($"Could not read a Yoco webhook body: {ex.Message}");
                // Signed but unreadable. Acked so Yoco stops retrying something
                // that will never parse.
                return Ok();
            }

            if (!string.Equals(type, "payment.succeeded", StringComparison.OrdinalIgnoreCase))
            {
                // Everything else is acked and ignored, so it is not retried.
                Logger.Info($"Yoco webhook '{type}' ignored.");
                return Ok();
            }

            if (string.IsNullOrWhiteSpace(reference) && string.IsNullOrWhiteSpace(applicationId))
            {
                Logger.Warn("A Yoco payment.succeeded carried no reference or application id.");
                return Ok();
            }

            await SettleAsync(reference, applicationId, paymentId);
            return Ok();
        }

        private async Task SettleAsync(string reference, string applicationId, string paymentId)
        {
            using (var unitOfWork = _unitOfWorkManager.Begin())
            using (_unitOfWorkManager.Current.DisableFilter(AbpDataFilters.MayHaveTenant))
            {
                var fee = await FindFeeAsync(reference, applicationId);
                if (fee == null)
                {
                    Logger.Warn($"A Yoco payment arrived for an unknown fee (reference {reference}).");
                    await unitOfWork.CompleteAsync();
                    return;
                }


                /* Yoco retries until it gets a 2xx, and a retry after a slow ack
                   is ordinary. Settling twice would move an application that has
                   since been reviewed back to the start of its workflow. */
                if (fee.Status == PaymentStatus.Completed)
                {
                    Logger.Info($"Yoco payment for {reference} was already settled; ignored.");
                    await unitOfWork.CompleteAsync();
                    return;
                }

                var application = await _applicationRepository.GetAll()
                    .FirstOrDefaultAsync(a => a.Id == fee.ApplicationId);

                if (application == null)
                {
                    Logger.Warn($"A Yoco payment arrived for fee {fee.Id} whose application is gone.");
                    await unitOfWork.CompleteAsync();
                    return;
                }

                fee.Status = PaymentStatus.Completed;
                fee.PaymentMethod = SouthAfricanPaymentMethod.CreditCard;
                fee.PaymentDate = DateTime.UtcNow;
                fee.PaymentReference = reference ?? paymentId;
                if (string.IsNullOrWhiteSpace(fee.ReceiptNumber))
                    fee.ReceiptNumber = $"RCP-{DateTime.UtcNow:yyyyMMddHHmmss}";
                await _feeRepository.UpdateAsync(fee);

                if (application.Status == ApplicationStatus.PaymentPending)
                    application.MarkPaymentReceived();

                await _applicationRepository.UpdateAsync(application);
                await _unitOfWorkManager.Current.SaveChangesAsync();

                Logger.Info($"Yoco payment {paymentId} settled the fee for {application.ApplicationNumber}.");

                await unitOfWork.CompleteAsync();
            }
        }

        private async Task<ApplicationFee> FindFeeAsync(string reference, string applicationId)
        {
            if (!string.IsNullOrWhiteSpace(reference))
            {
                var byReference = await _feeRepository.GetAll()
                    .FirstOrDefaultAsync(f => f.PaymentReference == reference);

                if (byReference != null) return byReference;
            }

            // The reference is written before the payer leaves, so this is the
            // unusual path — but an application id that round-tripped through
            // Yoco's metadata is still better than losing the payment.
            return Guid.TryParse(applicationId, out var id)
                ? await _feeRepository.GetAll().FirstOrDefaultAsync(f => f.ApplicationId == id)
                : null;
        }

        private static string Text(JsonElement element, string name) =>
            element.ValueKind == JsonValueKind.Object
            && element.TryGetProperty(name, out var value)
            && value.ValueKind == JsonValueKind.String
                ? value.GetString()
                : null;
    }
}
