namespace Web.Models
{
    // Uloge korisnika (Gost, Domaćin, Administrator)
    public enum UserRole
    {
        Guest,
        Host,
        Administrator
    }

    // Pol korisnika
    public enum Gender
    {
        Female,
        Male,
        Other
    }

    // Tip smeštajnog objekta (Hotel, Apartman, Hostel, ...)
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

    // Status rezervacije.
    // Mapiranje na specifikaciju: Created=KREIRANA, Approved=ODOBRENA,
    // Cancelled=OTKAZANA, Completed=ZAVRŠENA
    public enum ReservationStatus
    {
        Created,
        Approved,
        Cancelled,
        Completed
    }

    // Status recenzije.
    // Created=Kreirana, Approved=Odobrena, Rejected=Odbijena
    public enum ReviewStatus
    {
        Created,
        Approved,
        Rejected
    }
}
