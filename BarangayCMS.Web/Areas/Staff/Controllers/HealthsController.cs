using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Staff.ViewModels; // Inupdate sa Staff Models namespace
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class HealthController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HealthController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. GET: Staff/Health
        public async Task<IActionResult> Index()
        {
            var records = await _context.Set<HealthRecord>()
                .OrderByDescending(h => h.DateLogged)
                .Select(h => new HealthViewModel
                {
                    Id = h.HealthRecordId,
                    ResidentId = h.ResidentId,
                    WeightKg = h.WeightKg,
                    HeightCm = h.HeightCm,
                    BloodType = h.BloodType,
                    HealthClassification = h.HealthClassification,
                    IsVaccinated = h.IsVaccinated,
                    MedicalCondition = h.MedicalCondition,
                    AttendingHealthWorker = h.AttendingHealthWorker,
                    Remarks = h.Remarks,
                    LastCheckupDate = h.LastCheckupDate,
                    DateRecorded = h.DateLogged,

                    ResidentName = _context.Residents
                        .Where(r => r.ResidentId == h.ResidentId)
                        .Select(r => r.LastName + ", " + r.FirstName + (string.IsNullOrEmpty(r.MiddleName) ? "" : " " + r.MiddleName))
                        .FirstOrDefault() ?? "Unknown Resident"
                }).ToListAsync();

            return View(records);
        }

        // 2. GET: Staff/Health/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.Set<HealthRecord>()
                .FirstOrDefaultAsync(h => h.HealthRecordId == id);

            if (record == null) return NotFound();

            var model = new HealthViewModel
            {
                Id = record.HealthRecordId,
                ResidentId = record.ResidentId,
                WeightKg = record.WeightKg,
                HeightCm = record.HeightCm,
                BloodType = record.BloodType,
                HealthClassification = record.HealthClassification,
                IsVaccinated = record.IsVaccinated,
                MedicalCondition = record.MedicalCondition,
                AttendingHealthWorker = record.AttendingHealthWorker,
                Remarks = record.Remarks,
                LastCheckupDate = record.LastCheckupDate,
                DateRecorded = record.DateLogged,
                ResidentName = _context.Residents
                    .Where(r => r.ResidentId == record.ResidentId)
                    .Select(r => r.LastName + ", " + r.FirstName + (string.IsNullOrEmpty(r.MiddleName) ? "" : " " + r.MiddleName))
                    .FirstOrDefault() ?? "Unknown Resident"
            };

            return View(model);
        }

        // 3. GET: Staff/Health/Create
        public async Task<IActionResult> Create()
        {
            var residents = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new
                {
                    Id = r.ResidentId,
                    FullName = r.LastName + ", " + r.FirstName + (string.IsNullOrEmpty(r.MiddleName) ? "" : " " + r.MiddleName.Substring(0, 1) + ".")
                })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residents, "Id", "FullName");

            return View(new HealthViewModel { DateRecorded = DateTime.Now });
        }

        // 4. POST: Staff/Health/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(HealthViewModel model)
        {
            if (ModelState.IsValid)
            {
                var healthRecord = new HealthRecord
                {
                    ResidentId = model.ResidentId,
                    MedicalCondition = model.MedicalCondition,
                    WeightKg = model.WeightKg,
                    HeightCm = model.HeightCm,
                    BloodType = string.IsNullOrWhiteSpace(model.BloodType) ? "N/A" : model.BloodType,
                    HealthClassification = string.IsNullOrWhiteSpace(model.HealthClassification) ? "General" : model.HealthClassification,
                    IsVaccinated = model.IsVaccinated,
                    AttendingHealthWorker = string.IsNullOrWhiteSpace(model.AttendingHealthWorker) ? "Barangay Health Worker" : model.AttendingHealthWorker,
                    Remarks = model.Remarks ?? string.Empty,
                    DateLogged = model.DateRecorded,
                    LastCheckupDate = model.LastCheckupDate == default ? model.DateRecorded : model.LastCheckupDate
                };

                _context.Add(healthRecord);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            var residentsList = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new
                {
                    Id = r.ResidentId,
                    FullName = r.LastName + ", " + r.FirstName
                })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residentsList, "Id", "FullName");

            return View(model);
        }

        // 5. GET: Staff/Health/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.Set<HealthRecord>()
                .FirstOrDefaultAsync(h => h.HealthRecordId == id);

            if (record == null) return NotFound();

            var model = new HealthViewModel
            {
                Id = record.HealthRecordId,
                ResidentId = record.ResidentId,
                WeightKg = record.WeightKg,
                HeightCm = record.HeightCm,
                BloodType = record.BloodType,
                HealthClassification = record.HealthClassification,
                IsVaccinated = record.IsVaccinated,
                MedicalCondition = record.MedicalCondition,
                AttendingHealthWorker = record.AttendingHealthWorker,
                Remarks = record.Remarks,
                LastCheckupDate = record.LastCheckupDate,
                DateRecorded = record.DateLogged
            };

            var residents = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new
                {
                    Id = r.ResidentId,
                    FullName = r.LastName + ", " + r.FirstName + (string.IsNullOrEmpty(r.MiddleName) ? "" : " " + r.MiddleName.Substring(0, 1) + ".")
                })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residents, "Id", "FullName", model.ResidentId);

            return View(model);
        }

        // 6. POST: Staff/Health/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, HealthViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var recordToUpdate = await _context.Set<HealthRecord>()
                        .FirstOrDefaultAsync(h => h.HealthRecordId == id);

                    if (recordToUpdate == null) return NotFound();

                    recordToUpdate.ResidentId = model.ResidentId;
                    recordToUpdate.MedicalCondition = model.MedicalCondition;
                    recordToUpdate.WeightKg = model.WeightKg;
                    recordToUpdate.HeightCm = model.HeightCm;
                    recordToUpdate.BloodType = string.IsNullOrWhiteSpace(model.BloodType) ? "N/A" : model.BloodType;
                    recordToUpdate.HealthClassification = string.IsNullOrWhiteSpace(model.HealthClassification) ? "General" : model.HealthClassification;
                    recordToUpdate.IsVaccinated = model.IsVaccinated;
                    recordToUpdate.AttendingHealthWorker = string.IsNullOrWhiteSpace(model.AttendingHealthWorker) ? "Barangay Health Worker" : model.AttendingHealthWorker;
                    recordToUpdate.Remarks = model.Remarks ?? string.Empty;
                    recordToUpdate.DateLogged = model.DateRecorded;
                    recordToUpdate.LastCheckupDate = model.LastCheckupDate == default ? model.DateRecorded : model.LastCheckupDate;

                    _context.Update(recordToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Set<HealthRecord>().Any(e => e.HealthRecordId == model.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }

            var residentsList = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new { Id = r.ResidentId, FullName = r.LastName + ", " + r.FirstName })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residentsList, "Id", "FullName", model.ResidentId);

            return View(model);
        }

        // 7. GET: Staff/Health/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var record = await _context.Set<HealthRecord>()
                .FirstOrDefaultAsync(h => h.HealthRecordId == id);

            if (record == null) return NotFound();

            var model = new HealthViewModel
            {
                Id = record.HealthRecordId,
                ResidentId = record.ResidentId,
                WeightKg = record.WeightKg,
                HeightCm = record.HeightCm,
                BloodType = record.BloodType,
                HealthClassification = record.HealthClassification,
                IsVaccinated = record.IsVaccinated,
                MedicalCondition = record.MedicalCondition,
                AttendingHealthWorker = record.AttendingHealthWorker,
                Remarks = record.Remarks,
                LastCheckupDate = record.LastCheckupDate,
                DateRecorded = record.DateLogged,
                ResidentName = _context.Residents
                    .Where(r => r.ResidentId == record.ResidentId)
                    .Select(r => r.LastName + ", " + r.FirstName)
                    .FirstOrDefault() ?? "Unknown Resident"
            };

            return View(model);
        }

        // 8. POST: Staff/Health/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var record = await _context.Set<HealthRecord>()
                .FirstOrDefaultAsync(h => h.HealthRecordId == id);

            if (record != null)
            {
                _context.Set<HealthRecord>().Remove(record);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}