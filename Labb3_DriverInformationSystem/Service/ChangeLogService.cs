using System.Threading.Tasks;
using Labb3_DriverInformationSystem.Data;
using Labb3_DriverInformationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace Labb3_DriverInformationSystem.Services
{
    public class ChangeLogService
    {
        private readonly ApplicationDbContext _context;

        public ChangeLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        // Logga ändringar i systemet
        public async Task LogChangeAsync(string entityName, int entityId, string affectedName, string changeType, string propertyChanged, string oldValue, string newValue, string changedBy)
        {
            var changeLog = new ChangeLog
            {
                EntityName = entityName,
                EntityId = entityId,
                AffectedName = affectedName,
                ChangeType = changeType,
                PropertyChanged = propertyChanged,
                OldValue = oldValue,
                NewValue = newValue,
                ChangeDescription = $"{propertyChanged} ändrades från {oldValue} till {newValue}",
                ChangedBy = changedBy
            };

            _context.ChangeLogs.Add(changeLog);
            await _context.SaveChangesAsync();
        }

        // Logga skapelse av en ny post
        public async Task LogCreateAsync(string entityName, int entityId, string affectedName, string description, string changedBy)
        {
            var changeLog = new ChangeLog
            {
                EntityName = entityName,
                EntityId = entityId,
                AffectedName = affectedName,
                ChangeType = "Ny Skapelse", 
                ChangeDescription = description,
                ChangedBy = changedBy,
                OldValue = null, //Inget gammalt värde eftersom det är en ny post
                NewValue = affectedName // Det nya värdet är namnet på entiteten som skapades (föraren, anställd etc.)
            };

            _context.ChangeLogs.Add(changeLog);
            await _context.SaveChangesAsync();
        }

        // Logga borttagning av en post
        public async Task LogDeleteAsync(string entityName, int entityId, string affectedName, string changedBy)
        {
            var changeLog = new ChangeLog
            {
                EntityName = entityName,
                EntityId = entityId,
                AffectedName = affectedName,
                ChangeType = "Borttagning",
                PropertyChanged = null,
                OldValue = null,
                NewValue = null,
                ChangeDescription = $"{affectedName} togs bort från systemet.",
                ChangedBy = changedBy
            };

            _context.ChangeLogs.Add(changeLog);
            await _context.SaveChangesAsync();
        }

        public async Task LogEventChangeAsync(string changeType, int eventId, string eventName, string changeDescription, string changedBy)
        {
            var changeLog = new ChangeLog
            {
                EntityName = "Event",
                EntityId = eventId,
                AffectedName = eventName,
                ChangeType = changeType, // T.ex. "Create", "Update", "Delete"
                ChangeDescription = changeDescription, // T.ex. "Event skapades", "Event uppdaterades"
                ChangedBy = changedBy,
                ChangeDate = DateTime.Now
            };

            _context.ChangeLogs.Add(changeLog);
            await _context.SaveChangesAsync();
        }

        // Logga detaljerad borttagning av en post
        public async Task LogDetailedDeleteAsync(string entityName, int entityId, string affectedName, string changeDescription, string changedBy)
        {
            var changeLog = new ChangeLog
            {
                EntityName = entityName,
                EntityId = entityId,
                AffectedName = affectedName,
                ChangeType = "Borttagning",
                ChangeDescription = changeDescription, // Specifik borttagningsbeskrivning
                ChangedBy = changedBy
            };

            _context.ChangeLogs.Add(changeLog);
            await _context.SaveChangesAsync();
        }



        // Hämta alla ändringsloggar
        public async Task<List<ChangeLog>> GetAllChangeLogsAsync()
        {
            return await _context.ChangeLogs.OrderByDescending(c => c.ChangeDate).ToListAsync();
        }

        // Hämta specifika loggar för en viss entitet (t.ex. "Driver" eller "Employee")
        public async Task<List<ChangeLog>> GetChangeLogsForEntityAsync(string entityName)
        {
            return await _context.ChangeLogs
                .Where(c => c.EntityName == entityName)
                .OrderByDescending(c => c.ChangeDate)
                .ToListAsync();
        }
    }
}
