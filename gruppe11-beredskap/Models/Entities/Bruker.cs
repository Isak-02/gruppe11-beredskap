using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace gruppe11_beredskap.Models.Entities;

// Brukeren i systemet. Arver fra IdentityUser, som allerede har e-post, telefonnummer,
// passord (lagret som hash) og innloggingsinformasjon. Hva brukeren får gjøre, styres av
// rollen (se RoleNames): KriseInnmelder, RessursInnmelder eller Koordinator.
public class Bruker : IdentityUser
{
    [MaxLength(200)]
    public string Navn { get; set; } = string.Empty;
}
