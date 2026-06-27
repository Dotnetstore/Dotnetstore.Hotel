namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public class Equipment
{
    private Equipment()
    {
        Name = string.Empty;
    }

    private Equipment(Guid id, string name, string? description)
    {
        Id = id;
        Name = name;
        Description = description;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public static Equipment Create(Guid id, string name, string? description) => new(id, name, description);

    public void UpdateDetails(string name, string? description)
    {
        Name = name;
        Description = description;
    }
}
