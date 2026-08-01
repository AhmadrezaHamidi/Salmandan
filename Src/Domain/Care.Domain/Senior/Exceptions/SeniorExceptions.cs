using Msg = Care.Domain.Resources.Message;

namespace Care.Domain.Senior.Exceptions;

public sealed class SeniorNotFoundException              : BusinessException { public SeniorNotFoundException()              : base(Msg.SeniorNotFoundException) { } }
public sealed class SeniorNotActiveException             : BusinessException { public SeniorNotActiveException()             : base(Msg.SeniorNotActiveException) { } }
public sealed class FamilyMemberAlreadyExistsException   : BusinessException { public FamilyMemberAlreadyExistsException()   : base(Msg.FamilyMemberAlreadyExistsException) { } }
public sealed class FamilyMemberNotFoundException        : BusinessException { public FamilyMemberNotFoundException()        : base(Msg.FamilyMemberNotFoundException) { } }
public sealed class CannotRemoveLastManagerException     : BusinessException { public CannotRemoveLastManagerException()     : base(Msg.CannotRemoveLastManagerException) { } }
public sealed class DuplicateConditionException          : BusinessException { public DuplicateConditionException()          : base(Msg.DuplicateConditionException) { } }
public sealed class ConditionNotFoundException           : BusinessException { public ConditionNotFoundException()           : base(Msg.ConditionNotFoundException) { } }
public sealed class DuplicateMedicationException         : BusinessException { public DuplicateMedicationException()         : base(Msg.DuplicateMedicationException) { } }
public sealed class MedicationNotFoundException          : BusinessException { public MedicationNotFoundException()          : base(Msg.MedicationNotFoundException) { } }
public sealed class DuplicateAllergyException            : BusinessException { public DuplicateAllergyException()            : base(Msg.DuplicateAllergyException) { } }
public sealed class DuplicateEmergencyContactException   : BusinessException { public DuplicateEmergencyContactException()   : base(Msg.DuplicateEmergencyContactException) { } }
