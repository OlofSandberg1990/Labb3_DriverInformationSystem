using System.ComponentModel.DataAnnotations;

public class EmployeeViewModel
{
    public int EmployeeId { get; set; }
    public string Name { get; set; }
    public string Phonenumber { get; set; }
    public DateTime DateOfHire { get; set; }

    [EmailAddress]
    public string Email { get; set; }

    [DataType(DataType.Password)]
    public string Password { get; set; }
}
