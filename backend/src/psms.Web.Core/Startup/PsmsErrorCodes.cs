using Abp.UI;
using System;
using System.Text.RegularExpressions;

namespace psms.Web.Startup
{
    /// <summary>
    /// Recognising this application's own error codes in an exception.
    /// <para>
    /// ABP's two-argument <c>UserFriendlyException(string, string)</c> is
    /// <c>(message, details)</c>. Every refusal in this codebase is written as
    /// <c>new UserFriendlyException(SomeExceptionCodes.Thing, "A sentence.")</c>,
    /// so the code lands in the message and the sentence in the details. There
    /// are over nine hundred of those calls; reading them correctly in one place
    /// is a smaller and safer change than rewriting every one of them.
    /// </para>
    /// </summary>
    public static class PsmsErrorCodes
    {
        /// <summary>
        /// What one of our codes looks like: a short module prefix, an
        /// underscore, then upper-case words — ASM_REPORT_NOT_FOUND,
        /// WF_STEP_NOT_FOUND, ACA_CLASS_FULL.
        /// <para>
        /// Deliberately strict. An exception whose message is an ordinary
        /// sentence, or a single shouted word, must not be mistaken for a code
        /// and have a perfectly good message thrown away.
        /// </para>
        /// </summary>
        private static readonly Regex CodeShape =
            new Regex(@"^[A-Z][A-Z0-9]{1,9}(_[A-Z0-9]+)+$", RegexOptions.Compiled);

        /// <summary>
        /// The application's own error code, when this exception carries one in
        /// the field ABP treats as the message; otherwise null.
        /// </summary>
        public static string CarriedBy(Exception exception)
        {
            if (!(exception is UserFriendlyException userFriendly))
                return null;

            var message = userFriendly.Message?.Trim();

            return !string.IsNullOrEmpty(message) && CodeShape.IsMatch(message)
                ? message
                : null;
        }

        /// <summary>
        /// Whether this code says the thing was not there. Several of these are
        /// deliberate: a parent asking for another family's card is told "not
        /// found" rather than "not yours", and 404 is the honest status for
        /// that answer too.
        /// </summary>
        public static bool IsNotFound(string code) =>
            code != null && code.EndsWith("_NOT_FOUND", StringComparison.Ordinal);
    }
}
