namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public class Customer
{
    private Customer()
    {
        FullName = string.Empty;
        IdentificationType = IdentificationTypes.Passport;
        IdentificationNumber = string.Empty;
        Address = new Address(string.Empty, string.Empty, string.Empty, string.Empty);
        PhoneNumber = string.Empty;
        Email = string.Empty;
        Nationality = string.Empty;
    }

    private Customer(
        Guid id,
        string fullName,
        string identificationType,
        string identificationNumber,
        Address address,
        string phoneNumber,
        string email,
        DateOnly dateOfBirth,
        string nationality,
        string? notes)
    {
        Id = id;
        FullName = fullName;
        IdentificationType = identificationType;
        IdentificationNumber = identificationNumber;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        DateOfBirth = dateOfBirth;
        Nationality = nationality;
        Notes = notes;
    }

    public Guid Id { get; private set; }

    public int CustomerNumber { get; private set; }

    public string FullName { get; private set; }

    public string IdentificationType { get; private set; }

    public string IdentificationNumber { get; private set; }

    public Address Address { get; private set; }

    public string PhoneNumber { get; private set; }

    public string Email { get; private set; }

    public DateOnly DateOfBirth { get; private set; }

    public string Nationality { get; private set; }

    public string? Notes { get; private set; }

    public static Customer Create(
        Guid id,
        string fullName,
        string identificationType,
        string identificationNumber,
        Address address,
        string phoneNumber,
        string email,
        DateOnly dateOfBirth,
        string nationality,
        string? notes)
        => new(id, fullName, identificationType, identificationNumber, address, phoneNumber, email, dateOfBirth, nationality, notes);

    public void UpdateDetails(
        string fullName,
        string identificationType,
        string identificationNumber,
        Address address,
        string phoneNumber,
        string email,
        DateOnly dateOfBirth,
        string nationality,
        string? notes)
    {
        FullName = fullName;
        IdentificationType = identificationType;
        IdentificationNumber = identificationNumber;
        Address = address;
        PhoneNumber = phoneNumber;
        Email = email;
        DateOfBirth = dateOfBirth;
        Nationality = nationality;
        Notes = notes;
    }
}
