using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models.Entities;

// Databasetabellen for behov. Feltene tilsvarer BehovViewModel,
// men uten visningsattributter (de hører til view-modellen).
public class Behov
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string TypeBehov { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? GeografiskOmrade { get; set; }

    public DateTime Tidspunkt { get; set; }

    [Required]
    [MaxLength(50)]
    public string Prioritet { get; set; } = string.Empty;

    [Required]
    [MaxLength(500)]
    public string Kontaktpunkt { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = "Ny";

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public DateTime OpprettetTidspunkt { get; set; } = DateTime.UtcNow;
}
