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

    public async Task<Ressurs?> GetByIdAsync(int id)
    {
        return await _db.Ressurser.FindAsync(id);
    }

    public async Task UpdateAsync(Ressurs ressurs)
    {
        _db.Ressurser.Update(ressurs);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var ressurs = await _db.Ressurser.FindAsync(id);
        if (ressurs == null)
            return;

        _db.Ressurser.Remove(ressurs);
        await _db.SaveChangesAsync();
    }

    // Endrer bare status på én ressurs
    public async Task UpdateStatusAsync(int id, string status)
    {
        var ressurs = await _db.Ressurser.FindAsync(id);
        if (ressurs == null)
            return;

        ressurs.Status = status;
        await _db.SaveChangesAsync();
    }
}
