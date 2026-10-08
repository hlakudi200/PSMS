using Abp.AspNetCore.Configuration;
using Abp.AspNetCore.Mvc.ExceptionHandling;
using Abp.Web.Configuration;
using Abp.Web.Models;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Net;

namespace psms.Web.Startup
{
    /// <summary>
    /// A refused request is not a server error.
    /// <para>
    /// Every refusal this application raises goes out as a
    /// <c>UserFriendlyException</c>, and ABP answers all of those with
    /// <b>HTTP 500</b>. A parent asking for another family's report card —
    /// correctly refused — got the same status as a crashed server, and so did
    /// every "not found", every "already exists" and every "you cannot do that
    /// yet". Logs, uptime monitoring and any client that retries on 5xx all read
    /// those as faults.
    /// </para>
    /// <para>
    /// 404 for something that is not there, including the cases where the
    /// system deliberately answers "not found" to someone who may not be told
    /// whether it exists. 400 for every other refusal: the request cannot be
    /// carried out as asked, and the caller is the one who can do something
    /// about it. Anything that is not one of ours — a real fault — keeps the
    /// status ABP gives it.
    /// </para>
    /// <para>
    /// The code itself moves to the <c>X-Error-Code</c> header, which is where
    /// a machine-readable code belongs. <see cref="PsmsErrorInfoConverter"/>
    /// handles the other half: taking it out of the field a person reads.
    /// </para>
    /// </summary>
    public class PsmsExceptionFilter : AbpExceptionFilter
    {
        /// <summary>
        /// Carries the application's own error code, for a client that needs to
        /// branch on which refusal this was rather than only show it.
        /// </summary>
        public const string ErrorCodeHeader = "X-Error-Code";

        public PsmsExceptionFilter(
            IErrorInfoBuilder errorInfoBuilder,
            IAbpAspNetCoreConfiguration configuration,
            IAbpWebCommonModuleConfiguration abpWebCommonModuleConfiguration)
            : base(errorInfoBuilder, configuration, abpWebCommonModuleConfiguration)
        {
        }

        protected override int GetStatusCode(ExceptionContext context, bool wrapOnError)
        {
            var code = PsmsErrorCodes.CarriedBy(context.Exception);

            if (code == null)
                return base.GetStatusCode(context, wrapOnError);

            if (!context.HttpContext.Response.Headers.ContainsKey(ErrorCodeHeader))
                context.HttpContext.Response.Headers[ErrorCodeHeader] = code;

            return (int)(PsmsErrorCodes.IsNotFound(code)
                ? HttpStatusCode.NotFound
                : HttpStatusCode.BadRequest);
        }
    }
}
