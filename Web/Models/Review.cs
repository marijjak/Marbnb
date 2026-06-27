using System;

namespace Web.Models
{
    public class Review : BaseEntity
    {
        public int AccommodationId { get; set; }
        public string ReviewerUsername { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public int Rating { get; set; }
        public string ImagePath { get; set; }
        public ReviewStatus Status { get; set; } = ReviewStatus.Created;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
