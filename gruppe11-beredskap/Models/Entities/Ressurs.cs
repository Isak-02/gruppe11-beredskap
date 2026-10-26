using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models.Entities;

// Databasetabellen for ressurser. Feltene tilsvarer RessursViewModel,
// men uten visningsattributter (de hører til view-modellen).
public class Ressurs
{
    public int Id { get; set; }

    [Required]
    [MaxLength(200)]
    public string TypeRessurs { get; set; } = string.Empty;

    [MaxLength(200)]
    public string? AnnenType { get; set; }

    [MaxLength(50)]
    public string? Drivstofftype { get; set; }

    public decimal? Lastekapasitet { get; set; }

    [MaxLength(200)]
    public string? GeografiskOmrade { get; set; }

    public DateTime Tidspunkt { get; set; }

    [Required]
    [MaxLength(500)]
    public string Kontaktpunkt { get; set; } = string.Empty;

    [MaxLength(50)]
    public string Status { get; set; } = "Tilgjengelig";

    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public DateTime OpprettetTidspunkt { get; set; } = DateTime.UtcNow;
}
