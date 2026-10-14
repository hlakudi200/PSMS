using System.Collections.Generic;
using System.Threading.Tasks;
using Abp.Application.Services;
using psms.Financial.Statements.Dto;

namespace psms.Financial.Statements;

/// <summary>
/// MOB-BE-01 (#215): fee statements a parent can read for their own children,
/// and staff can read for any learner.
/// </summary>
public interface IFeeStatementAppService : IApplicationService
{
    /// <summary>The statement for the requested learner (or, for a parent, the whole family) and period.</summary>
    Task<FeeStatementDto> GetStatementAsync(GetFeeStatementInput input);

    /// <summary>The same statement as a PDF; returns a short-lived signed download link.</summary>
    Task<string> GenerateStatementPdfAsync(GetFeeStatementInput input);

    /// <summary>The academic years and terms a statement can be drawn for, newest first.</summary>
    Task<List<FeeStatementYearDto>> GetPeriodsAsync();
}
