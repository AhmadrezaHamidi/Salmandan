using Care.Domain.Agency.Enums;

namespace Agency.Application.Queries;

public record GetAgencyQuery(long Id) : IQuery<AgencyResult?>;

public record GetAgenciesQuery(
    int           Page = 1,
    int           PageSize = 20,
    string?       City = null,
    AgencyStatus? Status = null) : IQuery<PagedResult<AgencyListItem>>;

/// <summary>مؤسسات فعالی که می‌توانند شیفت بپذیرند — لیست انتخاب خانواده</summary>
public record GetAvailableAgenciesQuery(string City, CareServiceType ServiceType) : IQuery<IReadOnlyList<AgencyListItem>>;

public record GetAgencyPricesQuery(long AgencyId) : IQuery<IReadOnlyList<AgencyPriceItem>>;

public sealed record AgencyResult(
    long     Id,
    string   Name,
    string   LicenseNumber,
    DateOnly LicenseExpiryDate,
    bool     HasValidLicense,
    long     OwnerUserId,
    string   City,
    string?  District,
    string?  Address,
    string   PhoneNumber,
    string   Status,
    decimal  CommissionPercent,
    decimal  RatingAvg,
    int      RatingCount,
    int      CompletedShifts,
    DateTime CreatedAt);

public sealed record AgencyListItem(
    long     Id,
    string   Name,
    string   City,
    string?  District,
    string   Status,
    decimal  RatingAvg,
    int      CompletedShifts,
    DateOnly LicenseExpiryDate);

public sealed record AgencyPriceItem(
    long    Id,
    string  ServiceType,
    string  ShiftKind,
    decimal Price);
