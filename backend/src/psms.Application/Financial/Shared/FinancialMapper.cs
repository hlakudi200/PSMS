using AutoMapper;
using psms.Domain.Financial.Entities;
using psms.Financial.FeeStructures.Dto;
using psms.Financial.PaymentAllocations.Dto;
using psms.Financial.Payments.Dto;
using psms.Financial.StudentFees.Dto;

namespace psms.Financial.Shared;

/// <summary>
/// AutoMapper profile for the Financial module.
/// Maps between domain entities and DTOs.
/// </summary>
public class FinancialMapper : Profile
{
    public FinancialMapper()
    {
        CreateFeeStructureMappings();
        CreateStudentFeeMappings();
        CreatePaymentMappings();
        CreatePaymentAllocationMappings();
    }

    private void CreateFeeStructureMappings()
    {
        // Entity to DTO (full)
        CreateMap<FeeStructure, FeeStructureDto>()
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.FormattedAmount,
                opt => opt.MapFrom(src => src.GetFormattedAmount()))
            .ForMember(dest => dest.StudentFeeCount,
                opt => opt.MapFrom(src => src.StudentFees != null ? src.StudentFees.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<FeeStructure, FeeStructureListDto>()
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.FormattedAmount,
                opt => opt.MapFrom(src => src.GetFormattedAmount()))
            .ForMember(dest => dest.StudentFeeCount,
                opt => opt.MapFrom(src => src.StudentFees != null ? src.StudentFees.Count : 0));
    }

    private void CreateStudentFeeMappings()
    {
        // Entity to DTO (full)
        CreateMap<StudentFee, StudentFeeDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.FeeStructureName,
                opt => opt.MapFrom(src => src.FeeStructure != null ? src.FeeStructure.FeeName : null))
            .ForMember(dest => dest.FeeType,
                opt => opt.MapFrom(src => src.FeeStructure != null ? src.FeeStructure.FeeType : default))
            .ForMember(dest => dest.OutstandingBalance,
                opt => opt.MapFrom(src => src.GetOutstandingBalance()))
            .ForMember(dest => dest.FormattedAmountDue,
                opt => opt.MapFrom(src => src.FeeStructure != null
                    ? src.FeeStructure.Currency + " " + src.AmountDue.ToString("N2")
                    : "ZAR " + src.AmountDue.ToString("N2")))
            .ForMember(dest => dest.AllocationCount,
                opt => opt.MapFrom(src => src.PaymentAllocations != null ? src.PaymentAllocations.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<StudentFee, StudentFeeListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.FeeStructureName,
                opt => opt.MapFrom(src => src.FeeStructure != null ? src.FeeStructure.FeeName : null))
            .ForMember(dest => dest.FeeType,
                opt => opt.MapFrom(src => src.FeeStructure != null ? src.FeeStructure.FeeType : default))
            .ForMember(dest => dest.OutstandingBalance,
                opt => opt.MapFrom(src => src.GetOutstandingBalance()));
    }

    private void CreatePaymentMappings()
    {
        // Entity to DTO (full)
        CreateMap<Payment, PaymentDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.ParentName,
                opt => opt.MapFrom(src => src.Parent != null
                    ? src.Parent.FirstName + " " + src.Parent.LastName : null))
            .ForMember(dest => dest.FormattedAmount,
                opt => opt.MapFrom(src => src.GetFormattedAmount()))
            .ForMember(dest => dest.TotalAllocated,
                opt => opt.MapFrom(src => src.GetTotalAllocated()))
            .ForMember(dest => dest.UnallocatedAmount,
                opt => opt.MapFrom(src => src.GetUnallocatedAmount()))
            .ForMember(dest => dest.AllocationCount,
                opt => opt.MapFrom(src => src.PaymentAllocations != null ? src.PaymentAllocations.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<Payment, PaymentListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null
                    ? src.Student.FirstName + " " + src.Student.LastName : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.ParentName,
                opt => opt.MapFrom(src => src.Parent != null
                    ? src.Parent.FirstName + " " + src.Parent.LastName : null))
            .ForMember(dest => dest.FormattedAmount,
                opt => opt.MapFrom(src => src.GetFormattedAmount()))
            .ForMember(dest => dest.TotalAllocated,
                opt => opt.MapFrom(src => src.GetTotalAllocated()))
            .ForMember(dest => dest.UnallocatedAmount,
                opt => opt.MapFrom(src => src.GetUnallocatedAmount()))
            .ForMember(dest => dest.AllocationCount,
                opt => opt.MapFrom(src => src.PaymentAllocations != null ? src.PaymentAllocations.Count : 0));
    }

    private void CreatePaymentAllocationMappings()
    {
        // Entity to DTO (full)
        CreateMap<PaymentAllocation, PaymentAllocationDto>()
            .ForMember(dest => dest.ReceiptNumber,
                opt => opt.MapFrom(src => src.Payment != null ? src.Payment.ReceiptNumber : null))
            .ForMember(dest => dest.PaymentStatus,
                opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Status : default))
            .ForMember(dest => dest.PaymentAmount,
                opt => opt.MapFrom(src => src.Payment != null ? src.Payment.Amount : 0))
            .ForMember(dest => dest.FeeStructureName,
                opt => opt.MapFrom(src => src.StudentFee != null && src.StudentFee.FeeStructure != null
                    ? src.StudentFee.FeeStructure.FeeName : null))
            .ForMember(dest => dest.StudentFeeAmountDue,
                opt => opt.MapFrom(src => src.StudentFee != null ? src.StudentFee.AmountDue : 0))
            .ForMember(dest => dest.StudentFeeOutstandingBalance,
                opt => opt.MapFrom(src => src.StudentFee != null ? src.StudentFee.GetOutstandingBalance() : 0))
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.StudentFee != null && src.StudentFee.Student != null
                    ? src.StudentFee.Student.FirstName + " " + src.StudentFee.Student.LastName : null));

        // Entity to ListDto (lightweight)
        CreateMap<PaymentAllocation, PaymentAllocationListDto>()
            .ForMember(dest => dest.ReceiptNumber,
                opt => opt.MapFrom(src => src.Payment != null ? src.Payment.ReceiptNumber : null))
            .ForMember(dest => dest.FeeStructureName,
                opt => opt.MapFrom(src => src.StudentFee != null && src.StudentFee.FeeStructure != null
                    ? src.StudentFee.FeeStructure.FeeName : null))
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.StudentFee != null && src.StudentFee.Student != null
                    ? src.StudentFee.Student.FirstName + " " + src.StudentFee.Student.LastName : null));
    }
}
