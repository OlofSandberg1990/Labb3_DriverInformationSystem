using System.ComponentModel.DataAnnotations;

namespace Labb3_DriverInformationSystem.Models
{
    public class UserNotification
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } 

        [Required]
        public int EventId { get; set; }

        public bool IsRead { get; set; } = false; //Standardvärde är att notifikationen är oläst

        public Event Event { get; set; }
    }
}
