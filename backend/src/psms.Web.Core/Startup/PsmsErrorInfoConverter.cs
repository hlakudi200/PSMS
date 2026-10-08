using Abp.Web.Models;
using System;

namespace psms.Web.Startup
{
    /// <summary>
    /// Puts the sentence where a person will read it.
    /// <para>
    /// Both clients show <c>error.message</c> to the user — the web app as the
    /// title of its error dialog, the mobile app as the message itself — and
    /// what they were showing was <c>ASM_REPORT_CARD_INCOMPLETE</c>, with
    /// "This report card cannot be issued yet. It still needs the class
    /// teacher's comment." tucked into <c>details</c> underneath. The sentences
    /// in this codebase are written for the person reading them; they were
    /// simply in the wrong field.
    /// </para>
    /// <para>
    /// So where an exception carries one of our codes and a sentence to go with
    /// it, the two are swapped: the sentence becomes the message, and the code
    /// goes out in the <c>X-Error-Code</c> header that
    /// <see cref="PsmsExceptionFilter"/> sets. A client that needs to branch on
    /// which refusal this was reads the header.
    /// </para>
    /// <para>
    /// Where a call passes a code and nothing else, the code stays in the
    /// message: an unhelpful message beats an empty one.
    /// </para>
    /// </summary>
    public class PsmsErrorInfoConverter : IExceptionToErrorInfoConverter
    {
        public IExceptionToErrorInfoConverter Next { private get; set; }

        public ErrorInfo Convert(Exception exception)
        {
            var error = Next?.Convert(exception);

            if (error == null)
                return null;

            if (PsmsErrorCodes.CarriedBy(exception) == null)
                return error;

            if (string.IsNullOrWhiteSpace(error.Details))
                return error;

            error.Message = error.Details.Trim();
            error.Details = null;

            return error;
        }
    }
}
