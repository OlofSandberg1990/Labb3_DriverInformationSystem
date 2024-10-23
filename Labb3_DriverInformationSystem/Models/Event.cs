using System.ComponentModel.DataAnnotations;

namespace Labb3_DriverInformationSystem.Models
{
    public class Event
    {
        [Key]
        public int EventId { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [DataType(DataType.Date)]
        public DateTime EventDate { get; set; }       

        public int? Income { get; set; }

        public int? Expense { get; set; }

        public int DriverId { get; set; }
        public Driver Driver { get; set; }

        public ICollection<UserNotification> UserNotifications { get; set; }
    }
}
