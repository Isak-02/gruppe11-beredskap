using gruppe11_beredskap.Models.Entities;

namespace gruppe11_beredskap.DataAccess.Repositories;

// Kontrakten for matching av behov og ressurser.
public interface IMatchingRepository
{
    Task AddAsync(Matching matching);
    Task<List<Matching>> GetAllAsync();
    Task<HashSet<int>> GetMatchedeBehovIdsAsync();
    Task<HashSet<int>> GetMatchedeRessursIdsAsync();
}
