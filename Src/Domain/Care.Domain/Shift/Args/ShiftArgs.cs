using Care.Domain.Agency.Enums;
using Care.Domain.Shift.Enums;

namespace Care.Domain.Shift.Args;

public sealed record CreateShiftArg(
    long            Id,
    long            SeniorId,
    long            AgencyId,
    long            RequestedByUserId,
    CareServiceType ServiceType,
    ShiftKind       ShiftKind,
    DateOnly        Date,
    TimeOnly        StartTime,
    TimeOnly        EndTime,
    decimal         Price,
    string?         FamilyNote,
    long?           ReplacesShiftId = null);
