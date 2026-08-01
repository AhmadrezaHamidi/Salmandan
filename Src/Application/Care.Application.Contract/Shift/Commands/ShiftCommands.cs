using Care.Domain.Agency.Enums;
using Care.Domain.Shift.Enums;

namespace Shift.Application.Commands;

/// <summary>
/// ثبت درخواست شیفت مراقبت توسط خانواده.
/// قیمت از تعرفه مصوب مؤسسه خوانده می‌شود، نه از ورودی کاربر.
/// </summary>
public record RequestShiftCommand(
    long            SeniorId,
    long            AgencyId,
    CareServiceType ServiceType,
    ShiftKind       ShiftKind,
    DateOnly        Date,
    TimeOnly        StartTime,
    TimeOnly        EndTime,
    string?         FamilyNote,
    [property: JsonIgnore] long RequestedByUserId) : ICommand<long>;

/// <summary>تخصیص پرستار به شیفت توسط مؤسسه</summary>
public record AssignCaregiverCommand(
    [property: JsonIgnore] long ShiftId,
    long CaregiverId) : ICommand<bool>;

/// <summary>ثبت ورود پرستار در محل — کنترل ژئوفنس</summary>
public record CheckInShiftCommand(
    [property: JsonIgnore] long ShiftId,
    [property: JsonIgnore] long ActorUserId,
    double Latitude,
    double Longitude) : ICommand<bool>;

/// <summary>ثبت خروج پرستار</summary>
public record CheckOutShiftCommand(
    [property: JsonIgnore] long ShiftId,
    [property: JsonIgnore] long ActorUserId) : ICommand<bool>;

/// <summary>تکمیل شیفت — وجه از امانت آزاد می‌شود</summary>
public record CompleteShiftCommand([property: JsonIgnore] long ShiftId) : ICommand<bool>;

/// <summary>لغو شیفت</summary>
public record CancelShiftCommand(
    [property: JsonIgnore] long ShiftId,
    CancelActor CancelledBy,
    string?     Reason) : ICommand<bool>;

/// <summary>ثبت عدم حضور پرستار — تریگر تضمین پوشش شیفت</summary>
public record MarkShiftNoShowCommand([property: JsonIgnore] long ShiftId) : ICommand<bool>;

/// <summary>
/// ایجاد شیفت جایگزین برای شیفتی که پرستارش نیامده.
/// قلب وعده «تضمین پوشش شیفت» — تنها تمایزی که مستقیم به بزرگ‌ترین درد بازار می‌خورد.
/// </summary>
public record ReplaceShiftCommand(
    [property: JsonIgnore] long ShiftId,
    long ReplacementCaregiverId) : ICommand<long>;
