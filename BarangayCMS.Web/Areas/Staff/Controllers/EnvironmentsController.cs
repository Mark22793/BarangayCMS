using System;
using System.Collections.Generic;
using System.Linq;
using BarangayCMS.Areas.Staff.ViewModels;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using Microsoft.AspNetCore.Mvc;

namespace BarangayCMS.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("Staff/[controller]")]
    [Route("Staff/Environment")]
    public class EnvironmentsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public EnvironmentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /Staff/Environment o /Staff/Environments
        [HttpGet]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            var list = _context.EnvironmentRecords
                .Select(e => new EnvironmentViewModel
                {
                    EnvironmentRecordId = e.EnvironmentRecordId,
                    ActivityName = e.ActivityName,
                    Location = e.LocationArea,
                    ActivityDate = e.InspectionOrActivityDate,
                    Description = e.Remarks
                })
                .OrderByDescending(e => e.ActivityDate)
                .ToList();

            return View(list);
        }

        // GET: /Staff/Environment/Details/5
        [HttpGet("Details/{id}")]
        public IActionResult Details(int id)
        {
            var item = _context.EnvironmentRecords
                .Where(e => e.EnvironmentRecordId == id)
                .Select(e => new EnvironmentViewModel
                {
                    EnvironmentRecordId = e.EnvironmentRecordId,
                    ActivityName = e.ActivityName,
                    Location = e.LocationArea,
                    ActivityDate = e.InspectionOrActivityDate,
                    Description = e.Remarks
                })
                .FirstOrDefault();

            if (item == null) return NotFound();
            return View(item);
        }

        // GET: /Staff/Environment/Create
        [HttpGet("Create")]
        public IActionResult Create()
        {
            return View(new EnvironmentViewModel());
        }

        // POST: /Staff/Environment/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public IActionResult Create(EnvironmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newRecord = new EnvironmentRecord
                {
                    ActivityName = model.ActivityName,
                    LocationArea = model.Location,
                    InspectionOrActivityDate = model.ActivityDate,
                    Remarks = model.Description,
                    WasteManagementStatus = "Compliant",
                    InspectorName = "Barangay Staff",
                    DateLogged = DateTime.Now
                };

                _context.EnvironmentRecords.Add(newRecord);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Staff/Environment/Edit/5
        [HttpGet("Edit/{id}")]
        public IActionResult Edit(int id)
        {
            var item = _context.EnvironmentRecords.FirstOrDefault(e => e.EnvironmentRecordId == id);
            if (item == null) return NotFound();

            var viewModel = new EnvironmentViewModel
            {
                EnvironmentRecordId = item.EnvironmentRecordId,
                ActivityName = item.ActivityName,
                Location = item.LocationArea,
                ActivityDate = item.InspectionOrActivityDate,
                Description = item.Remarks
            };

            return View(viewModel);
        }

        // POST: /Staff/Environment/Edit/5
        [HttpPost("Edit/{id?}")]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, EnvironmentViewModel model)
        {
            if (id == 0) id = model.EnvironmentRecordId;

            if (ModelState.IsValid)
            {
                var existing = _context.EnvironmentRecords.FirstOrDefault(e => e.EnvironmentRecordId == id);
                if (existing == null) return NotFound();

                existing.ActivityName = model.ActivityName;
                existing.LocationArea = model.Location;
                existing.InspectionOrActivityDate = model.ActivityDate;
                existing.Remarks = model.Description;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Staff/Environment/Delete/5
        [HttpGet("Delete/{id}")]
        public IActionResult Delete(int id)
        {
            var item = _context.EnvironmentRecords
                .Where(e => e.EnvironmentRecordId == id)
                .Select(e => new EnvironmentViewModel
                {
                    EnvironmentRecordId = e.EnvironmentRecordId,
                    ActivityName = e.ActivityName,
                    Location = e.LocationArea,
                    ActivityDate = e.InspectionOrActivityDate,
                    Description = e.Remarks
                })
                .FirstOrDefault();

            if (item == null) return NotFound();
            return View(item);
        }

        // POST: /Staff/Environment/Delete/5
        [HttpPost("Delete/{id?}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.EnvironmentRecords.FirstOrDefault(e => e.EnvironmentRecordId == id);
            if (item != null)
            {
                _context.EnvironmentRecords.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}