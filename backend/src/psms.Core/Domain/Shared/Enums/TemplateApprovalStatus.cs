namespace psms.Domain.Shared.Enums
{
    /// <summary>
    /// COMM-06: approval state of a notification template. Only WhatsApp needs this
    /// (Meta pre-approves templates); other channels are NotApplicable (always
    /// usable). The WhatsApp provider (COMM-07) only sends an Approved template.
    /// </summary>
    public enum TemplateApprovalStatus
    {
        NotApplicable = 1,
        Draft = 2,
        Submitted = 3,
        Approved = 4,
        Rejected = 5
    }
}
