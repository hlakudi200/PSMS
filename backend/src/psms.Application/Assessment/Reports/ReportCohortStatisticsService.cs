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
/// RC-06. Fills in the figures on a report card that only exist relative to the
/// rest of the class: the learner's position overall, the size of the cohort
/// they were ranked against, and per subject their position, the class average,
/// the highest and the lowest mark.
/// <para>
/// These cannot be computed while generating one learner — there is no cohort
/// to compare against yet — so this runs as a pass over the whole class once
/// its reports exist. It is called after a bulk run, after a single report is
/// generated, whenever a subject mark is edited, and after a report is deleted,
/// since each of those changes who is in the cohort or where they sit in it.
/// </para>
/// <para>
/// A cohort is one (class, term, report type). Two report types for the same
/// term rank separately, which is what you want: a mid-year and a term 2 card
/// assess different work.
/// </para>
/// </summary>
public class ReportCohortStatisticsService : ITransientDependency
{
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly IUnitOfWorkManager _unitOfWorkManager;

    public Castle.Core.Logging.ILogger Logger { get; set; } = Castle.Core.Logging.NullLogger.Instance;

    public ReportCohortStatisticsService(
        IRepository<Report, Guid> reportRepository,
        IUnitOfWorkManager unitOfWorkManager)
    {
        _reportRepository = reportRepository;
        _unitOfWorkManager = unitOfWorkManager;
    }

    /// <summary>
    /// Recomputes the cohort figures for every report in one (class, term,
    /// report type) and saves them. Returns how many reports were restated —
    /// which is fewer than the cohort when some of it is already approved or
    /// published.
    /// <para>
    /// Runs in the caller's unit of work, so it sees the caller's uncommitted
    /// writes and fails with them. The caller must have flushed those writes
    /// (SaveChangesAsync) first, or the pass will read the class as it was
    /// before the edit.
    /// </para>
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

        var restated = Apply(reports);

        // No UpdateAsync: every entity here was loaded by this unit of work and
        // is tracked by it, so the change tracker writes exactly the rows Apply
        // actually altered. Marking them all Modified instead — which ABP's
        // Update does unconditionally — rewrote the whole class on every pass
        // and stamped LastModifierUserId on forty untouched report cards
        // because one teacher saved one mark.
        await _unitOfWorkManager.Current.SaveChangesAsync();

        return restated;
    }

    /// <summary>
    /// As <see cref="RecalculateAsync"/>, but contained — for a caller whose
    /// own work is <b>already committed</b> and must not be undone by a failure
    /// to work out class positions. That is the bulk generation run: each
    /// learner commits in its own unit of work, and forty saved report cards
    /// must not be reported as a failed request because the ranking pass fell
    /// over. The figures are recomputed on the next edit or run.
    /// <para>
    /// It takes its own unit of work so a failure rolls back only its own
    /// writes; swallowing the exception on the caller's unit of work would
    /// leave mutated entities tracked on a transaction the database had already
    /// failed, and the same exception would resurface when the caller completed
    /// — the outcome catching it exists to prevent.
    /// </para>
    /// <para>
    /// Because that new unit of work is a separate transaction, it cannot see
    /// the caller's <i>uncommitted</i> writes. A caller still inside its own
    /// transaction — a single generation, a mark edit, a delete — must call
    /// <see cref="RecalculateAsync"/> instead, and let a failure roll the whole
    /// operation back, which is the coherent outcome there: either the edit and
    /// its restatement both happen, or neither does.
    /// </para>
    /// </summary>
    public async Task TryRecalculateAsync(
        int? tenantId,
        Guid classId,
        Guid? termId,
        ReportType reportType)
    {
        try
        {
            using (var uow = _unitOfWorkManager.Begin(new UnitOfWorkOptions
            {
                Scope = System.Transactions.TransactionScopeOption.RequiresNew
            }))
            {
                await RecalculateAsync(tenantId, classId, termId, reportType);
                await uow.CompleteAsync();
            }
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
    /// a database. Returns how many reports it restated.
    /// <para>
    /// Every report passed in counts towards the ranking and the class figures;
    /// only reports that are still open to restatement are written to. A
    /// published classmate's mark is a real mark and belongs in the class
    /// average, but their card has been issued and must not be rewritten.
    /// </para>
    /// </summary>
    public static int Apply(IReadOnlyCollection<Report> reports)
    {
        if (reports == null || reports.Count == 0)
            return 0;

        var writable = reports.Where(r => !r.IsLockedForRestatement()).ToList();
        if (writable.Count == 0)
            return 0;

        // ── Overall: where each learner placed in the class, out of how many.
        var overallPositions = CohortRanking.Positions(
            reports.Select(r => new KeyValuePair<Guid, decimal?>(r.Id, r.OverallPercentage)));

        foreach (var report in writable)
        {
            report.SetClassPosition(
                overallPositions.TryGetValue(report.Id, out var position)
                    ? position
                    : (int?)null);

            // The denominator is the cohort actually ranked, so "3 of 30" is
            // internally consistent. It used to be a class headcount taken at
            // generation time, which drifted as learners moved in and out and
            // could print a position larger than the class.
            report.SetCohortSize(reports.Count);
        }

        // ── Per subject: the class figures, and where each learner placed in it.
        var rowsBySubject = reports
            .SelectMany(r => (r.SubjectReports ?? Enumerable.Empty<ReportSubject>())
                .Select(rs => new { Report = r, Row = rs }))
            .GroupBy(x => x.Row.SubjectId);

        foreach (var subjectRows in rowsBySubject)
        {
            var all = subjectRows.ToList();
            var summary = CohortRanking.Summarise(all.Select(x => x.Row.FinalMark));
            var writableRows = all.Where(x => !x.Report.IsLockedForRestatement()).ToList();

            if (summary == null)
            {
                // Nobody in the class has a mark in this subject. Clear any
                // figures left from a previous run rather than leaving stale
                // ones next to a now-blank mark.
                foreach (var entry in writableRows)
                    entry.Row.ClearClassStatistics();

                continue;
            }

            var positions = CohortRanking.Positions(
                all.Select(x => new KeyValuePair<Guid, decimal?>(x.Row.Id, x.Row.FinalMark)));

            foreach (var entry in writableRows)
            {
                var row = entry.Row;

                // An unmarked learner has no position, but the class figures
                // still belong on their card — that is the comparison the card
                // is for.
                row.SetClassStatistics(
                    row.FinalMark.HasValue && positions.TryGetValue(row.Id, out var position)
                        ? position
                        : (int?)null,
                    summary.Value.Average,
                    summary.Value.Highest,
                    summary.Value.Lowest);
            }
        }

        return writable.Count;
    }
}
