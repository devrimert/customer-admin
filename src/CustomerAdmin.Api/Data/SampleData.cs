using CustomerAdmin.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace CustomerAdmin.Api.Data;

/// <summary>
/// Demo data, so the app doesn't start empty. Only runs when "SeedSampleData" is true
/// and there are no customers yet, so it never touches existing data.
/// Contact details are made up (example domains, fake numbers).
/// </summary>
public static class SampleData
{
    public static async Task SeedAsync(CustomerAdminDbContext db, CancellationToken cancellationToken = default)
    {
        if (await db.Customers.AnyAsync(cancellationToken))
        {
            return;
        }

        db.Customers.AddRange(
            new Customer
            {
                Name = "Scuderia Ferrari HP",
                Email = "info@ferrari.example",
                Phone = "+39 0536 555 0100",
                Street = "Via Abetone Inferiore 4",
                PostalCode = "41053",
                City = "Maranello",
                Note = "Rot ist keine Farbe, sondern eine Einstellung.",
                Contacts =
                [
                    NewContact("Frédéric", "Vasseur", "Teamchef", "f.vasseur@ferrari.example", "+39 0536 555 0101", isPrimary: true),
                    NewContact("Charles", "Leclerc", "Fahrer", "c.leclerc@ferrari.example", "+39 0536 555 0116"),
                    NewContact("Michael", "Schumacher", "Ehemaliger Fahrer", "m.schumacher@ferrari.example"),
                    NewContact("Sebastian", "Vettel", "Ehemaliger Fahrer", "s.vettel@ferrari.example"),
                    NewContact("Giuseppe", "Rossi", "Mechaniker", "g.rossi@ferrari.example", "+39 0536 555 0142",
                        note: "Wechselt einen Reifen in unter zwei Sekunden."),
                ],
            },
            new Customer
            {
                Name = "Galatasaray SK",
                Email = "info@galatasaray.example",
                Phone = "+90 212 555 1905",
                Street = "RAMS Park, Seyrantepe",
                PostalCode = "34415",
                City = "İstanbul",
                Note = "Gegründet 1905.",
                Contacts =
                [
                    NewContact("Dursun", "Özbek", "Präsident", "d.ozbek@galatasaray.example", "+90 212 555 1901", isPrimary: true),
                    NewContact("Okan", "Buruk", "Cheftrainer", "o.buruk@galatasaray.example", "+90 212 555 1902"),
                    NewContact("Victor", "Osimhen", "Stürmer", "v.osimhen@galatasaray.example"),
                    NewContact("Abdülkerim", "Bardakcı", "Verteidiger", "a.bardakci@galatasaray.example"),
                ],
            },
            new Customer
            {
                Name = "FC Bayern München",
                Email = "info@fcbayern.example",
                Phone = "+49 89 555 1900",
                Street = "Säbener Straße 51",
                PostalCode = "81547",
                City = "München",
                Contacts =
                [
                    NewContact("Vincent", "Kompany", "Cheftrainer", "v.kompany@fcbayern.example", "+49 89 555 1901", isPrimary: true),
                    NewContact("Harry", "Kane", "Stürmer", "h.kane@fcbayern.example"),
                    NewContact("Jamal", "Musiala", "Mittelfeldspieler", "j.musiala@fcbayern.example"),
                    NewContact("Joshua", "Kimmich", "Mittelfeldspieler", "j.kimmich@fcbayern.example"),
                ],
            },
            new Customer
            {
                Name = "New York Knicks",
                Email = "info@knicks.example",
                Phone = "+1 212 555 0146",
                Street = "4 Pennsylvania Plaza",
                PostalCode = "NY 10001",
                City = "New York",
                Contacts =
                [
                    NewContact("Jalen", "Brunson", "Point Guard", "j.brunson@knicks.example", "+1 212 555 0111", isPrimary: true),
                    NewContact("Karl-Anthony", "Towns", "Center", "k.towns@knicks.example"),
                    NewContact("OG", "Anunoby", "Forward", "o.anunoby@knicks.example"),
                    NewContact("Patrick", "Ewing", "Markenbotschafter", "p.ewing@knicks.example"),
                ],
            },
            new Customer
            {
                Name = "Hellsburg United",
                Email = "hallo@hellsburg.example",
                Phone = "+90 236 555 4530",
                Street = "Atatürk Caddesi No. 42",
                PostalCode = "45300",
                City = "Salihli / Manisa",
                Note = "Easter Egg: Ein Freundeskreis aus Salihli.",
                IsActive = false,
                Contacts =
                [
                    NewContact("Devrim Mert", "Yöyen", "Softwareentwickler", "devrim@hellsburg.example", "+90 236 555 4531", isPrimary: true),
                    NewContact("Salih Alper", "Aşkın", "Softwareentwickler", "salih@hellsburg.example"),
                    NewContact("Berke", "Yelaldı", "Elektro- und Elektronikingenieur", "berke@hellsburg.example"),
                    NewContact("Umut Barkın", "Köz", "Bauingenieur", "umut@hellsburg.example"),
                    NewContact("Mert", "Ertürk", "Künstler", "mert@hellsburg.example"),
                ],
            },
            // No contacts on purpose, shows the empty state
            new Customer
            {
                Name = "SSV Ulm 1846 Fußball",
                Email = "info@ssvulm.example",
                Street = "Stadionstraße 17",
                PostalCode = "89073",
                City = "Ulm",
            });

        await db.SaveChangesAsync(cancellationToken);
    }

    private static Contact NewContact(
        string firstName,
        string lastName,
        string position,
        string email,
        string? phone = null,
        bool isPrimary = false,
        string? note = null) => new()
    {
        FirstName = firstName,
        LastName = lastName,
        Position = position,
        Email = email,
        Phone = phone,
        IsPrimary = isPrimary,
        Note = note,
    };
}
