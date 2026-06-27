using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Web.Models
{
    // Entitet: Smeštajni objekat
    public class Accommodation : BaseEntity
    {
        public string Name { get; set; }              // Naziv
        public AccommodationType Type { get; set; }   // Tip (Hotel, Apartman, ...)
        public string Description { get; set; }       // Opis
        public string Address { get; set; }           // Adresa
        public string City { get; set; }              // Grad
        public decimal PricePerNight { get; set; }    // Cena po noći
        public int MaxGuests { get; set; }            // Maksimalan broj gostiju
        public string ImagePath { get; set; }         // putanja do uploadovane slike
        public DateTime DatePosted { get; set; }      // Datum postavljanja oglasa (dd/MM/yyyy)
        public bool IsAvailable { get; set; } = true; // Status da li je dostupan

        // Strani ključ: domaćin (vlasnik) objekta
        public string HostUsername { get; set; }

        // Lista recenzija (navigaciono, popunjava servisni sloj)
        [JsonIgnore]
        public List<Review> Reviews { get; set; } = new List<Review>();

        // Prosečna ocena (računa se iz odobrenih recenzija)
        [JsonIgnore]
        public double AverageRating { get; set; }
    }
}
