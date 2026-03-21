using Abp.Application.Services;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Domain.Workflow.Entities;
using psms.Domain.Workflow.Enums;
using psms.Workflow.Shared;
using psms.Workflow.WorkflowDefinitions;
using psms.Workflow.WorkflowDefinitions.Dto;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace psms.Workflow.Seed;

[AbpAuthorize(PermissionNames.Workflow_Definitions_Create)]
public class WorkflowSeedService : ApplicationService
{
    private readonly IRepository<WorkflowDefinition, Guid> _definitionRepository;
    private readonly IRepository<WorkflowStep, Guid> _stepRepository;

    public WorkflowSeedService(
        IRepository<WorkflowDefinition, Guid> definitionRepository,
        IRepository<WorkflowStep, Guid> stepRepository)
    {
        _definitionRepository = definitionRepository;
        _stepRepository = stepRepository;
    }

    /// <summary>
    /// Seeds default workflow definitions for the current tenant.
    /// Skips any entity type that already has a definition.
    /// </summary>
    public async Task SeedDefaultsAsync()
    {
        await SeedAdmissionsWorkflow();
        await SeedReportApprovalWorkflow();
        await SeedFeeWaiverApprovalWorkflow();
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    private async Task SeedAdmissionsWorkflow()
    {
        var exists = await _definitionRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.EntityType == WorkflowEntityType.Application);

        if (exists) return;

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            Name = "Admissions Approval",
            Description = "Default admissions workflow for SA private schools. Covers the full application lifecycle from submission to enrollment.",
            EntityType = WorkflowEntityType.Application,
            IsActive = true
        };

        await _definitionRepository.InsertAsync(definition);

        var steps = new[]
        {
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 1,
                Name = "Admin Review",
                Description = "Admin reviews the submitted application for completeness.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 2,
                Name = "Document Verification",
                Description = "Verify all required documents are present and valid.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 3,
                Name = "Interview",
                Description = "Schedule and conduct parent/student interview.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 4,
                Name = "Committee Review",
                Description = "Admissions committee reviews the complete application.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 5,
                Name = "Principal Decision",
                Description = "Principal makes the final admission decision.",
                AssignedRole = "Principal",
                ActionType = WorkflowActionType.Approve,
                IsTerminal = true,
                NextStepOnReject = 4 // Send back to committee for reconsideration
            }
        };

        foreach (var step in steps)
        {
            await _stepRepository.InsertAsync(step);
        }
    }

    private async Task SeedReportApprovalWorkflow()
    {
        var exists = await _definitionRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.EntityType == WorkflowEntityType.Report);

        if (exists) return;

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            Name = "Report Approval",
            Description = "Report card approval workflow. HOD reviews then principal approves before publishing to parents.",
            EntityType = WorkflowEntityType.Report,
            IsActive = true
        };

        await _definitionRepository.InsertAsync(definition);

        var steps = new[]
        {
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 1,
                Name = "HOD Review",
                Description = "Head of Department reviews the report card for accuracy.",
                AssignedRole = "Teacher",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 2,
                Name = "Principal Approval",
                Description = "Principal approves the report card for publishing.",
                AssignedRole = "Principal",
                ActionType = WorkflowActionType.Approve,
                IsTerminal = true,
                NextStepOnReject = 1 // Send back to HOD
            }
        };

        foreach (var step in steps)
        {
            await _stepRepository.InsertAsync(step);
        }
    }

    private async Task SeedFeeWaiverApprovalWorkflow()
    {
        var exists = await _definitionRepository
            .GetAll()
            .AnyAsync(d => d.TenantId == AbpSession.TenantId
                && d.EntityType == WorkflowEntityType.FeeWaiver);

        if (exists) return;

        var definition = new WorkflowDefinition
        {
            Id = Guid.NewGuid(),
            TenantId = AbpSession.TenantId,
            Name = "Fee Waiver Approval",
            Description = "Fee waiver approval workflow. Finance reviews then principal approves the discount.",
            EntityType = WorkflowEntityType.FeeWaiver,
            IsActive = true
        };

        await _definitionRepository.InsertAsync(definition);

        var steps = new[]
        {
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 1,
                Name = "Finance Review",
                Description = "Finance department reviews the fee waiver request and supporting documents.",
                AssignedRole = "Admin",
                ActionType = WorkflowActionType.Review
            },
            new WorkflowStep
            {
                Id = Guid.NewGuid(),
                WorkflowDefinitionId = definition.Id,
                StepOrder = 2,
                Name = "Principal Approval",
                Description = "Principal approves or rejects the fee waiver.",
                AssignedRole = "Principal",
                ActionType = WorkflowActionType.Approve,
                IsTerminal = true
            }
        };

        foreach (var step in steps)
        {
            await _stepRepository.InsertAsync(step);
        }
    }
}
