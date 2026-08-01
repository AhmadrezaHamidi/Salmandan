using Care.Domain.Identity.Enums;

namespace Care.Domain.Identity.Args;

public sealed record CreateUserArg(long Id, string PhoneNumber, UserRole Role = UserRole.Family);
