using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Labb3_DriverInformationSystem.Data;
using Labb3_DriverInformationSystem.Models;
using Microsoft.AspNetCore.Identity;
using Labb3_DriverInformationSystem.Services;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Labb3_DriverInformationSystem.Controllers
{
    public class EventsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ChangeLogService _changeLogService;

        public EventsController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ChangeLogService changeLogService)
        {
            _context = context;
            _userManager = userManager;
            _changeLogService = changeLogService;
        }

        // GET: Events
        public async Task<IActionResult> Index(string searchDriver, DateTime? fromDate, DateTime? toDate)
        {
            var events = _context.Events.Include(e => e.Driver).AsQueryable();

            // Om det finns en söksträng, filtrera efter förarnamn
            if (!string.IsNullOrEmpty(searchDriver))
            {
                events = events.Where(e => e.Driver.Name.Contains(searchDriver));
            }

            // Filtrera efter datumintervall
            if (fromDate.HasValue)
            {
                events = events.Where(e => e.EventDate >= fromDate.Value);
            }

            if (toDate.HasValue)
            {
                events = events.Where(e => e.EventDate <= toDate.Value);
            }

            events = events.OrderByDescending(e => e.EventDate);

            // Spara söksträngar och datumintervall i ViewData för att kunna återanvända dem i vyn
            ViewData["FromDate"] = fromDate?.ToString("yyyy-MM-dd");
            ViewData["ToDate"] = toDate?.ToString("yyyy-MM-dd");

            return View(await events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Driver)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create(int? driverId)
        {
            // Skapa en ny instans av event
            var @event = new Event
            {
                EventDate = DateTime.Now
            };

            // Kolla om vi fått ett driverId som parameter
            if (driverId.HasValue)
            {
                @event.DriverId = driverId.Value; // Förinställ DriverId
            }

            // Skapa en SelectList för att kunna välja förare
            ViewData["DriverId"] = new SelectList(_context.Drivers, "DriverId", "Name", @event.DriverId);
            return View(@event);
        }


        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventId,Title,Description,EventDate,Income,Expense,DriverId")] Event @event)
        {
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();

                // Loggning för skapandet av händelsen
                var currentUser = await _userManager.GetUserAsync(User);
                var username = currentUser?.UserName ?? "Okänd användare";

                await _changeLogService.LogCreateAsync("Event", @event.EventId, @event.Title, "En ny händelse skapades", username);

                // Hämta admin-användare
                var adminRole = await _context.Roles.FirstOrDefaultAsync(r => r.NormalizedName == "ADMIN");
                var adminUserId = await _context.UserRoles
                    .Where(ur => ur.RoleId == adminRole.Id)
                    .Select(ur => ur.UserId)
                    .FirstOrDefaultAsync();

                // Skapa notifikation för den inloggade användaren om användaren inte är admin
                if (currentUser.Id != adminUserId)
                {
                    var userNotification = new UserNotification
                    {
                        // Spara användarid för att kunna länka till användaren
                        UserId = currentUser.Id,
                        // Spara EventId för att kunna länka till händelsen
                        EventId = @event.EventId,

                        // Markera notifikationen som oläst
                        IsRead = false
                    };

                    // Lägg till notifikationen i databasen
                    _context.UserNotifications.Add(userNotification);
                }

                if (!string.IsNullOrEmpty(adminUserId))
                {
                    // Skapa notifikation för admin
                    var adminNotification = new UserNotification
                    {
                        UserId = adminUserId,
                        EventId = @event.EventId,
                        IsRead = false
                    };
                    _context.UserNotifications.Add(adminNotification);
                }

                // Skapa notifikation för den anställda som är ansvarig för föraren (om någon)
                var driver = await _context.Drivers.Include(d => d.Employee).FirstOrDefaultAsync(d => d.DriverId == @event.DriverId);

                if (driver?.Employee != null && driver.Employee.IdentityUserId != currentUser.Id)
                {
                    var employeeNotification = new UserNotification
                    {
                        UserId = driver.Employee.IdentityUserId,
                        EventId = @event.EventId,
                        IsRead = false
                    };
                    _context.UserNotifications.Add(employeeNotification);
                }

                await _context.SaveChangesAsync(); // Spara alla notifikationer

                return RedirectToAction(nameof(Index));
            }

            ViewData["DriverId"] = new SelectList(_context.Drivers, "DriverId", "Name", @event.DriverId);
            return View(@event);
        }




        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            ViewData["DriverId"] = new SelectList(_context.Drivers, "DriverId", "Name", @event.DriverId);
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventId,Title,Description,EventDate,Income,Expense,DriverId")] Event @event)
        {
            if (id != @event.EventId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {


                try
                {
                    //hämtar originalhändelsen för att kunna jämföra ändringar
                    var originalEvent = await _context.Events.AsNoTracking().FirstOrDefaultAsync(e => e.EventId == id);
                    _context.Update(@event);
                    await _context.SaveChangesAsync();

                    // Hämta användarnamn för loggningen
                    var currentUser = await _userManager.GetUserAsync(User);
                    var username = currentUser?.UserName ?? "Okänd användare";

                    // Logga ändringar om fälten skiljer sig från originalet
                    if (originalEvent.Title != @event.Title)
                    {
                        await _changeLogService.LogChangeAsync("Event", @event.EventId, @event.Title, "Uppdatering", "Titel", originalEvent.Title, @event.Title, username);
                    }
                    if (originalEvent.Description != @event.Description)
                    {
                        await _changeLogService.LogChangeAsync("Event", @event.EventId, @event.Title, "Uppdatering", "Beskrivning", originalEvent.Description, @event.Description, username);
                    }
                    if (originalEvent.Income != @event.Income)
                    {
                        await _changeLogService.LogChangeAsync("Event", @event.EventId, @event.Title, "Uppdatering", "Inkomst", originalEvent.Income.ToString(), @event.Income.ToString(), username);
                    }
                    if (originalEvent.Expense != @event.Expense)
                    {
                        await _changeLogService.LogChangeAsync("Event", @event.EventId, @event.Title, "Uppdatering", "Utgift", originalEvent.Expense.ToString(), @event.Expense.ToString(), username);
                    }
                }

                //Felhantering av konflikter i databasen
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventId))
                    {
                        return NotFound();
                    } else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            // Skapa en SelectList för att kunna välja förare
            ViewData["DriverId"] = new SelectList(_context.Drivers, "DriverId", "Name", @event.DriverId);
            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Events
                .Include(e => e.Driver)
                .FirstOrDefaultAsync(m => m.EventId == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var @event = await _context.Events.FindAsync(id);
            if (@event != null)
            {
                // Logga borttagningen av händelsen
                var currentUser = await _userManager.GetUserAsync(User);
                var username = currentUser?.UserName ?? "Okänd användare";

                await _changeLogService.LogDetailedDeleteAsync("Event", @event.EventId, @event.Title, "Händelsen togs bort", username);

                _context.Events.Remove(@event);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // Recent Events
        public async Task<IActionResult> RecentEvents()
        {
            var user = await _userManager.GetUserAsync(User);
            var isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Sätt en tidsgräns för hur gamla händelser som ska visas
            var timeLimit = isAdmin ? DateTime.Now.AddHours(-24) : DateTime.Now.AddHours(-12);

            // Hämta alla nya händelser/notifikationer inom rätt tidsram
            var recentEvents = await _context.Events
                .Include(e => e.Driver)
                .Where(e => e.EventDate >= timeLimit)
                .OrderByDescending(e => e.EventDate)
                .ToListAsync();

            // Markera alla olästa notifikationer som lästa
            var unreadNotifications = await _context.UserNotifications
                .Where(un => un.UserId == user.Id && !un.IsRead)
                .ToListAsync();

            foreach (var notification in unreadNotifications)
            {
                notification.IsRead = true; // Markera som läst
            }

            await _context.SaveChangesAsync(); // Spara ändringar i databasen

            return View(recentEvents);
        }




        private bool EventExists(int id)
        {
            return _context.Events.Any(e => e.EventId == id);
        }
    }
}
