using Care.Domain.Identity.Aggregates;
using Care.Domain.Identity.Entities;

namespace Care.Domain.Identity.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<User?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default);
    Task        AddAsync(User user, CancellationToken ct = default);
    Task        UpdateAsync(User user, CancellationToken ct = default);
    Task<long>  GetNextIdAsync();
}

public interface IOtpRepository
{
    Task<OtpRequest?> GetLatestByPhoneAsync(string phoneNumber, CancellationToken ct = default);
    Task              AddAsync(OtpRequest otp, CancellationToken ct = default);
    Task              UpdateAsync(OtpRequest otp, CancellationToken ct = default);
    Task<long>        GetNextIdAsync();
}

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default);
    Task                AddAsync(RefreshToken token, CancellationToken ct = default);
    Task                UpdateAsync(RefreshToken token, CancellationToken ct = default);
    Task                RevokeAllByUserAsync(long userId, CancellationToken ct = default);
    Task<long>          GetNextIdAsync();
}
