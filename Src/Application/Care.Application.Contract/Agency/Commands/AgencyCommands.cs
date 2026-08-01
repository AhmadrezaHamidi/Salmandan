using Care.Domain.Agency.Enums;
using Care.Domain.Shift.Enums;

namespace Agency.Application.Commands;

/// <summary>ثبت مؤسسه پرستاری جدید — در انتظار تأیید ادمین</summary>
public record RegisterAgencyCommand(
    string   Name,
    string   LicenseNumber,
    DateOnly LicenseExpiryDate,
    [property: JsonIgnore] long OwnerUserId,
    string   City,
    string?  District,
    string?  Address,
    string   PhoneNumber) : ICommand<long>;

/// <summary>تأیید مؤسسه پس از بررسی مجوز وزارت بهداشت</summary>
public record ApproveAgencyCommand([property: JsonIgnore] long AgencyId) : ICommand<bool>;

/// <summary>رد درخواست مؤسسه</summary>
public record RejectAgencyCommand([property: JsonIgnore] long AgencyId, string Reason) : ICommand<bool>;

/// <summary>تعلیق مؤسسه</summary>
public record SuspendAgencyCommand([property: JsonIgnore] long AgencyId, string Reason) : ICommand<bool>;

/// <summary>تمدید مجوز مؤسسه</summary>
public record RenewAgencyLicenseCommand(
    [property: JsonIgnore] long AgencyId,
    string   LicenseNumber,
    DateOnly LicenseExpiryDate) : ICommand<bool>;

/// <summary>بروزرسانی اطلاعات تماس مؤسسه</summary>
public record UpdateAgencyProfileCommand(
    [property: JsonIgnore] long AgencyId,
    string  Name,
    string  City,
    string? District,
    string? Address,
    string  PhoneNumber) : ICommand<bool>;

/// <summary>تعیین درصد کمیسیون پلتفرم برای مؤسسه</summary>
public record SetAgencyCommissionCommand(
    [property: JsonIgnore] long AgencyId,
    decimal CommissionPercent) : ICommand<bool>;

/// <summary>ثبت یا بروزرسانی قیمت یک خدمت</summary>
public record SetAgencyServicePriceCommand(
    [property: JsonIgnore] long AgencyId,
    CareServiceType ServiceType,
    ShiftKind       ShiftKind,
    decimal         Price) : ICommand<bool>;
