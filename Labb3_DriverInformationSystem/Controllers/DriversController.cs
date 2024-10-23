using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Labb3_DriverInformationSystem.Data;
using Labb3_DriverInformationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Labb3_DriverInformationSystem.Services;
using Microsoft.AspNetCore.Authorization;

namespace Labb3_DriverInformationSystem.Controllers
{
    [Authorize(Roles = "Admin, Employee")]
    public class DriversController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ChangeLogService _changeLogService;

        public DriversController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ChangeLogService changeLogService)
        {
            _context = context;
            _userManager = userManager;
            _changeLogService = changeLogService;
        }

        //Get: Drivers
        public async Task<IActionResult> Index(string searchString, DateTime? fromDate, DateTime? toDate, bool showAll = false)
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            //Tilldela isAdmin till ViewBag för att kunna använda i vyn
            ViewBag.IsAdmin = isAdmin;

            //Hämta alla förare inklusive anställd och händelser
            IQueryable<Driver> drivers = _context.Drivers
                .Include(d => d.Employee)
                .Include(d => d.Events)
                .AsQueryable();


            //Tilldela showAll till ViewBag för att kunna använda i vyn
            ViewBag.ShowAll = showAll;

            if (!isAdmin && !showAll)
            {
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.IdentityUserId == user.Id);
                if (employee == null)
                {
                    return Content("Medarbetare ej funnen.");
                }

                // Filtrera förare endast för inloggad medarbetare
                drivers = drivers.Where(d => d.EmployeeId == employee.EmployeeId);
            }

            //Filtrera förare baserat på söksträng
            if (!string.IsNullOrEmpty(searchString))
            {
                drivers = drivers.Where(d => d.Name.Contains(searchString));
            }

            //Filtrera förare baserat på datumintervall
            if (fromDate.HasValue && toDate.HasValue)
            {
                var startDate = fromDate.Value.Date; // Sätter tiden till 00:00:00
                var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1); // Sätter tiden till 23:59:59

                drivers = drivers.Select(d => new Driver
                {
                    DriverId = d.DriverId,
                    Name = d.Name,
                    LicenseNumber = d.LicenseNumber,
                    Employee = d.Employee,
                    Events = d.Events.Where(e => e.EventDate >= startDate && e.EventDate <= endDate).ToList()
                });
            }

            //Tilldela söksträng till ViewBag för att kunna använda i vyn
            ViewBag.CurrentFilter = searchString;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            return View(await drivers.ToListAsync());
        }



        // GET: Drivers/Details/5
        public async Task<IActionResult> Details(int? id, string searchQuery, DateTime? fromDate, DateTime? toDate)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Hämta föraren inklusive anställd och händelser
            var driver = await _context.Drivers
                .Include(d => d.Employee)
                .Include(d => d.Events)
                .FirstOrDefaultAsync(m => m.DriverId == id);

            // Om föraren inte finns, returnera NotFound
            if (driver == null)
            {
                return NotFound();
            }

            // Filtrera händelser baserat på söksträng, OrdinalIgnoreCase för att ignorera storlek på bokstäver
            if (!string.IsNullOrEmpty(searchQuery))
            {
                driver.Events = driver.Events
                    .Where(e => e.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) || e.Description.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                    .ToList();
            }

            // Filtrera händelser baserat på datumintervall
            if (fromDate.HasValue && toDate.HasValue)
            {
                var startDate = fromDate.Value.Date;
                var endDate = toDate.Value.Date.AddDays(1).AddTicks(-1); // Inkludera hela slutdatumet

                driver.Events = driver.Events
                    .Where(e => e.EventDate >= startDate && e.EventDate <= endDate)
                    .ToList();
            }

            // Skicka värden till vyn för att bevara sök- och filterparametrar
            ViewData["SearchQuery"] = searchQuery;
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            // Returnera föraren till vyn
            return View(driver);
        }


        // GET: Drivers/Create
        public IActionResult Create()
        {
            //Hämta alla anställda för att kunna skapa en dropdown-lista
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name");
            return View();
        }

        // POST: Drivers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("DriverId,Name,LicenseNumber,PhoneNumber,Salary,EmployeeId")] Driver driver)
        {
            if (ModelState.IsValid)
            {
                // Lägg till föraren i databasen
                _context.Add(driver);
                await _context.SaveChangesAsync();

                // Hämta användarnamn för loggningen
                var currentUser = await _userManager.GetUserAsync(User);
                var username = currentUser?.UserName ?? "Okänd användare";

                // Använd LogCreateAsync för att logga skapandet av en ny förare
                await _changeLogService.LogCreateAsync(
                    "Driver",
                    driver.DriverId,
                    driver.Name,
                    "En ny förare skapades",
                    username);

                return RedirectToAction(nameof(Index));
            }
            //Om modellen inte är giltlig, returnera till vyn med samma data
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name", driver.EmployeeId);
            return View(driver);
        }


        // GET: Drivers/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {

            //Om id är null, returnera NotFound
            if (id == null)
            {
                return NotFound();
            }

            //Hämta föraren inklusive anställd
            var driver = await _context.Drivers.FindAsync(id);
            if (driver == null)
            {
                return NotFound();
            }

            //Hämta alla anställda för att kunna skapa en dropdown-lista
            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name", driver.EmployeeId);
            return View(driver);
        }

        // POST: Drivers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("DriverId,Name,LicenseNumber,PhoneNumber,Salary,EmployeeId")] Driver driver)
        {
            if (id != driver.DriverId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    //Hämta den ursprungliga föraren för att jämföra gamla värden med nya. AsNoTracking för att inte spara ändringar
                    var originalDriver = await _context.Drivers.AsNoTracking().FirstOrDefaultAsync(d => d.DriverId == id);

                    // Uppdatera föraren i databasen
                    _context.Update(driver);
                    await _context.SaveChangesAsync();

                    // Hämta användarnamn för loggningen
                    var currentUser = await _userManager.GetUserAsync(User);
                    var username = currentUser?.UserName ?? "Okänd användare";

                    // Logga ändringar om fälten skiljer sig från originalet
                    if (originalDriver.Name != driver.Name)
                    {
                        await _changeLogService.LogChangeAsync(
                            "Driver", driver.DriverId, driver.Name, "Uppdatering",
                            "Namn", originalDriver.Name, driver.Name, username
                        );
                    }
                    if (originalDriver.LicenseNumber != driver.LicenseNumber)
                    {
                        await _changeLogService.LogChangeAsync(
                            "Driver", driver.DriverId, driver.Name, "Uppdatering",
                            "Licensnummer", originalDriver.LicenseNumber, driver.LicenseNumber, username
                        );
                    }
                    if (originalDriver.PhoneNumber != driver.PhoneNumber)
                    {
                        await _changeLogService.LogChangeAsync(
                            "Driver", driver.DriverId, driver.Name, "Uppdatering",
                            "Telefonnummer", originalDriver.PhoneNumber, driver.PhoneNumber, username
                        );
                    }
                    if (originalDriver.Salary != driver.Salary)
                    {
                        await _changeLogService.LogChangeAsync(
                            "Driver", driver.DriverId, driver.Name, "Uppdatering",
                            "Lön", originalDriver.Salary.ToString(), driver.Salary.ToString(), username
                        );
                    }
                    if (originalDriver.EmployeeId != driver.EmployeeId)
                    {
                        var oldEmployee = await _context.Employees.FindAsync(originalDriver.EmployeeId);
                        var newEmployee = await _context.Employees.FindAsync(driver.EmployeeId);
                        var oldEmployeeName = oldEmployee?.Name ?? "Ingen";
                        var newEmployeeName = newEmployee?.Name ?? "Ingen";

                        await _changeLogService.LogChangeAsync(
                            "Driver", driver.DriverId, driver.Name, "Uppdatering",
                            "Ansvarig Anställd", oldEmployeeName, newEmployeeName, username
                        );
                    }
                }


                //Fånga upp fel vid samtidig redigering
                catch (DbUpdateConcurrencyException)
                {
                    if (!DriverExists(driver.DriverId))
                    {
                        return NotFound();
                    } else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            ViewBag.EmployeeList = new SelectList(_context.Employees, "EmployeeId", "Name", driver.EmployeeId);
            return View(driver);
        }

        // GET: Drivers/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var driver = await _context.Drivers
                .Include(d => d.Employee)
                .FirstOrDefaultAsync(m => m.DriverId == id);
            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        // POST: Drivers/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var driver = await _context.Drivers
                .Include(d => d.Events)
                .FirstOrDefaultAsync(d => d.DriverId == id);

            if (driver == null)
            {
                return NotFound();
            }

            // Ta bort alla händelser kopplade till föraren
            if (driver.Events != null)
            {
                _context.Events.RemoveRange(driver.Events);
            }

            // Hämta användarnamn för loggningen
            var currentUser = await _userManager.GetUserAsync(User);
            var username = currentUser?.UserName ?? "Okänd användare";

            //Logga borttagning av föraren
            await _changeLogService.LogDeleteAsync("Driver", driver.DriverId, driver.Name, username);
            ;

            _context.Drivers.Remove(driver);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        //GET: Drivers/DriverDetails/5
        public async Task<IActionResult> DriverDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            //Hämta föraren inklusive anställd och händelser
            var driver = await _context.Drivers
                .Include(d => d.Employee)
                .Include(d => d.Events)
                .FirstOrDefaultAsync(m => m.DriverId == id);

            if (driver == null)
            {
                return NotFound();
            }

            return View(driver);
        }

        private bool DriverExists(int id)
        {
            return _context.Drivers.Any(e => e.DriverId == id);
        }
    }
}
