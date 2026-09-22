using System;
using System.Threading.Tasks;
using Abp.BackgroundJobs;
using Abp.Dependency;
using Abp.Domain.Entities;
using Abp.Domain.Repositories;
using Abp.Domain.Uow;
using psms.Domain.Assessment.Entities;
using psms.Domain.Shared.Storage;

namespace psms.Assessment.Reports.Pdf;

/// <summary>
/// Background job that generates a PDF for a single report, uploads to Supabase, and updates the entity.
/// </summary>
public class GenerateReportPdfJob : AsyncBackgroundJob<GenerateReportPdfJobArgs>, ITransientDependency
{
    private readonly IRepository<Report, Guid> _reportRepository;
    private readonly ReportPdfDataLoader _dataLoader;
    private readonly IFileStorageService _storageService;

    public GenerateReportPdfJob(
        IRepository<Report, Guid> reportRepository,
        ReportPdfDataLoader dataLoader,
        IFileStorageService storageService)
    {
        _reportRepository = reportRepository;
        _dataLoader = dataLoader;
        _storageService = storageService;
    }

    [UnitOfWork]
    public override async Task ExecuteAsync(GenerateReportPdfJobArgs args)
    {
        // Disable tenant filter — background jobs run without session tenant context
        using (CurrentUnitOfWork.DisableFilter(AbpDataFilters.MayHaveTenant))
        {
            // Load report data — pass tenantId explicitly, the loader filters by it
            var data = await _dataLoader.LoadAsync(args.ReportId, args.TenantId);
            if (data == null)
            {
                Logger.Warn($"Report {args.ReportId} not found for PDF generation.");
                return;
            }

            // Generate PDF bytes
            var pdfBytes = ReportPdfGenerator.Generate(data);

            // Build storage path
            var report = await _reportRepository.GetAsync(args.ReportId);
            var storagePath = $"reports/{args.TenantId ?? 0}/{report.AcademicYearId}/{report.TermId ?? Guid.Empty}/{data.AdmissionNumber ?? "unknown"}_{args.ReportId}.pdf";

            // Private upload: the key is kept, and a short-lived signed URL is
            // minted per request. Nothing durable points at a child's report.
            var objectKey = await _storageService.UploadAsync(storagePath, pdfBytes, "application/pdf");

            report.SetPdfObjectKey(objectKey);
            await _reportRepository.UpdateAsync(report);
            await CurrentUnitOfWork.SaveChangesAsync();
        }
    }
}
