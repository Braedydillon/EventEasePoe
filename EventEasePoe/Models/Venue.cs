using System.ComponentModel.DataAnnotations;

namespace EventEasePoe.Models
{

    public class Venue
    {
        [Key]
        public int VenueID { get; set; }

        [Required, StringLength(30)]
        public string? VenueName { get; set; }

        [Required, StringLength(40)]
        public string? Location { get; set; }

        // Increased length to handle long URLs
        [StringLength(500)]
        public string? ImageURL { get; set; }

        public int VenueCapcity { get; set; }

        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}

