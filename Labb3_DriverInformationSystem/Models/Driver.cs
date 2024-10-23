using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations;

namespace Labb3_DriverInformationSystem.Models
{
    public class Driver
    {
        [Key]
        public int DriverId { get; set; }

        [Required]
        public string Name { get; set; }

        public string LicenseNumber { get; set; }

        public string PhoneNumber { get; set; }

        public int Salary { get; set; }

        //Summerar inkomst och utgifter för alla händelser som föraren har
        public int TotalIncome => Events?.Sum(e => e.Income ?? 0) ?? 0;
        
        public int TotalExpense => Events?.Sum(e => e.Expense ?? 0) ?? 0;

        public int EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public ICollection<Event> Events { get; set; }
    }
}
