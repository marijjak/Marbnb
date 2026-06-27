using System;

namespace Web.Models
{
    // Entitet: Recenzija
    public class Review : BaseEntity
    {
        // Strani ključevi
        public int AccommodationId { get; set; }       // Smeštajni objekat
        public string ReviewerUsername { get; set; }   // Gost koji je napisao recenziju

        public string Title { get; set; }              // Naslov
        public string Content { get; set; }            // Sadržaj recenzije
        public int Rating { get; set; }                // Ocena (1–5)
        public string ImagePath { get; set; }          // Slika (opcioni parametar)
        public ReviewStatus Status { get; set; } = ReviewStatus.Created;

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
