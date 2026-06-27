namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public class Tag
{
    private Tag()
    {
        Name = string.Empty;
    }

    private Tag(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    public Guid Id { get; private set; }

    public string Name { get; private set; }

    public static Tag Create(Guid id, string name) => new(id, name);

    public void UpdateDetails(string name)
    {
        Name = name;
    }
}
