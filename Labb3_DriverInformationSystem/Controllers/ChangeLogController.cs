using System.Threading.Tasks;
using Labb3_DriverInformationSystem.Data;
using Labb3_DriverInformationSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Labb3_DriverInformationSystem.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ChangeLogController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ChangeLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: ChangeLog
        public async Task<IActionResult> Index(string searchEntityName, string driverName, string employeeName, DateTime? fromDate, DateTime? toDate)
        {
            // Hämta alla loggar från databasen
            var logs = _context.ChangeLogs.AsQueryable();

            // Filtrera baserat på entitetsnamn (om det skickas in), och mappa sedan till inmatning på svenska. 
            if (!string.IsNullOrEmpty(searchEntityName))
            {
                switch (searchEntityName.ToLower())
                {
                    case "förare":
                        searchEntityName = "Driver";
                        break;
                    case "anställd":
                        searchEntityName = "Employee";
                        break;
                    case "händelse":
                        searchEntityName = "Event";
                        break;
                    default:
                        // Om det inte matchar någon känd entitet, returnera tomt resultat
                        return View(new List<ChangeLog>());
                }

                logs = logs.Where(log => log.EntityName.Contains(searchEntityName));
            }

            // Filtrera baserat på förare (driver) namn
            if (!string.IsNullOrEmpty(driverName))
            {
                var drivers = await _context.Drivers.Where(d => d.Name.Contains(driverName)).Select(d => d.DriverId).ToListAsync();
                logs = logs.Where(log => log.EntityName == "Driver" && drivers.Contains(log.EntityId));
            }

            // Filtrera baserat på anställdas (employee) namn
            if (!string.IsNullOrEmpty(employeeName))
            {
                var employees = await _context.Employees.Where(e => e.Name.Contains(employeeName)).Select(e => e.EmployeeId).ToListAsync();
                logs = logs.Where(log => log.EntityName == "Employee" && employees.Contains(log.EntityId));
            }

            // Filtrera baserat på datumintervall
            if (fromDate.HasValue)
            {
                logs = logs.Where(log => log.ChangeDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                logs = logs.Where(log => log.ChangeDate <= toDate.Value);
            }

            // Sortera resultatet efter datum
            logs = logs.OrderByDescending(log => log.ChangeDate);

            // Returnera filtrerade och sorterade loggar till vyn
            return View(await logs.ToListAsync());
        }



    }
}
