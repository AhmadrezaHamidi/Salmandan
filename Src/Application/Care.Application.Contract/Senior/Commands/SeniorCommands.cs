using Care.Domain.Common.Enums;
using Care.Domain.Senior.Enums;

namespace Senior.Application.Commands;

/// <summary>ایجاد پرونده مراقبت سالمند — سازنده خودکار مدیر حلقه خانواده می‌شود</summary>
public record CreateSeniorCommand(
    string          FullName,
    string?         NationalId,
    DateOnly        BirthDate,
    Gender          Gender,
    DependencyLevel DependencyLevel,
    string          City,
    string?         District,
    string          Address,
    double?         Latitude,
    double?         Longitude,
    string?         TimeZoneId,
    string?         Notes,
    [property: JsonIgnore] long CreatedByUserId) : ICommand<long>;

/// <summary>بروزرسانی اطلاعات پایه و آدرس سالمند</summary>
public record UpdateSeniorCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    string  FullName,
    string  City,
    string? District,
    string  Address,
    double? Latitude,
    double? Longitude,
    string? Notes) : ICommand<bool>;

/// <summary>تغییر سطح وابستگی سالمند</summary>
public record ChangeDependencyLevelCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    DependencyLevel DependencyLevel) : ICommand<bool>;

// ── حلقه خانواده ────────────────────────────────────────────────────────────

/// <summary>افزودن عضو به حلقه خانواده با شماره موبایل — کاربر در صورت نبود ساخته می‌شود</summary>
public record AddFamilyMemberCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    string            PhoneNumber,
    FamilyRelation    Relation,
    FamilyAccessLevel AccessLevel,
    string?           TimeZoneId) : ICommand<bool>;

/// <summary>حذف عضو از حلقه خانواده</summary>
public record RemoveFamilyMemberCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    long MemberUserId) : ICommand<bool>;

/// <summary>تغییر سطح دسترسی عضو حلقه خانواده</summary>
public record ChangeFamilyMemberAccessCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    long              MemberUserId,
    FamilyAccessLevel AccessLevel) : ICommand<bool>;

// ── پرونده پزشکی ────────────────────────────────────────────────────────────

/// <summary>افزودن بیماری به پرونده</summary>
public record AddConditionCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    string    Title,
    string?   Description,
    DateOnly? DiagnosedAt,
    bool      IsChronic) : ICommand<bool>;

/// <summary>حذف بیماری از پرونده</summary>
public record RemoveConditionCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    long ConditionId) : ICommand<bool>;

/// <summary>افزودن دارو به پرونده</summary>
public record AddMedicationCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    string              Name,
    string              Dosage,
    MedicationFrequency Frequency,
    string?             Instruction,
    DateOnly            StartDate,
    DateOnly?           EndDate) : ICommand<bool>;

/// <summary>قطع مصرف دارو</summary>
public record DiscontinueMedicationCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    long MedicationId) : ICommand<bool>;

/// <summary>افزودن حساسیت دارویی یا غذایی</summary>
public record AddAllergyCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    string  Title,
    string? Reaction) : ICommand<bool>;

/// <summary>افزودن تماس اضطراری</summary>
public record AddEmergencyContactCommand(
    [property: JsonIgnore] long SeniorId,
    [property: JsonIgnore] long ActorUserId,
    string FullName,
    string PhoneNumber,
    string Relation,
    int    Priority) : ICommand<bool>;
