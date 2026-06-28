using System.ComponentModel.DataAnnotations;

namespace Web.Models.ViewModels
{
    public class ReviewFormViewModel
    {
        public int Id { get; set; }

        public int AccommodationId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [StringLength(80, ErrorMessage = "Title is too long.")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Please write a few words.")]
        public string Content { get; set; }

        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5.")]
        public int Rating { get; set; }
    }
}
