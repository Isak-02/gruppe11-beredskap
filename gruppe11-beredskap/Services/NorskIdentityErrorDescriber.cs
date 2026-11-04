using Microsoft.AspNetCore.Identity;

namespace gruppe11_beredskap.Services;

// Identity sine feilmeldinger er på engelsk. Her oversettes de som brukeren kan få se
// når hen oppretter bruker eller bytter passord.
public class NorskIdentityErrorDescriber : IdentityErrorDescriber
{
    public override IdentityError PasswordTooShort(int length) =>
        new() { Code = nameof(PasswordTooShort), Description = $"Passordet må være minst {length} tegn." };

    public override IdentityError PasswordRequiresDigit() =>
        new() { Code = nameof(PasswordRequiresDigit), Description = "Passordet må inneholde minst ett tall (0-9)." };

    public override IdentityError PasswordRequiresLower() =>
        new() { Code = nameof(PasswordRequiresLower), Description = "Passordet må inneholde minst én liten bokstav (a-z)." };

    public override IdentityError PasswordRequiresUpper() =>
        new() { Code = nameof(PasswordRequiresUpper), Description = "Passordet må inneholde minst én stor bokstav (A-Z)." };

    public override IdentityError PasswordRequiresNonAlphanumeric() =>
        new() { Code = nameof(PasswordRequiresNonAlphanumeric), Description = "Passordet må inneholde minst ett spesialtegn, for eksempel ! eller ?." };

    public override IdentityError DuplicateUserName(string userName) =>
        new() { Code = nameof(DuplicateUserName), Description = $"Det finnes allerede en bruker med e-postadressen {userName}." };

    public override IdentityError DuplicateEmail(string email) =>
        new() { Code = nameof(DuplicateEmail), Description = $"Det finnes allerede en bruker med e-postadressen {email}." };

    public override IdentityError InvalidEmail(string? email) =>
        new() { Code = nameof(InvalidEmail), Description = "E-postadressen er ikke gyldig." };

    public override IdentityError InvalidToken() =>
        new() { Code = nameof(InvalidToken), Description = "Lenken for å tilbakestille passordet er ugyldig eller utløpt." };
}
