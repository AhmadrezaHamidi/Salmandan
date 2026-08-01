using Msg = Care.Domain.Resources.Message;

namespace Care.Domain.Caregiver.Exceptions;

public sealed class CaregiverNotFoundException          : BusinessException { public CaregiverNotFoundException()          : base(Msg.CaregiverNotFoundException) { } }
public sealed class CaregiverAlreadyVerifiedException   : BusinessException { public CaregiverAlreadyVerifiedException()   : base(Msg.CaregiverAlreadyVerifiedException) { } }
public sealed class CaregiverAlreadySuspendedException  : BusinessException { public CaregiverAlreadySuspendedException()  : base(Msg.CaregiverAlreadySuspendedException) { } }
public sealed class CaregiverNotVerifiedException       : BusinessException { public CaregiverNotVerifiedException()       : base(Msg.CaregiverNotVerifiedException) { } }
public sealed class CriminalRecordCheckRequiredException: BusinessException { public CriminalRecordCheckRequiredException(): base(Msg.CriminalRecordCheckRequiredException) { } }
public sealed class DuplicateNationalIdException        : BusinessException { public DuplicateNationalIdException()        : base(Msg.DuplicateNationalIdException) { } }
public sealed class DuplicateSkillException             : BusinessException { public DuplicateSkillException()             : base(Msg.DuplicateSkillException) { } }
public sealed class SkillNotFoundException              : BusinessException { public SkillNotFoundException()              : base(Msg.SkillNotFoundException) { } }
public sealed class InvalidCaregiverRatingException     : BusinessException { public InvalidCaregiverRatingException()     : base(Msg.InvalidRatingException) { } }
public sealed class CaregiverNotInAgencyException       : BusinessException { public CaregiverNotInAgencyException()       : base(Msg.CaregiverNotInAgencyException) { } }
