namespace gruppe11_beredskap.Models.Entities;

// matching av behov og resurser - admin/kriseleder
public class Matching
{
    public int Id { get; set; }

    public int BehovId { get; set; }
    public Behov? Behov { get; set; }

    public int RessursId { get; set; }
    public Ressurs? Ressurs { get; set; }

    public DateTime OpprettetTidspunkt { get; set; } = DateTime.UtcNow;

    // Tom så lenge koblingen er aktiv. Settes når behovet er løst, så historikken bevares.
    public DateTime? LostTidspunkt { get; set; }
}
