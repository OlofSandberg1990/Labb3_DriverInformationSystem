using System;
using System.ComponentModel.DataAnnotations;

namespace Labb3_DriverInformationSystem.Models
{
    public class ChangeLog
    {
        [Key]
        public int ChangeLogId { get; set; }

        public string EntityName { get; set; } //T.ex. "Driver", "Employee"
        public string AffectedName { get; set; } //Namnet på den som påverkas (t.ex. Petter Olsson)

        public int EntityId { get; set; }

        public string ChangeType { get; set; } //T.ex. "Create", "Update", "Delete"
        public string PropertyChanged { get; set; } //T.ex. "Salary", "PhoneNumber"

        public string OldValue { get; set; }
        public string NewValue { get; set; }

        public string ChangeDescription { get; set; } // Beskrivning av ändringen (t.ex. "Lön ändrades från 20000 till 25000")
        public string ChangedBy { get; set; } // Namn på användaren som gjorde ändringen

        public DateTime ChangeDate { get; set; } = DateTime.Now; // Datum för ändringen
    }

}
