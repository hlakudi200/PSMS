using Abp.Dependency;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using Microsoft.EntityFrameworkCore;
using psms.Domain.Assessment;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Assessment.Reports;

/// <summary>
/// RC-06. Fills in the five figures on a report card that only exist relative
/// to the rest of the class: the learner's position overall, and per subject
/// their position, the class average, the highest and the lowest mark.
/// <para>
/// These cannot be computed while generating one learner — there is no cohort
/// to compare against yet — so this runs as a pass over the whole class once
/// its reports exist. It is called after a bulk run, after a single report is
/// generated, and whenever a subject mark is edited, since any one edit
/// reorders everybody.
/// </para>
/// <para>
/// A cohort is one (class, term, report type). Two report types for the same
/// term rank separately, which is what you want: a mid-year and a term 2 card
/// are different assessments of different work.
/// </para>
/// </summary>
public class ReportCohortStatisticsService : ITransientDependency
{
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IRepository<ReportSubject, Guid> _reportSubjectRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public Castle.Core.Logging.ILogger Logger { get; set; } = Castle.Core.Logging.NullLogger.Instance;

    public ReportCohortStatisticsService(
        IRepository<Report, Guid> reportRepository,
        IRepository<ReportSubject, Guid> reportSubjectRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _reportRepository = reportRepository;
        _reportSubjectRepository = reportSubjectRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// Recomputes the cohort figures for every report in one (class, term,
    /// report type) and saves them. Returns how many reports were touched.
    /// </summary>
    public async Task<int> RecalculateAsync(
        int? tenantId,
        Guid classId,
        Guid? termId,
        ReportType reportType)
    {
        var reports = await _reportRepository
            .GetAll()
            .Include(r => r.SubjectReports)
            .Where(r => r.TenantId == tenantId)
            .Where(r => r.ClassId == classId)
            .Where(r => r.TermId == termId)
            .Where(r => r.ReportType == reportType)
            .ToListAsync();

        if (reports.Count == 0)
            return 0;

        Apply(reports);

        foreach (var report in reports)
        {
            await _reportRepository.UpdateAsync(report);

            foreach (var subject in report.SubjectReports)
                await _reportSubjectRepository.UpdateAsync(subject);
        }

        await _unitOfWorkManager.Current.SaveChangesAsync();

        return reports.Count;
    }

    /// <summary>
    /// As <see cref="RecalculateAsync"/>, but never throws: a failure to work
    /// out class positions must not fail the generation run that produced the
    /// reports, or the mark edit that was otherwise saved. The figures are
    /// recomputed on the next edit or run.
    /// </summary>
    public async Task TryRecalculateAsync(
        int? tenantId,
        Guid classId,
        Guid? termId,
        ReportType reportType)
    {
        try
        {
            await RecalculateAsync(tenantId, classId, termId, reportType);
        }
        catch (Exception ex)
        {
            Logger.Error(
                $"Could not recalculate class statistics for class {classId}, term {termId}, {reportType}.",
                ex);
        }
    }

    /// <summary>
    /// The calculation itself, over reports already loaded with their subject
    /// rows. Separated from the loading and saving so it can be tested without
    /// a database.
    /// </summary>
    public static void Apply(IReadOnlyCollection<Report> reports)
    {
        if (reports == null || reports.Count == 0)
            return;

        // ── Overall: where each learner placed in the class.
        var overallPositions = CohortRanking.Positions(
            reports.Select(r => new KeyValuePair<Guid, decimal?>(r.Id, r.OverallPercentage)));

        foreach (var report in reports)
        {
            report.SetClassPosition(
                overallPositions.TryGetValue(report.Id, out var position)
                    ? position
                    : (int?)null);
        }

        // ── Per subject: the class figures, and where each learner placed in it.
        var rowsBySubject = reports
            .SelectMany(r => r.SubjectReports ?? Enumerable.Empty<ReportSubject>())
            .GroupBy(rs => rs.SubjectId);

        foreach (var subjectRows in rowsBySubject)
        {
            var rows = subjectRows.ToList();
            var summary = CohortRanking.Summarise(rows.Select(rs => rs.FinalMark));

            if (summary == null)
            {
                // Nobody in the class has a mark in this subject yet. Clear any
                // figures left from a previous run rather than leaving stale
                // ones next to a now-blank mark.
                foreach (var row in rows)
                    row.ClearClassStatistics();

                continue;
            }

            var positions = CohortRanking.Positions(
                rows.Select(rs => new KeyValuePair<Guid, decimal?>(rs.Id, rs.FinalMark)));

            foreach (var row in rows)
            {
                if (!row.FinalMark.HasValue)
                {
                    // An unmarked learner has no position, but the class
                    // figures still belong on their card — that is the
                    // comparison the card is for.
                    row.SetClassStatistics(null, summary.Value.Average, summary.Value.Highest, summary.Value.Lowest);
                    continue;
                }

                row.SetClassStatistics(
                    positions.TryGetValue(row.Id, out var position) ? position : (int?)null,
                    summary.Value.Average,
                    summary.Value.Highest,
                    summary.Value.Lowest);
            }
        }
    }
}
