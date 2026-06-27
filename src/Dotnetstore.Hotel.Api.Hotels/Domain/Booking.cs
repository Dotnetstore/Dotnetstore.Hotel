namespace Dotnetstore.Hotel.Api.Hotels.Domain;

public class Booking
{
    private readonly List<Room> _rooms = [];

    private Booking()
    {
        Status = BookingStatuses.Reserved;
    }

    private Booking(Guid id, Customer customer, DateOnly checkInDate, DateOnly checkOutDate, IEnumerable<Room> rooms)
    {
        Id = id;
        CustomerId = customer.Id;
        Customer = customer;
        CheckInDate = checkInDate;
        CheckOutDate = checkOutDate;
        Status = BookingStatuses.Reserved;
        _rooms.AddRange(rooms);
    }

    public Guid Id { get; private set; }

    public Guid CustomerId { get; private set; }

    public Customer Customer { get; private set; } = null!;

    public DateOnly CheckInDate { get; private set; }

    public DateOnly CheckOutDate { get; private set; }

    public string Status { get; private set; }

    public IReadOnlyCollection<Room> Rooms => _rooms;

    public static Booking Create(Guid id, Customer customer, DateOnly checkInDate, DateOnly checkOutDate, IEnumerable<Room> rooms)
        => new(id, customer, checkInDate, checkOutDate, rooms);

    public bool Cancel()
    {
        if (Status is not (BookingStatuses.Reserved or BookingStatuses.CheckedIn))
        {
            return false;
        }

        Status = BookingStatuses.Cancelled;
        return true;
    }

    public bool CheckIn()
    {
        if (Status != BookingStatuses.Reserved)
        {
            return false;
        }

        Status = BookingStatuses.CheckedIn;
        return true;
    }

    public bool CheckOut()
    {
        if (Status != BookingStatuses.CheckedIn)
        {
            return false;
        }

        Status = BookingStatuses.CheckedOut;
        return true;
    }
}
