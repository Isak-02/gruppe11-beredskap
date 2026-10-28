using gruppe11_beredskap.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace gruppe11_beredskap.DataAccess.Repositories;

// Databasearbeidet for matching av behov og ressurser via EF Core.
public class MatchingRepository : IMatchingRepository
{
    private readonly ApplicationDbContext _db;

    public MatchingRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Matching matching)
    {
        _db.Matchinger.Add(matching);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Matching>> GetAllAsync()
    {
        return await _db.Matchinger
            .AsNoTracking()
            .Include(m => m.Behov)
            .Include(m => m.Ressurs)
            .OrderBy(m => m.Id)
            .ToListAsync();
    }

    public async Task<HashSet<int>> GetMatchedeBehovIdsAsync()
    {
        var ids = await _db.Matchinger.Select(m => m.BehovId).ToListAsync();
        return ids.ToHashSet();
    }

    public async Task<HashSet<int>> GetMatchedeRessursIdsAsync()
    {
        var ids = await _db.Matchinger.Select(m => m.RessursId).ToListAsync();
        return ids.ToHashSet();
    }
}
