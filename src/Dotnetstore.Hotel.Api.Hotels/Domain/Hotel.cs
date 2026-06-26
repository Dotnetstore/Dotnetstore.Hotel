namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public class Hotel
{
    private readonly List<string> _amenities = [];

    private Hotel()
    {
        // EF Core
        Name = string.Empty;
        Address = null!;
        ContactInfo = null!;
    }

    private Hotel(Guid id, string name, Address address, ContactInfo contactInfo)
    {
        Id = id;
        Name = name;
        Address = address;
        ContactInfo = contactInfo;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public Address Address { get; private set; }

    public ContactInfo ContactInfo { get; private set; }

    public IReadOnlyCollection<string> Amenities => _amenities;

    public static Hotel Create(Guid id, string name, Address address, ContactInfo contactInfo)
        => new(id, name, address, contactInfo);

    public void UpdateProfile(string name, Address address, ContactInfo contactInfo)
    {
        Name = name;
        Address = address;
        ContactInfo = contactInfo;
    }

    public void SetAmenities(IEnumerable<string> amenities)
    {
        _amenities.Clear();
        _amenities.AddRange(amenities);
    }
}
