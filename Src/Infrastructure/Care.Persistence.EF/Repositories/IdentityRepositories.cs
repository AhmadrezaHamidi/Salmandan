using AhmadBase.Persistence.NHiLoHelper;
using Care.Domain.Identity.Aggregates;
using Care.Domain.Identity.Entities;
using Care.Domain.Identity.Repositories;

namespace Care.Persistence.EF.Repositories;

public sealed class UserRepository(CareDbContext db, IHiLoIdGenerator hiLo) : IUserRepository
{
    public Task<User?> GetByIdAsync(long id, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByPhoneAsync(string phoneNumber, CancellationToken ct = default)
        => db.Users.FirstOrDefaultAsync(u => u.PhoneNumber == phoneNumber, ct);

    public async Task AddAsync(User user, CancellationToken ct = default)
        => await db.Users.AddAsync(user, ct);

    public Task UpdateAsync(User user, CancellationToken ct = default)
    {
        db.Users.Update(user);
        return Task.CompletedTask;
    }

    public Task<long> GetNextIdAsync() => Task.FromResult(hiLo.GetNextId<User>());
}

public sealed class OtpRepository(CareDbContext db) : IOtpRepository
{
    public Task<OtpRequest?> GetLatestByPhoneAsync(string phoneNumber, CancellationToken ct = default)
        => db.OtpRequests
            .Where(o => o.PhoneNumber == phoneNumber && !o.IsUsed && o.ExpiresAt > DateTime.UtcNow)
            .OrderByDescending(o => o.Id)
            .FirstOrDefaultAsync(ct);

    public async Task AddAsync(OtpRequest otp, CancellationToken ct = default)
        => await db.OtpRequests.AddAsync(otp, ct);

    public Task UpdateAsync(OtpRequest otp, CancellationToken ct = default)
    {
        db.OtpRequests.Update(otp);
        return Task.CompletedTask;
    }

    public async Task<long> GetNextIdAsync()
    {
        var max = await db.OtpRequests.MaxAsync(o => (long?)o.Id) ?? 0;
        return max + 1;
    }
}

public sealed class RefreshTokenRepository(CareDbContext db) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken ct = default)
        => db.RefreshTokens.FirstOrDefaultAsync(r => r.Token == token, ct);

    public async Task AddAsync(RefreshToken token, CancellationToken ct = default)
        => await db.RefreshTokens.AddAsync(token, ct);

    public Task UpdateAsync(RefreshToken token, CancellationToken ct = default)
    {
        db.RefreshTokens.Update(token);
        return Task.CompletedTask;
    }

    public async Task RevokeAllByUserAsync(long userId, CancellationToken ct = default)
    {
        var tokens = await db.RefreshTokens.Where(r => r.UserId == userId && !r.IsRevoked).ToListAsync(ct);
        foreach (var t in tokens) t.Revoke();
    }

    public async Task<long> GetNextIdAsync()
    {
        var max = await db.RefreshTokens.MaxAsync(r => (long?)r.Id) ?? 0;
        return max + 1;
    }
}
