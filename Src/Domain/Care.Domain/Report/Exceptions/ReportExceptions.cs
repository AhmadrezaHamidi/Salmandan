using Msg = Care.Domain.Resources.Message;

namespace Care.Domain.Report.Exceptions;

public sealed class ReportNotFoundException              : BusinessException { public ReportNotFoundException()              : base(Msg.ReportNotFoundException) { } }
public sealed class ReportAlreadySubmittedException      : BusinessException { public ReportAlreadySubmittedException()      : base(Msg.ReportAlreadySubmittedException) { } }
public sealed class ReportAlreadyApprovedException       : BusinessException { public ReportAlreadyApprovedException()       : base(Msg.ReportAlreadyApprovedException) { } }
public sealed class InvalidPainLevelException            : BusinessException { public InvalidPainLevelException()            : base(Msg.InvalidPainLevelException) { } }
public sealed class InvalidVitalSignException            : BusinessException { public InvalidVitalSignException()            : base(Msg.InvalidVitalSignException) { } }
public sealed class DuplicateMedicationIntakeException   : BusinessException { public DuplicateMedicationIntakeException()   : base(Msg.DuplicateMedicationIntakeException) { } }
public sealed class IncidentDescriptionRequiredException : BusinessException { public IncidentDescriptionRequiredException() : base(Msg.IncidentDescriptionRequiredException) { } }
