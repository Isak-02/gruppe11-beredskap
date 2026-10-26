using gruppe11_beredskap.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace gruppe11_beredskap.DataAccess.Repositories;

// Gjør selve databasearbeidet for ressurser via EF Core.
public class RessursRepository : IRessursRepository
{
    private readonly ApplicationDbContext _db;

    public RessursRepository(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Ressurs ressurs)
    {
        _db.Ressurser.Add(ressurs);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Ressurs>> GetAllAsync()
    {
        return await _db.Ressurser
            .AsNoTracking()
            .OrderBy(r => r.Id)
            .ToListAsync();
    }
}
