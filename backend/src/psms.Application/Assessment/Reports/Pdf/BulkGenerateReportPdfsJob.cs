using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;

namespace psms.Assessment.Reports.Pdf;

/// <summary>
/// Background job that fans out individual PDF generation jobs for each report.
/// </summary>
public class BulkGenerateReportPdfsJob : AsyncBackgroundJob<BulkGenerateReportPdfsJobArgs>, ITransientDependency
{
    private readonly IBackgroundJobManager _backgroundJobManager;

    public BulkGenerateReportPdfsJob(IBackgroundJobManager backgroundJobManager)
    {
        _backgroundJobManager = backgroundJobManager;
    }

    public override async Task ExecuteAsync(BulkGenerateReportPdfsJobArgs args)
    {
        foreach (var reportId in args.ReportIds)
        {
            await _backgroundJobManager.EnqueueAsync<GenerateReportPdfJob, GenerateReportPdfJobArgs>(
                new GenerateReportPdfJobArgs
                {
                    ReportId = reportId,
                    TenantId = args.TenantId,
                    UserId = args.UserId,
                });
        }

        Logger.Info($"Enqueued {args.ReportIds.Count} individual PDF generation jobs.");
    }
}
