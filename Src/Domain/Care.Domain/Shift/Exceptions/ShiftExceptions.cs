using Msg = Care.Domain.Resources.Message;

namespace Care.Domain.Shift.Exceptions;

public sealed class ShiftNotFoundException            : BusinessException { public ShiftNotFoundException()            : base(Msg.ShiftNotFoundException) { } }
public sealed class ShiftAlreadyCancelledException    : BusinessException { public ShiftAlreadyCancelledException()    : base(Msg.ShiftAlreadyCancelledException) { } }
public sealed class ShiftAlreadyCompletedException    : BusinessException { public ShiftAlreadyCompletedException()    : base(Msg.ShiftAlreadyCompletedException) { } }
public sealed class ShiftNotAssignedException         : BusinessException { public ShiftNotAssignedException()         : base(Msg.ShiftNotAssignedException) { } }
public sealed class ShiftInProgressException          : BusinessException { public ShiftInProgressException()          : base(Msg.ShiftInProgressException) { } }
public sealed class InvalidShiftTransitionException   : BusinessException { public InvalidShiftTransitionException()   : base(Msg.InvalidShiftTransitionException) { } }
public sealed class InvalidShiftPriceException        : BusinessException { public InvalidShiftPriceException()        : base(Msg.InvalidShiftPriceException) { } }
public sealed class ShiftDateInPastException          : BusinessException { public ShiftDateInPastException()          : base(Msg.ShiftDateInPastException) { } }
public sealed class CheckInOutsideGeofenceException   : BusinessException { public CheckInOutsideGeofenceException()   : base(Msg.CheckInOutsideGeofenceException) { } }
public sealed class ShiftReportRequiredException      : BusinessException { public ShiftReportRequiredException()      : base(Msg.ShiftReportRequiredException) { } }
public sealed class CaregiverShiftOverlapException    : BusinessException { public CaregiverShiftOverlapException()    : base(Msg.CaregiverShiftOverlapException) { } }
public sealed class ShiftNotCompletedException        : BusinessException { public ShiftNotCompletedException()        : base(Msg.ShiftNotCompletedException) { } }
public sealed class ShiftAlreadyPaidOutException      : BusinessException { public ShiftAlreadyPaidOutException()      : base(Msg.ShiftAlreadyPaidOutException) { } }
