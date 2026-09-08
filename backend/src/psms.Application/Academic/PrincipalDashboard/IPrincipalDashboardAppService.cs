using Abp.Application.Services;
using psms.Academic.PrincipalDashboard.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Academic.PrincipalDashboard;

public interface IPrincipalDashboardAppService : IApplicationService
{
    /// <summary>
    /// Aggregated counts and per-grade performance for the principal dashboard.
    /// When <paramref name="academicYearId"/> is supplied, classes, reports and
    /// per-grade student counts are scoped to that year; the attention queue is
    /// always school-wide.
    /// </summary>
    Task<PrincipalDashboardSummaryDto> GetSummaryAsync(Guid? academicYearId = null);
}
