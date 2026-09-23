using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.Areas.Staff.ViewModels;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("Staff/[controller]")]
    [Route("Staff/Disaster")]
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
                .OrderByDescending(d => d.OccurrenceDate)
                .ToListAsync();

            ViewBag.Evacuation = await _evacuationService.GetPublicEvacuationInfoAsync();

            var list = disasters.Select(MapToViewModel).ToList();
            return View(list);
        }

        // GET: /Staff/Disasters/Manage/5
        [HttpGet("Manage/{id}")]
        public async Task<IActionResult> Manage(int id)
        {
            var item = await _context.Disasters.FindAsync(id);
            if (item == null) return NotFound();
            return View(MapToViewModel(item));
        }

        // GET: /Staff/Disasters/Details/5
        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
        {
            var item = await _context.Disasters.FindAsync(id);
            if (item == null) return NotFound();
            return View(MapToViewModel(item));
        }

        // GET: /Staff/Disasters/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new DisasterViewModel { OccurrenceDate = DateTime.Now });
        }

        // POST: /Staff/Disasters/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DisasterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var disaster = new Disaster
                {
                    IncidentName = EncodeIncidentName(model),
                    DisasterType = model.DisasterType,
                    OccurrenceDate = model.OccurrenceDate,
                    AffectedHouseholdsCount = model.AffectedHouseholdsCount,
                    DisplacedIndividualsCount = model.DisplacedIndividualsCount,
                    CasualtiesCount = 0,
                    EvacuationCenterStatus = string.IsNullOrWhiteSpace(model.EvacuationCenterStatus) ? "Closed" : model.EvacuationCenterStatus,
                    ReliefDistributionStatus = string.IsNullOrWhiteSpace(model.ReliefDistributionStatus) ? "Ongoing" : model.ReliefDistributionStatus,
                    LoggedBy = User.Identity?.Name ?? "Staff",
                    DateCreated = DateTime.Now
                };

                _context.Disasters.Add(disaster);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Staff/Disasters/Edit/5
        [HttpGet("Edit/{id}")]
        public async Task<IActionResult> Edit(int id)
        {
            var item = await _context.Disasters.FindAsync(id);
            if (item == null) return NotFound();
            return View(MapToViewModel(item));
        }

        // POST: /Staff/Disasters/Edit/5
        [HttpPost("Edit/{id?}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DisasterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var existing = await _context.Disasters.FindAsync(id);
                if (existing == null) return NotFound();

                existing.IncidentName = EncodeIncidentName(model);
                existing.DisasterType = model.DisasterType;
                existing.OccurrenceDate = model.OccurrenceDate;
                existing.AffectedHouseholdsCount = model.AffectedHouseholdsCount;
                existing.DisplacedIndividualsCount = model.DisplacedIndividualsCount;
                existing.EvacuationCenterStatus = string.IsNullOrWhiteSpace(model.EvacuationCenterStatus) ? "Closed" : model.EvacuationCenterStatus;
                existing.ReliefDistributionStatus = string.IsNullOrWhiteSpace(model.ReliefDistributionStatus) ? "Ongoing" : model.ReliefDistributionStatus;
                existing.LoggedBy = User.Identity?.Name ?? "Staff";
                existing.DateUpdated = DateTime.Now;

                _context.Disasters.Update(existing);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Staff/Disasters/Delete/5
        [HttpGet("Delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var item = await _context.Disasters.FindAsync(id);
            if (item == null) return NotFound();
            return View(MapToViewModel(item));
        }

        // POST: /Staff/Disasters/Delete/5
        [HttpPost("Delete/{id?}"), ActionName("Delete")]
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

        private static DisasterViewModel MapToViewModel(Disaster d)
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

            return new DisasterViewModel
            {
                Id = d.DisasterId,
                IncidentName = description,
                DisasterType = d.DisasterType,
                Description = description,
                Location = location,
                OccurrenceDate = d.OccurrenceDate,
                AffectedHouseholdsCount = d.AffectedHouseholdsCount,
                DisplacedIndividualsCount = d.DisplacedIndividualsCount,
                EvacuationCenterStatus = d.EvacuationCenterStatus,
                ReliefDistributionStatus = d.ReliefDistributionStatus,
                Status = isActive ? "Active" : "Resolved"
            };
        }

        private static string EncodeIncidentName(DisasterViewModel model)
        {
            var name = !string.IsNullOrWhiteSpace(model.IncidentName)
                ? model.IncidentName
                : $"{model.DisasterType} Incident";

            if (!string.IsNullOrWhiteSpace(model.Location))
            {
                name += $"{LocationSeparator}{model.Location}";
            }
            return name;
        }
    }
}