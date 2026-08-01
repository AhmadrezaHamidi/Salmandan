using Care.Domain.Caregiver.Enums;
using Care.Domain.Common.Enums;

namespace Caregiver.Application.Commands;

/// <summary>ثبت پرستار جدید توسط مؤسسه — در انتظار احراز صلاحیت</summary>
public record RegisterCaregiverCommand(
    [property: JsonIgnore] long AgencyId,
    string         PhoneNumber,
    string         FullName,
    string         NationalId,
    Gender         Gender,
    int            BirthYear,
    CaregiverLevel Level) : ICommand<long>;

/// <summary>ثبت مدارک احراز صلاحیت پرستار</summary>
public record SubmitCaregiverDocumentsCommand(
    [property: JsonIgnore] long CaregiverId,
    bool HasCriminalRecordCheck,
    bool HasCertificate,
    bool HasLiabilityInsurance) : ICommand<bool>;

/// <summary>تأیید صلاحیت پرستار — نیازمند گواهی عدم سوءپیشینه</summary>
public record VerifyCaregiverCommand([property: JsonIgnore] long CaregiverId) : ICommand<bool>;

/// <summary>رد صلاحیت پرستار</summary>
public record RejectCaregiverCommand([property: JsonIgnore] long CaregiverId, string Reason) : ICommand<bool>;

/// <summary>تعلیق پرستار</summary>
public record SuspendCaregiverCommand([property: JsonIgnore] long CaregiverId, string Reason) : ICommand<bool>;

/// <summary>بروزرسانی اطلاعات پایه پرستار</summary>
public record UpdateCaregiverProfileCommand(
    [property: JsonIgnore] long CaregiverId,
    string         FullName,
    CaregiverLevel Level) : ICommand<bool>;

/// <summary>افزودن مهارت تخصصی</summary>
public record AddCaregiverSkillCommand(
    [property: JsonIgnore] long CaregiverId,
    CaregiverSkillType SkillType,
    int                YearsOfExperience) : ICommand<bool>;

/// <summary>حذف مهارت</summary>
public record RemoveCaregiverSkillCommand(
    [property: JsonIgnore] long CaregiverId,
    CaregiverSkillType SkillType) : ICommand<bool>;
