using AutoMapper;
using psms.Academic.AcademicYears.Dto;
using psms.Academic.Classes.Dto;
using psms.Academic.ClassSubjects.Dto;
using psms.Academic.EmergencyContacts.Dto;
using psms.Academic.Grades.Dto;
using psms.Academic.GradeSubjects.Dto;
using psms.Academic.MedicalInfos.Dto;
using psms.Academic.Parents.Dto;
using psms.Academic.POPIAConsents.Dto;
using psms.Academic.Attendances.Dto;
using psms.Academic.StudentClasses.Dto;
using psms.Academic.StudentParents.Dto;
using psms.Academic.StudentTransfers.Dto;
using psms.Academic.TermEvents.Dto;
using psms.Academic.Timetables.Dto;
using psms.Academic.TimetableSlots.Dto;
using psms.Academic.Students.Dto;
using psms.Academic.StudentSubjects.Dto;
using psms.Academic.Subjects.Dto;
using psms.Academic.TeacherClasses.Dto;
using psms.Academic.Teachers.Dto;
using psms.Academic.TeacherSubjects.Dto;
using psms.Academic.Terms.Dto;
using psms.Domain.Academic.Entities;
using psms.Domain.Shared.ValueObjects;
using psms.Shared;
using System;
using System.Linq;

namespace psms.Academic.Shared;

/// <summary>
/// AutoMapper profile for the Academic module.
/// Maps between domain entities and DTOs.
/// </summary>
public class AcademicMapper : Profile
{
    public AcademicMapper()
    {
        CreateGradeMappings();
        CreateAcademicYearMappings();
        CreateTermMappings();
        CreateSubjectMappings();
        CreateGradeSubjectMappings();
        CreateTeacherMappings();
        CreateParentMappings();
        CreateClassMappings();
        CreateStudentMappings();
        CreateStudentParentMappings();
        CreateTeacherSubjectMappings();
        CreateTeacherClassMappings();
        CreateStudentSubjectMappings();
        CreateClassSubjectMappings();
        CreateEmergencyContactMappings();
        CreateMedicalInfoMappings();
        CreatePOPIAConsentMappings();
        CreateStudentClassMappings();
        CreateTermEventMappings();
        CreateAttendanceMappings();
        CreateTimetableMappings();
        CreateTimetableSlotMappings();
        CreateStudentTransferMappings();
    }

    private void CreateGradeMappings()
    {
        // Entity to DTO (full)
        CreateMap<Grade, GradeDto>()
            .ForMember(dest => dest.ClassCount,
                opt => opt.MapFrom(src => src.Classes != null ? src.Classes.Count(c => !c.IsDeleted) : 0))
            .ForMember(dest => dest.StudentCount,
                opt => opt.MapFrom(src => src.Students != null ? src.Students.Count(s => !s.IsDeleted) : 0))
            .ForMember(dest => dest.SubjectCount,
                opt => opt.MapFrom(src => src.GradeSubjects != null ? src.GradeSubjects.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<Grade, GradeListDto>()
            .ForMember(dest => dest.ClassCount,
                opt => opt.MapFrom(src => src.Classes != null ? src.Classes.Count(c => !c.IsDeleted) : 0))
            .ForMember(dest => dest.StudentCount,
                opt => opt.MapFrom(src => src.Students != null ? src.Students.Count(s => !s.IsDeleted) : 0));

        // CreateDto to Entity
        CreateMap<CreateGradeDto, Grade>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Classes, opt => opt.Ignore())
            .ForMember(dest => dest.Students, opt => opt.Ignore())
            .ForMember(dest => dest.GradeSubjects, opt => opt.Ignore());

        // UpdateDto to Entity (null-skip)
        CreateMap<UpdateGradeDto, Grade>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));
    }

    private void CreateAcademicYearMappings()
    {
        // Entity to DTO
        CreateMap<AcademicYear, AcademicYearDto>()
            .ForMember(dest => dest.TermCount,
                opt => opt.MapFrom(src => src.Terms != null ? src.Terms.Count : 0))
            .ForMember(dest => dest.ClassCount,
                opt => opt.MapFrom(src => src.Classes != null ? src.Classes.Count(c => !c.IsDeleted) : 0))
            .ForMember(dest => dest.Terms, opt => opt.Ignore()); // Mapped manually in service

        // Entity to ListDto
        CreateMap<AcademicYear, AcademicYearListDto>()
            .ForMember(dest => dest.TermCount,
                opt => opt.MapFrom(src => src.Terms != null ? src.Terms.Count : 0))
            .ForMember(dest => dest.ClassCount,
                opt => opt.MapFrom(src => src.Classes != null ? src.Classes.Count(c => !c.IsDeleted) : 0));
    }

    private void CreateTermMappings()
    {
        // Entity to DTO
        CreateMap<Term, TermDto>()
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));

        // Entity to ListDto
        CreateMap<Term, TermListDto>()
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));
    }

    private void CreateSubjectMappings()
    {
        // Entity to DTO (full)
        CreateMap<Subject, SubjectDto>()
            .ForMember(dest => dest.GradeCount,
                opt => opt.MapFrom(src => src.GradeSubjects != null ? src.GradeSubjects.Count : 0))
            .ForMember(dest => dest.TeacherCount,
                opt => opt.MapFrom(src => src.TeacherSubjects != null ? src.TeacherSubjects.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<Subject, SubjectListDto>()
            .ForMember(dest => dest.GradeCount,
                opt => opt.MapFrom(src => src.GradeSubjects != null ? src.GradeSubjects.Count : 0));
    }

    private void CreateGradeSubjectMappings()
    {
        // Entity to DTO with flattened navigation properties
        CreateMap<GradeSubject, GradeSubjectDto>()
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null))
            .ForMember(dest => dest.GradeLevel,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeLevel : default))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.SubjectCode,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : null))
            .ForMember(dest => dest.IsCore,
                opt => opt.MapFrom(src => src.Subject != null && src.Subject.IsCore));
    }

    private void CreateTeacherMappings()
    {
        // Address value object to DTO
        CreateMap<Address, AddressDto>();

        // Entity to DTO (full)
        CreateMap<Teacher, TeacherDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.SubjectAssignmentCount,
                opt => opt.MapFrom(src => src.SubjectAssignments != null ? src.SubjectAssignments.Count : 0))
            .ForMember(dest => dest.ClassAssignmentCount,
                opt => opt.MapFrom(src => src.ClassAssignments != null ? src.ClassAssignments.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<Teacher, TeacherListDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.SubjectAssignmentCount,
                opt => opt.MapFrom(src => src.SubjectAssignments != null ? src.SubjectAssignments.Count : 0))
            .ForMember(dest => dest.ClassAssignmentCount,
                opt => opt.MapFrom(src => src.ClassAssignments != null ? src.ClassAssignments.Count : 0));
    }

    private void CreateParentMappings()
    {
        // Entity to DTO (full)
        CreateMap<Parent, ParentDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.StudentCount,
                opt => opt.MapFrom(src => src.StudentLinks != null ? src.StudentLinks.Count : 0))
            .ForMember(dest => dest.IdNumber, opt => opt.MapFrom(src => PiiMasking.Mask(src.IdNumber)));

        // Entity to ListDto (lightweight)
        CreateMap<Parent, ParentListDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.StudentCount,
                opt => opt.MapFrom(src => src.StudentLinks != null ? src.StudentLinks.Count : 0));
    }

    private void CreateClassMappings()
    {
        // Entity to DTO (full)
        CreateMap<Class, ClassDto>()
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.ClassTeacherName,
                opt => opt.MapFrom(src => src.ClassTeacher != null ? src.ClassTeacher.GetFullName() : null))
            .ForMember(dest => dest.StudentCount,
                opt => opt.MapFrom(src => src.Students != null ? src.Students.Count(s => !s.IsDeleted) : 0))
            .ForMember(dest => dest.TeacherAssignmentCount,
                opt => opt.MapFrom(src => src.TeacherAssignments != null ? src.TeacherAssignments.Count : 0));

        // Entity to ListDto (lightweight)
        CreateMap<Class, ClassListDto>()
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.ClassTeacherName,
                opt => opt.MapFrom(src => src.ClassTeacher != null ? src.ClassTeacher.GetFullName() : null))
            .ForMember(dest => dest.StudentCount,
                opt => opt.MapFrom(src => src.Students != null ? src.Students.Count(s => !s.IsDeleted) : 0));
    }

    private void CreateStudentMappings()
    {
        // Entity to DTO (full)
        CreateMap<Student, StudentDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.Age,
                opt => opt.MapFrom(src => src.GetAge()))
            .ForMember(dest => dest.CurrentGradeName,
                opt => opt.MapFrom(src => src.CurrentGrade != null ? src.CurrentGrade.GradeName : null))
            .ForMember(dest => dest.CurrentClassName,
                opt => opt.MapFrom(src => src.CurrentClass != null ? src.CurrentClass.ClassName : null))
            .ForMember(dest => dest.ParentCount,
                opt => opt.MapFrom(src => src.ParentLinks != null ? src.ParentLinks.Count : 0))
            .ForMember(dest => dest.SubjectCount,
                opt => opt.MapFrom(src => src.SubjectEnrollments != null ? src.SubjectEnrollments.Count : 0))
            .ForMember(dest => dest.IdNumber, opt => opt.MapFrom(src => PiiMasking.Mask(src.IdNumber)))
            .ForMember(dest => dest.PassportNumber, opt => opt.MapFrom(src => PiiMasking.Mask(src.PassportNumber)));

        // Entity to ListDto (lightweight)
        CreateMap<Student, StudentListDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.Age,
                opt => opt.MapFrom(src => src.GetAge()))
            .ForMember(dest => dest.CurrentGradeName,
                opt => opt.MapFrom(src => src.CurrentGrade != null ? src.CurrentGrade.GradeName : null))
            .ForMember(dest => dest.CurrentClassName,
                opt => opt.MapFrom(src => src.CurrentClass != null ? src.CurrentClass.ClassName : null))
            .ForMember(dest => dest.ParentCount,
                opt => opt.MapFrom(src => src.ParentLinks != null ? src.ParentLinks.Count : 0));
    }

    private void CreateStudentParentMappings()
    {
        CreateMap<StudentParent, StudentParentDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.ParentName,
                opt => opt.MapFrom(src => src.Parent != null ? src.Parent.GetFullName() : null))
            .ForMember(dest => dest.ParentUserId,
                opt => opt.MapFrom(src => src.Parent != null ? src.Parent.UserId : 0))
            .ForMember(dest => dest.ParentEmail,
                opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Email : null))
            .ForMember(dest => dest.ParentPhone,
                opt => opt.MapFrom(src => src.Parent != null ? src.Parent.Phone : null));
    }

    private void CreateTeacherSubjectMappings()
    {
        CreateMap<TeacherSubject, TeacherSubjectDto>()
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.SubjectCode,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : null))
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Grade != null ? src.Grade.GradeName : null));
    }

    private void CreateTeacherClassMappings()
    {
        CreateMap<TeacherClass, TeacherClassDto>()
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : null))
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.SubjectCode,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : null));
    }

    private void CreateStudentSubjectMappings()
    {
        CreateMap<StudentSubject, StudentSubjectDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.StudentAdmissionNumber,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.AdmissionNumber : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.SubjectCode,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));
    }

    private void CreateClassSubjectMappings()
    {
        // Entity to DTO (full)
        CreateMap<ClassSubject, ClassSubjectDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.GradeId,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.GradeId : Guid.Empty))
            .ForMember(dest => dest.GradeName,
                opt => opt.MapFrom(src => src.Class != null && src.Class.Grade != null ? src.Class.Grade.GradeName : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.SubjectCode,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : null))
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : null));

        // Entity to ListDto (lightweight)
        CreateMap<ClassSubject, ClassSubjectListDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.SubjectCode,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectCode : null))
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : null));
    }

    private void CreateEmergencyContactMappings()
    {
        CreateMap<EmergencyContact, EmergencyContactDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.GetFullName()))
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null));

        CreateMap<EmergencyContact, EmergencyContactListDto>()
            .ForMember(dest => dest.FullName,
                opt => opt.MapFrom(src => src.GetFullName()));
    }

    private void CreateMedicalInfoMappings()
    {
        CreateMap<MedicalInfo, MedicalInfoDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null));
    }

    private void CreatePOPIAConsentMappings()
    {
        CreateMap<POPIAConsent, POPIAConsentDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null));
    }

    private void CreateStudentClassMappings()
    {
        CreateMap<StudentClass, StudentClassDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));

        CreateMap<StudentClass, StudentClassListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null));
    }

    private void CreateTermEventMappings()
    {
        CreateMap<TermEvent, TermEventDto>()
            .ForMember(dest => dest.TermName,
                opt => opt.MapFrom(src => src.Term != null ? src.Term.TermName : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.Term != null && src.Term.AcademicYear != null
                    ? src.Term.AcademicYear.YearName : null));

        CreateMap<TermEvent, TermEventListDto>()
            .ForMember(dest => dest.TermName,
                opt => opt.MapFrom(src => src.Term != null ? src.Term.TermName : null));
    }

    private void CreateAttendanceMappings()
    {
        CreateMap<Attendance, AttendanceDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : null))
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null));

        CreateMap<Attendance, AttendanceListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null));
    }

    private void CreateTimetableMappings()
    {
        CreateMap<Timetable, TimetableDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.SlotCount,
                opt => opt.MapFrom(src => src.TimetableSlots != null ? src.TimetableSlots.Count : 0));

        CreateMap<Timetable, TimetableListDto>()
            .ForMember(dest => dest.ClassName,
                opt => opt.MapFrom(src => src.Class != null ? src.Class.ClassName : null))
            .ForMember(dest => dest.SlotCount,
                opt => opt.MapFrom(src => src.TimetableSlots != null ? src.TimetableSlots.Count : 0));
    }

    private void CreateTimetableSlotMappings()
    {
        CreateMap<TimetableSlot, TimetableSlotDto>()
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : null));

        CreateMap<TimetableSlot, TimetableSlotListDto>()
            .ForMember(dest => dest.SubjectName,
                opt => opt.MapFrom(src => src.Subject != null ? src.Subject.SubjectName : null))
            .ForMember(dest => dest.TeacherName,
                opt => opt.MapFrom(src => src.Teacher != null ? src.Teacher.GetFullName() : null));
    }

    private void CreateStudentTransferMappings()
    {
        // Entity to DTO (full)
        CreateMap<StudentTransferRequest, StudentTransferDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null))
            .ForMember(dest => dest.AcademicYearName,
                opt => opt.MapFrom(src => src.AcademicYear != null ? src.AcademicYear.YearName : null))
            .ForMember(dest => dest.TransferGradeName,
                opt => opt.MapFrom(src => src.TransferGrade != null ? src.TransferGrade.GradeName : null));

        // Entity to ListDto (lightweight)
        CreateMap<StudentTransferRequest, StudentTransferListDto>()
            .ForMember(dest => dest.StudentName,
                opt => opt.MapFrom(src => src.Student != null ? src.Student.GetFullName() : null));
    }
}
