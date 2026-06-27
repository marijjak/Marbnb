namespace Web.Models
{
    public enum UserRole
    {
        Guest,
        Host,
        Administrator
    }

    public enum Gender
    {
        Female,
        Male,
        Other
    }

    public enum AccommodationType
    {
        Apartment,
        Hotel,
        Hostel,
        Villa,
        Cottage,
        Studio,
        Room
    }

    public enum ReservationStatus
    {
        Created,
        Approved,
        Cancelled,
        Completed
    }

    public enum ReviewStatus
    {
        Created,
        Approved,
        Rejected
    }
}
