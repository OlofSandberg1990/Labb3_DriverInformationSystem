using System;
using System.Linq;
using System.Threading.Tasks;
using Labb3_DriverInformationSystem.Data;
using Labb3_DriverInformationSystem.Models;
using Labb3_DriverInformationSystem.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Labb3_DriverInformationSystem.Controllers
{

    [Authorize(Roles = "Admin")]
    public class EmployeesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ChangeLogService _changeLogService;

        public EmployeesController(ApplicationDbContext context, UserManager<IdentityUser> userManager, ChangeLogService changeLogService)
        {
            _context = context;
            _userManager = userManager;
            _changeLogService = changeLogService;
        }

        // GET: Employees
        public async Task<IActionResult> Index()
        {

            //Hämta alla anställda inklusive användarinformation från IdentoityUser
            var employees = await _context.Employees.Include(e => e.IdentityUser).ToListAsync();
            return View(employees);
        }

        // GET: Employees/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            
            var employee = await _context.Employees
                .Include(e => e.Drivers)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        // GET: Employees/Create
        public IActionResult Create()
        {

            //Retunerar en tom vy-modell för att skapa en ny anställd
            return View(new EmployeeViewModel());
        }

        // POST: Employees/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Skapa en ny IdentityUser med e-postadressen som användarnamn
                var user = new IdentityUser { UserName = model.Email, Email = model.Email };

                // Skapa användare i Identity baserat på det användarnamn och lösenord som skickas in
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    var employee = new Employee
                    {
                        Name = model.Name,
                        Phonenumber = model.Phonenumber,
                        DateOfHire = model.DateOfHire,
                        IdentityUserId = user.Id
                    };

                    //Sätt rollen Employee till den nyskapade användaren
                    await _userManager.AddToRoleAsync(user, "Employee");

                    _context.Employees.Add(employee);
                    await _context.SaveChangesAsync();

                    // Logga händelsen för att skapa anställd
                    var currentUser = await _userManager.GetUserAsync(User);
                    var username = currentUser?.UserName ?? "Okänd användare";
                    await _changeLogService.LogCreateAsync("Employee", employee.EmployeeId, employee.Name, "Ny anställd skapades", username);

                    return RedirectToAction(nameof(Index));
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }


        // GET: Employees/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Hämta anställd inklusive användarinformation från databasen
            var employee = await _context.Employees
                .Include(e => e.IdentityUser)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Skapa en vy-modell för att skicka till vyn
            var model = new EmployeeViewModel
            {
                Name = employee.Name,
                Phonenumber = employee.Phonenumber,
                DateOfHire = employee.DateOfHire,
                Email = employee.IdentityUser?.Email
            };

            return View(model);
        }
   
        // POST: Employees/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Hämta anställd inklusive användarinformation från databasen
            var employee = await _context.Employees
                .Include(e => e.IdentityUser)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Hämta användarnamn för loggningen
            var currentUser = await _userManager.GetUserAsync(User);
            var username = currentUser?.UserName ?? "Okänd användare";

            // Logga ändringar om fälten skiljer sig från originalet
            if (employee.Name != model.Name)
            {
                await _changeLogService.LogChangeAsync(
                    "Employee", employee.EmployeeId, employee.Name, "Uppdatering",
                    "Namn", employee.Name, model.Name, username
                );
            }
            if (employee.Phonenumber != model.Phonenumber)
            {
                await _changeLogService.LogChangeAsync(
                    "Employee", employee.EmployeeId, employee.Name, "Uppdatering",
                    "Telefonnummer", employee.Phonenumber, model.Phonenumber, username
                );
            }
            if (employee.DateOfHire != model.DateOfHire)
            {
                await _changeLogService.LogChangeAsync(
                    "Employee", employee.EmployeeId, employee.Name, "Uppdatering",
                    "Anställningsdatum", employee.DateOfHire.ToString(), model.DateOfHire.ToString(), username
                );
            }

            employee.Name = model.Name;
            employee.Phonenumber = model.Phonenumber;
            employee.DateOfHire = model.DateOfHire;

            if (employee.IdentityUser != null)
            {
                if (employee.IdentityUser.Email != model.Email)
                {
                    await _changeLogService.LogChangeAsync(
                        "Employee", employee.EmployeeId, employee.Name, "Uppdatering",
                        "E-post", employee.IdentityUser.Email, model.Email, username
                    );
                    employee.IdentityUser.Email = model.Email;
                }

                if (!string.IsNullOrEmpty(model.Password))
                {
                    var token = await _userManager.GeneratePasswordResetTokenAsync(employee.IdentityUser);
                    await _userManager.ResetPasswordAsync(employee.IdentityUser, token, model.Password);
                }
            }

            _context.Update(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        // GET: Employees/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var employee = await _context.Employees
                .AsNoTracking()
                .Include(e => e.IdentityUser)
                .FirstOrDefaultAsync(e => e.EmployeeId == id);
            if (employee == null)
            {
                return NotFound();
            }

            return View(employee);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var employee = await _context.Employees
                .Include(e => e.Drivers)
                .FirstOrDefaultAsync(m => m.EmployeeId == id);

            if (employee == null)
            {
                return NotFound();
            }

            // Kontrollera om anställd har kopplade förare
            if (employee.Drivers.Any())
            {
                // Generera ett meddelande med förarnamn och antal
                var driverNames = string.Join(", ", employee.Drivers.Select(d => d.Name));
                var message = $"Kan ej ta bort {employee.Name} eftersom hen ansvarar för {employee.Drivers.Count} förare: {driverNames}. Ändra detta och försök igen.";

                // Lägg till ett felmeddelande i ModelState som visas i vyn
                ModelState.AddModelError("", message);
                return View(employee); // Återgå till vyn med felmeddelandet
            }

            // Om inga förare är kopplade, logga borttagningen
            var currentUser = await _userManager.GetUserAsync(User);
            var username = currentUser?.UserName ?? "Okänd användare";

            await _changeLogService.LogDeleteAsync("Employee", employee.EmployeeId, employee.Name, username);

            // Ta bort anställd från databasen
            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }


        private bool EmployeeExists(int id)
        {
            return _context.Employees.Any(e => e.EmployeeId == id);
        }
    }
}
