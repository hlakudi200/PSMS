using AutoMapper;
using psms.Domain.Workflow.Entities;
using psms.Workflow.WorkflowDefinitions.Dto;
using psms.Workflow.WorkflowDelegations.Dto;
using psms.Workflow.WorkflowInstances.Dto;
using psms.Workflow.WorkflowSteps.Dto;
using System.Linq;

namespace psms.Workflow.Shared;

public class WorkflowMapper : Profile
{
    public WorkflowMapper()
    {
        CreateDefinitionMappings();
        CreateStepMappings();
        CreateInstanceMappings();
        CreateTransitionMappings();
        CreateDelegationMappings();
    }

    private void CreateDefinitionMappings()
    {
        CreateMap<WorkflowDefinition, WorkflowDefinitionDto>();

        CreateMap<WorkflowDefinition, WorkflowDefinitionListDto>()
            .ForMember(dest => dest.StepCount, opt => opt.MapFrom(src => src.Steps.Count));
    }

    private void CreateStepMappings()
    {
        CreateMap<WorkflowStep, WorkflowStepDto>();
    }

    private void CreateInstanceMappings()
    {
        CreateMap<WorkflowInstance, WorkflowInstanceDto>()
            .ForMember(dest => dest.WorkflowDefinitionName,
                opt => opt.MapFrom(src => src.WorkflowDefinition != null ? src.WorkflowDefinition.Name : null))
            .ForMember(dest => dest.CurrentStepName,
                opt => opt.MapFrom(src => src.CurrentStep != null ? src.CurrentStep.Name : null))
            .ForMember(dest => dest.CurrentStepAssignedRole,
                opt => opt.MapFrom(src => src.CurrentStep != null ? src.CurrentStep.AssignedRole : null))
            .ForMember(dest => dest.IsOverdue,
                opt => opt.MapFrom(src => src.IsOverdue));

        CreateMap<WorkflowInstance, WorkflowInstanceListDto>()
            .ForMember(dest => dest.WorkflowDefinitionName,
                opt => opt.MapFrom(src => src.WorkflowDefinition != null ? src.WorkflowDefinition.Name : null))
            .ForMember(dest => dest.CurrentStepName,
                opt => opt.MapFrom(src => src.CurrentStep != null ? src.CurrentStep.Name : null))
            .ForMember(dest => dest.CurrentStepAssignedRole,
                opt => opt.MapFrom(src => src.CurrentStep != null ? src.CurrentStep.AssignedRole : null))
            .ForMember(dest => dest.IsOverdue,
                opt => opt.MapFrom(src => src.IsOverdue));
    }

    private void CreateTransitionMappings()
    {
        CreateMap<WorkflowTransition, WorkflowTransitionDto>()
            .ForMember(dest => dest.FromStepName,
                opt => opt.MapFrom(src => src.FromStep != null ? src.FromStep.Name : null))
            .ForMember(dest => dest.ToStepName,
                opt => opt.MapFrom(src => src.ToStep != null ? src.ToStep.Name : null));
    }

    private void CreateDelegationMappings()
    {
        CreateMap<WorkflowDelegation, WorkflowDelegationDto>()
            .ForMember(dest => dest.IsEffective,
                opt => opt.MapFrom(src => src.IsEffective));
    }
}
