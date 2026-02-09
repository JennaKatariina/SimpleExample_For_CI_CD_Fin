namespace SimpleExample.Domain.Entities;

public class User : BaseEntity
{
    // Private setterit - vain entiteetti voi päivittää arvoja
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }

    // Paramiteriton konstruktori EF Core:a varten
    private User()
    {
        FirstName = string.Empty;
        LastName = string.Empty;
        Email = string.Empty;
    }

    // Julkinen konstruktori uuden käyttäjän luomiseen
    public User(string firstName, string lastName, string email)
    {
        // Käytetään validoivia metodeja - ei koodin toistoa!
        UpdateBasicInfo(firstName, lastName);
        UpdateEmail(email);
    }

    /// <summary>
    /// Päivittää käyttäjän perustiedot (etu- ja sukunimi)
    /// </summary>
    public void UpdateBasicInfo(string firstName, string lastName)
    {
        ArgumentNullException.ThrowIfNull(firstName);
        ArgumentNullException.ThrowIfNull(lastName);

        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("Etunimi ei voi olla tyhjä.", nameof(firstName));

        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Sukunimi ei voi olla tyhjä.", nameof(lastName));

        if (firstName.Length < 3)
            throw new ArgumentException("Etunimen tulee olla vähintään 3 merkkiä pitkä.", nameof(firstName));

        if (lastName.Length < 3)
            throw new ArgumentException("Sukunimen tulee olla vähintään 3 merkkiä pitkä.", nameof(lastName));

        if (firstName.Length > 100)
            throw new ArgumentException("Etunimi voi olla enintään 100 merkkiä pitkä.", nameof(firstName));

        if (lastName.Length > 100)
            throw new ArgumentException("Sukunimi voi olla enintään 100 merkkiä pitkä.", nameof(lastName));

        FirstName = firstName;
        LastName = lastName;
    }

    /// <summary>
    /// Päivittää käyttäjän sähköpostiosoitteen
    /// </summary>
    public void UpdateEmail(string email)
    {
        ArgumentNullException.ThrowIfNull(email);

        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Sähköposti ei voi olla tyhjä.", nameof(email));

        if (!email.Contains('@'))
            throw new ArgumentException("Sähköpostin tulee olla kelvollinen.", nameof(email));

        if (email.Length > 255)
            throw new ArgumentException("Sähköposti voi olla enintään 255 merkkiä pitkä.", nameof(email));

        Email = email;
    }
}