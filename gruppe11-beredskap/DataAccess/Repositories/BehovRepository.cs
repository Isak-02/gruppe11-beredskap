using gruppe11_beredskap.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace gruppe11_beredskap.DataAccess.Repositories;

// Gjør selve databasearbeidet for behov via EF Core.
public class BehovRepository : IBehovRepository
{
    private readonly ApplicationDbContext _db;

    public BehovRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Behov behov)
    {
        _db.Behov.Add(behov);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Behov>> GetAllAsync()
    {
        return await _db.Behov
            .AsNoTracking()
            .OrderBy(b => b.Id)
            .ToListAsync();
    }

    // Endrer bare status på ett behov
    public async Task UpdateStatusAsync(int id, string status)
    {
        var behov = await _db.Behov.FindAsync(id);
        if (behov == null)
            return;

        behov.Status = status;
        await _db.SaveChangesAsync();
    }
}
