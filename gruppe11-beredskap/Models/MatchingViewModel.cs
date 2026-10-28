using System.ComponentModel.DataAnnotations;

namespace gruppe11_beredskap.Models;

// Data for matching-siden: åpne behov, ledige ressurser og eksisterende koblinger.
public class MatchingViewModel
{
    [Required(ErrorMessage = "Du må velge et behov")]
    [Display(Name = "Behov")]
    public int? BehovId { get; set; }

    [Required(ErrorMessage = "Du må velge en ressurs")]
    [Display(Name = "Ressurs")]
    public int? RessursId { get; set; }

    public List<BehovViewModel> ÅpneBehov { get; set; } = new();
    public List<RessursViewModel> LedigeRessurser { get; set; } = new();
    public List<MatchingVisning> Matchinger { get; set; } = new();
}

// En ferdig kobling, formatert for visning i tabellen.
public class MatchingVisning
{
    public int Id { get; set; }
    public string Behov { get; set; } = string.Empty;
    public string Ressurs { get; set; } = string.Empty;
    public DateTime OpprettetTidspunkt { get; set; }
}
