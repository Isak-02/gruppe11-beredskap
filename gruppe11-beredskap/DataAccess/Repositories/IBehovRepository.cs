using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.DataAccess.Repositories;

// Kontrakten controlleren bruker. Controlleren vet ikke at det er MariaDB bak.
public interface IBehovRepository
{
    Task AddAsync(Behov behov);
    Task<List<Behov>> GetAllAsync();
}
