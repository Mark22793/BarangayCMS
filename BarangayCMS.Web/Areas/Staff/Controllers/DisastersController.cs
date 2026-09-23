using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
// 🔥 Alias para maiwasan ang Ambiguous Reference CS0104 error
using StaffDisasterVM = BarangayCMS.Web.Areas.Staff.Models.DisasterViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("Staff/[controller]")]
    [Route("Staff/Disaster")]
    [Authorize(Roles = "Staff,Admin,SuperAdmin")]
    public class DisastersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IEvacuationService _evacuationService;

        private const string LocationSeparator = " | Lokasyon: ";

        public DisastersController(ApplicationDbContext context, IEvacuationService evacuationService)
        {
            _context = context;
            _evacuationService = evacuationService;
        }

        // GET: /Staff/Disaster o /Staff/Disasters
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var disasters = await _context.Disasters
                .AsNoTracking()
                .OrderByDescending(d => d.OccurrenceDate)
                .ToListAsync();

            var list = disasters.Select(MapToViewModel).ToList();
            return View(list);
        }

        // GET: /Staff/Disasters/HazardMaps
        [HttpGet("HazardMaps")]
        public IActionResult HazardMaps()
        {
            return View();
        }

        // GET: /Staff/Disasters/EvacuationCenters
        [HttpGet("EvacuationCenters")]
        public async Task<IActionResult> EvacuationCenters()
        {
            var evacuationInfo = await _evacuationService.GetPublicEvacuationInfoAsync();
            // 🔑 INAYOS: Explicitly na itinuro sa "EvacuationCenter.cshtml" (nang walang 's')
            return View("EvacuationCenter", evacuationInfo);
        }

        // GET: /Staff/Disasters/SmsHistory
        [HttpGet("SmsHistory")]
        public IActionResult SmsHistory()
        {
            return View();
        }

        // GET: /Staff/Disasters/Manage/5
        [HttpGet("Manage/{id:int}")]
        public async Task<IActionResult> Manage(int id)
        {
            var item = await _context.Disasters.AsNoTracking().FirstOrDefaultAsync(d => d.DisasterId == id);
            if (item == null) return NotFound();

            return View(MapToViewModel(item));
        }

        // GET: /Staff/Disasters/Details/5
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Disasters.AsNoTracking().FirstOrDefaultAsync(d => d.DisasterId == id);
            if (item == null) return NotFound();

            return View(MapToViewModel(item));
        }

        // GET: /Staff/Disasters/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new StaffDisasterVM { DateOccurred = DateTime.Now });
        }

        // POST: /Staff/Disasters/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(StaffDisasterVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var disaster = new Disaster
            {
                IncidentName = EncodeIncidentName(model),
                DisasterType = model.DisasterType,
                OccurrenceDate = model.DateOccurred,
                AffectedHouseholdsCount = 0,
                DisplacedIndividualsCount = 0,
                CasualtiesCount = 0,
                EvacuationCenterStatus = string.Equals(model.Status, "Active", StringComparison.OrdinalIgnoreCase) ? "Open" : "Closed",
                ReliefDistributionStatus = string.Equals(model.Status, "Active", StringComparison.OrdinalIgnoreCase) ? "Ongoing" : "Completed",
                LoggedBy = User.Identity?.Name ?? "Staff",
                DateCreated = DateTime.Now
            };

            _context.Disasters.Add(disaster);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Staff/Disasters/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Disasters.FindAsync(id);
            if (item == null) return NotFound();

            return View(MapToViewModel(item));
        }

        // POST: /Staff/Disasters/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, StaffDisasterVM model)
        {
            if (!ModelState.IsValid) return View(model);

            var existing = await _context.Disasters.FindAsync(id);
            if (existing == null) return NotFound();

            existing.IncidentName = EncodeIncidentName(model);
            existing.DisasterType = model.DisasterType;
            existing.OccurrenceDate = model.DateOccurred;
            existing.EvacuationCenterStatus = string.Equals(model.Status, "Active", StringComparison.OrdinalIgnoreCase) ? "Open" : "Closed";
            existing.ReliefDistributionStatus = string.Equals(model.Status, "Active", StringComparison.OrdinalIgnoreCase) ? "Ongoing" : "Completed";
            existing.LoggedBy = User.Identity?.Name ?? "Staff";
            existing.DateUpdated = DateTime.Now;

            _context.Disasters.Update(existing);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        // GET: /Staff/Disasters/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Disasters.AsNoTracking().FirstOrDefaultAsync(d => d.DisasterId == id);
            if (item == null) return NotFound();

            return View(MapToViewModel(item));
        }

        // POST: /Staff/Disasters/Delete/5
        [HttpPost("Delete/{id:int}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var item = await _context.Disasters.FindAsync(id);
            if (item != null)
            {
                _context.Disasters.Remove(item);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        #region 🛠 Mapping Helpers

        private static StaffDisasterVM MapToViewModel(Disaster d)
        {
            var description = d.IncidentName;
            var location = "Barangay Jurisdiction";

            if (!string.IsNullOrEmpty(d.IncidentName) && d.IncidentName.Contains(LocationSeparator))
            {
                var parts = d.IncidentName.Split(new[] { LocationSeparator }, StringSplitOptions.None);
                description = parts[0];
                location = parts.Length > 1 ? parts[1] : location;
            }

            var isActive = string.Equals(d.EvacuationCenterStatus, "Open", StringComparison.OrdinalIgnoreCase)
                || string.Equals(d.ReliefDistributionStatus, "Ongoing", StringComparison.OrdinalIgnoreCase);

            return new StaffDisasterVM
            {
                Id = d.DisasterId,
                DisasterType = d.DisasterType ?? string.Empty,
                Description = description ?? string.Empty,
                Location = location ?? string.Empty,
                DateOccurred = d.OccurrenceDate,
                Status = isActive ? "Active" : "Resolved"
            };
        }

        private static string EncodeIncidentName(StaffDisasterVM model)
        {
            var name = !string.IsNullOrWhiteSpace(model.Description)
                ? model.Description
                : $"{model.DisasterType} Incident";

            if (!string.IsNullOrWhiteSpace(model.Location))
            {
                name += $"{LocationSeparator}{model.Location}";
            }
            return name;
        }

        #endregion
    }
}