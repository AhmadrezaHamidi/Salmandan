using Care.Domain.Agency.Aggregates;

namespace Care.Domain.Agency.Repositories;

public interface IAgencyRepository
{
    Task<CareAgency?>      GetByIdAsync(long id, CancellationToken ct = default);
    Task<CareAgency?>      GetByOwnerUserIdAsync(long ownerUserId, CancellationToken ct = default);
    Task<bool>             LicenseNumberExistsAsync(string licenseNumber, CancellationToken ct = default);
    Task<List<CareAgency>> GetActiveByCityAsync(string city, CancellationToken ct = default);
    Task                   AddAsync(CareAgency agency, CancellationToken ct = default);
    Task                   UpdateAsync(CareAgency agency, CancellationToken ct = default);
    Task<long>             GetNextIdAsync();
    Task<long>             GetNextPriceIdAsync();
}
