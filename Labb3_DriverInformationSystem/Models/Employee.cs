using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace Labb3_DriverInformationSystem.Models
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string Phonenumber { get; set; }

        public DateTime DateOfHire { get; set; }

        [Required]
        public string IdentityUserId { get; set; }

        public IdentityUser IdentityUser { get; set; }

        //One-to-many relation to Driver
        public ICollection<Driver> Drivers { get; set; }
    }
}
