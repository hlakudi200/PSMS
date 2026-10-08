using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Assessment.Shared;
using psms.Authorization;
using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Assessment.Reports;

/// <summary>What the school has recorded about a retention.</summary>
public class RetentionProcedureDto
{
    public Guid ReportId { get; set; }

    public DateTime? StaffMeetingDate { get; set; }
    public string StaffMeetingNote { get; set; }

    public DateTime? ParentMeetingDate { get; set; }
    public string ParentMeetingNote { get; set; }

    public DateTime? ParentConfirmedInWritingDate { get; set; }
    public string ParentConfirmationReference { get; set; }

    public DateTime? AppealDeadline { get; set; }
    public DateTime? AppealLodgedDate { get; set; }
    public DateTime? AppealDeterminationDeadline { get; set; }
    public DateTime? AppealDeterminedDate { get; set; }
    public string AppealOutcome { get; set; }

    /// <summary>§(2c): lodged after the three days had passed.</summary>
    public bool AppealWasLate { get; set; }

    /// <summary>§(2c): the fourteen working days have run out.</summary>
    public bool DeterminationIsOverdue { get; set; }

    /// <summary>
    /// §(2b)(b). Whether the card may be handed over yet — the one step the
    /// Protocol puts before it.
    /// </summary>
    public bool ReportMayBeIssued { get; set; }
}

/// <summary>What is being recorded about the meetings.</summary>
public class RecordRetentionProcedureDto
{
    public DateTime? StaffMeetingDate { get; set; }
    public string StaffMeetingNote { get; set; }
    public DateTime? ParentMeetingDate { get; set; }
    public string ParentMeetingNote { get; set; }
    public DateTime? ParentConfirmedInWritingDate { get; set; }
    public string ParentConfirmationReference { get; set; }
}

public class LodgeAppealDto
{
    /// <summary>When the written request was received.</summary>
    public DateTime LodgedDate { get; set; }

    /// <summary>
    /// When schools officially opened, which is what §(2c)'s three days run
    /// from. Optional — supplied when the school knows it, so the record can
    /// show whether the appeal was in time.
    /// </summary>
    public DateTime? SchoolsOpenedOn { get; set; }
}

public class DetermineAppealDto
{
    public DateTime DeterminedDate { get; set; }
    public string Outcome { get; set; }
}

public interface IRetentionProcedureAppService : IApplicationService
{
    Task<RetentionProcedureDto> GetAsync(Guid reportId);
    Task<RetentionProcedureDto> RecordAsync(Guid reportId, RecordRetentionProcedureDto input);
    Task<RetentionProcedureDto> LodgeAppealAsync(Guid reportId, LodgeAppealDto input);
    Task<RetentionProcedureDto> DetermineAppealAsync(Guid reportId, DetermineAppealDto input);
}

/// <summary>
/// RC-26. The procedure NPPPPR §(2b) and §(2c) put around retaining a learner.
/// <para>
/// §(2b): a special meeting of subject staff, then a meeting with the parent
/// "before the learner's school report is handed to them", and — if the learner
/// is retained — written confirmation by the parent. §(2c): the parent may
/// appeal within three days of schools opening, and the head of department
/// determines it within fourteen working days.
/// </para>
/// <para>
/// This records that those things happened and when. The minutes and the
/// parent's written confirmation live in the school's own file; what is held
/// here is the fact of them, which is what makes the ordering enforceable and
/// the deadlines visible.
/// </para>
/// </summary>
[AbpAuthorize(PermissionNames.Assessment_ReportCards)]
public class RetentionProcedureAppService : ApplicationService, IRetentionProcedureAppService
{
    private readonly IRepository<RetentionProcedure, Guid> _repository;
    private readonly IRepository<Report, Guid> _reportRepository;

    public RetentionProcedureAppService(
        IRepository<RetentionProcedure, Guid> repository,
        IRepository<Report, Guid> reportRepository)
    {
        _repository = repository;
        _reportRepository = reportRepository;
    }

    public async Task<RetentionProcedureDto> GetAsync(Guid reportId)
    {
        await LoadReportAsync(reportId);

        var procedure = await FindAsync(reportId);

        return procedure == null
            ? new RetentionProcedureDto { ReportId = reportId, ReportMayBeIssued = false }
            : MapToDto(procedure);
    }

    /// <summary>
    /// §(2b). Records the meetings, and the parent's written confirmation where
    /// it has been received. Patch-style: anything not supplied is left alone,
    /// so recording the parent meeting later does not erase the staff meeting.
    /// </summary>
    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<RetentionProcedureDto> RecordAsync(Guid reportId, RecordRetentionProcedureDto input)
    {
        var report = await LoadReportAsync(reportId);

        if (report.PromotionDecision != PromotionDecision.Retained)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound,
                "This learner is not being retained, so there is no retention procedure to record. "
                + "Record the promotion decision first.");

        var procedure = await FindAsync(reportId) ?? await CreateAsync(reportId);

        if (input.StaffMeetingDate.HasValue) procedure.StaffMeetingDate = input.StaffMeetingDate;
        if (input.StaffMeetingNote != null) procedure.StaffMeetingNote = Trim(input.StaffMeetingNote);
        if (input.ParentMeetingDate.HasValue) procedure.ParentMeetingDate = input.ParentMeetingDate;
        if (input.ParentMeetingNote != null) procedure.ParentMeetingNote = Trim(input.ParentMeetingNote);
        if (input.ParentConfirmedInWritingDate.HasValue)
            procedure.ParentConfirmedInWritingDate = input.ParentConfirmedInWritingDate;
        if (input.ParentConfirmationReference != null)
            procedure.ParentConfirmationReference = Trim(input.ParentConfirmationReference);

        await _repository.UpdateAsync(procedure);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToDto(procedure);
    }

    /// <summary>
    /// §(2c). Records a parent's written request to appeal, and the working-day
    /// deadline the determination then runs to.
    /// <para>
    /// A late appeal is recorded, not refused. It is still a parent asking, and
    /// whether to hear it is the department's call rather than this system's —
    /// so the record says it was late and leaves the decision to a person.
    /// </para>
    /// </summary>
    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Generate)]
    public async Task<RetentionProcedureDto> LodgeAppealAsync(Guid reportId, LodgeAppealDto input)
    {
        await LoadReportAsync(reportId);

        var procedure = await FindAsync(reportId) ?? await CreateAsync(reportId);

        procedure.AppealLodgedDate = input.LodgedDate.Date;

        if (input.SchoolsOpenedOn.HasValue)
            procedure.AppealDeadline = AppealDeadlines.LodgingDeadline(input.SchoolsOpenedOn.Value);

        procedure.AppealDeterminationDeadline =
            AppealDeadlines.DeterminationDeadline(input.LodgedDate);

        await _repository.UpdateAsync(procedure);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToDto(procedure);
    }

    /// <summary>§(2c). The head of department's final determination.</summary>
    [AbpAuthorize(PermissionNames.Assessment_ReportCards_Publish)]
    public async Task<RetentionProcedureDto> DetermineAppealAsync(Guid reportId, DetermineAppealDto input)
    {
        await LoadReportAsync(reportId);

        var procedure = await FindAsync(reportId);

        if (procedure?.AppealLodgedDate == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound,
                "No appeal has been lodged against this decision, so there is nothing to determine.");

        if (string.IsNullOrWhiteSpace(input.Outcome))
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportCardIncomplete,
                "Say what was determined. A determination with nothing recorded against it is "
                + "not a record of one.");

        procedure.AppealDeterminedDate = input.DeterminedDate.Date;
        procedure.AppealOutcome = Trim(input.Outcome);

        await _repository.UpdateAsync(procedure);
        await CurrentUnitOfWork.SaveChangesAsync();

        return MapToDto(procedure);
    }

    /* ==================== Helpers ==================== */

    private async Task<Report> LoadReportAsync(Guid reportId)
    {
        var report = await _reportRepository
            .FirstOrDefaultAsync(r => r.Id == reportId && r.TenantId == AbpSession.TenantId);

        if (report == null)
            throw new UserFriendlyException(AssessmentExceptionCodes.ReportNotFound, "Report not found.");

        return report;
    }

    private Task<RetentionProcedure> FindAsync(Guid reportId) =>
        _repository.GetAll()
            .FirstOrDefaultAsync(rp => rp.ReportId == reportId && rp.TenantId == AbpSession.TenantId);

    private async Task<RetentionProcedure> CreateAsync(Guid reportId)
    {
        var procedure = new RetentionProcedure(Guid.NewGuid(), AbpSession.TenantId, reportId);
        await _repository.InsertAsync(procedure);
        await CurrentUnitOfWork.SaveChangesAsync();
        return procedure;
    }

    private static string Trim(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static RetentionProcedureDto MapToDto(RetentionProcedure p) => new()
    {
        ReportId = p.ReportId,
        StaffMeetingDate = p.StaffMeetingDate,
        StaffMeetingNote = p.StaffMeetingNote,
        ParentMeetingDate = p.ParentMeetingDate,
        ParentMeetingNote = p.ParentMeetingNote,
        ParentConfirmedInWritingDate = p.ParentConfirmedInWritingDate,
        ParentConfirmationReference = p.ParentConfirmationReference,
        AppealDeadline = p.AppealDeadline,
        AppealLodgedDate = p.AppealLodgedDate,
        AppealDeterminationDeadline = p.AppealDeterminationDeadline,
        AppealDeterminedDate = p.AppealDeterminedDate,
        AppealOutcome = p.AppealOutcome,
        AppealWasLate = p.AppealWasLate(),
        DeterminationIsOverdue = p.DeterminationIsOverdue(DateTime.UtcNow),
        ReportMayBeIssued = p.ParentHasBeenMet(),
    };
}
