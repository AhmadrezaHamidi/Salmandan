using Msg = Care.Domain.Resources.Message;

namespace Care.Domain.Agency.Exceptions;

public sealed class AgencyNotFoundException         : BusinessException { public AgencyNotFoundException()         : base(Msg.AgencyNotFoundException) { } }
public sealed class AgencyAlreadyApprovedException  : BusinessException { public AgencyAlreadyApprovedException()  : base(Msg.AgencyAlreadyApprovedException) { } }
public sealed class AgencyAlreadySuspendedException : BusinessException { public AgencyAlreadySuspendedException() : base(Msg.AgencyAlreadySuspendedException) { } }
public sealed class AgencyNotActiveException        : BusinessException { public AgencyNotActiveException()        : base(Msg.AgencyNotActiveException) { } }
public sealed class AgencyLicenseExpiredException   : BusinessException { public AgencyLicenseExpiredException()   : base(Msg.AgencyLicenseExpiredException) { } }
public sealed class DuplicateLicenseNumberException : BusinessException { public DuplicateLicenseNumberException() : base(Msg.DuplicateLicenseNumberException) { } }
public sealed class InvalidCommissionException      : BusinessException { public InvalidCommissionException()      : base(Msg.InvalidCommissionException) { } }
public sealed class InvalidServicePriceException    : BusinessException { public InvalidServicePriceException()    : base(Msg.InvalidServicePriceException) { } }
public sealed class ServicePriceNotFoundException   : BusinessException { public ServicePriceNotFoundException()   : base(Msg.ServicePriceNotFoundException) { } }
public sealed class InvalidPayRateException         : BusinessException { public InvalidPayRateException()         : base(Msg.InvalidPayRateException) { } }
public sealed class PayRateExceedsPriceException    : BusinessException { public PayRateExceedsPriceException()    : base(Msg.PayRateExceedsPriceException) { } }
public sealed class InvalidRatingException          : BusinessException { public InvalidRatingException()          : base(Msg.InvalidRatingException) { } }
