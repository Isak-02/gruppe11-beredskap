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

    [Required]
    [MaxLength(1000)]
    public string Beskrivelse { get; set; } = string.Empty;

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

    // Brukeren som registrerte den. Er null for det som ble registrert før innlogging fantes.
    public string? OpprettetAvId { get; set; }
    public Bruker? OpprettetAv { get; set; }
}
