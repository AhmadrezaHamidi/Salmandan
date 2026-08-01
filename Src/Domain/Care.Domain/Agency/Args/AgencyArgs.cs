namespace Care.Domain.Agency.Args;

public sealed record CreateAgencyArg(
    long     Id,
    string   Name,
    string   LicenseNumber,
    DateOnly LicenseExpiryDate,
    long     OwnerUserId,
    string   City,
    string?  District,
    string?  Address,
    string   PhoneNumber,
    decimal  CommissionPercent);
