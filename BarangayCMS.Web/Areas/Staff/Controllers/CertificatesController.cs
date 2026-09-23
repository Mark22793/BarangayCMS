using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Staff.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BarangayCMS.Web.Areas.Staff.Controllers
{
    [Area("Staff")]
    [Route("Staff/[controller]")]
    public class CertificatesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CertificatesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Staff/Certificates o Staff/Certificates/Index
        [HttpGet]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var certificates = await _context.Set<Certificate>()
                .OrderByDescending(c => c.DateRequested)
                .Select(c => new CertificateViewModel
                {
                    CertificateId = c.CertificateId,
                    ResidentId = c.ResidentId ?? 0,
                    CertificateType = c.CertificateType ?? "Barangay Clearance",
                    Purpose = c.Purpose ?? string.Empty,
                    ControlNumber = string.IsNullOrEmpty(c.ControlNumber) ? $"CERT-{c.CertificateId}" : c.ControlNumber,
                    Status = c.Status ?? "Pending",
                    FeePaid = c.FeePaid,
                    PaymentReceiptPath = c.PaymentReceiptPath ?? string.Empty,
                    DateRequested = c.DateRequested,
                    DateIssued = c.DateIssued,
                    IssuedBy = c.IssuedBy ?? string.Empty,
                    ResidentName = string.IsNullOrEmpty(c.ResidentName)
                        ? (_context.Residents
                            .Where(r => r.ResidentId == c.ResidentId)
                            .Select(r => r.LastName + ", " + r.FirstName + (string.IsNullOrEmpty(r.MiddleName) ? "" : " " + r.MiddleName))
                            .FirstOrDefault() ?? "Unknown Resident")
                        : c.ResidentName
                }).ToListAsync();

            return View(certificates);
        }

        // 📌 INIDAGDAG: GET & POST para sa Approve (Resolves 404 Error: /Staff/Certificates/Approve/10)
        [HttpGet("Approve/{id:int}")]
        [HttpPost("Approve/{id:int}")]
        public async Task<IActionResult> Approve(int id)
        {
            var cert = await _context.Set<Certificate>().FindAsync(id);
            if (cert == null) return NotFound();

            cert.Status = "Approved";
            cert.DateIssued = DateTime.Now;
            cert.IssuedBy = User.Identity?.Name ?? "Barangay Staff";

            _context.Set<Certificate>().Update(cert);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // GET: Staff/Certificates/Create
        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            await PopulateResidentsDropDownList();
            return View(new CertificateViewModel { DateRequested = DateTime.Now });
        }

        // POST: Staff/Certificates/Create
        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CertificateViewModel model)
        {
            if (ModelState.IsValid)
            {
                var residentName = model.ResidentName;
                if (string.IsNullOrEmpty(residentName) && model.ResidentId > 0)
                {
                    var resident = await _context.Residents.FindAsync(model.ResidentId);
                    if (resident != null)
                    {
                        residentName = $"{resident.LastName}, {resident.FirstName}";
                    }
                }

                var certificate = new Certificate
                {
                    ResidentId = model.ResidentId > 0 ? model.ResidentId : null,
                    ResidentName = residentName ?? "Walk-in Resident",
                    CertificateType = model.CertificateType,
                    Purpose = model.Purpose,
                    ControlNumber = "CERT-" + DateTime.Now.ToString("yyyyMMddHHmmss"),
                    Status = string.IsNullOrEmpty(model.Status) ? "Pending" : model.Status,
                    FeePaid = model.FeePaid,
                    PaymentReceiptPath = model.PaymentReceiptPath,
                    DateRequested = DateTime.Now
                };

                _context.Add(certificate);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            await PopulateResidentsDropDownList(model.ResidentId);
            return View(model);
        }

        // GET: Staff/Certificates/Edit/5
        [HttpGet("Edit/{id:int}")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var cert = await _context.Set<Certificate>().FindAsync(id);
            if (cert == null) return NotFound();

            var model = new CertificateViewModel
            {
                CertificateId = cert.CertificateId,
                ResidentId = cert.ResidentId ?? 0,
                ResidentName = cert.ResidentName,
                CertificateType = cert.CertificateType,
                Purpose = cert.Purpose,
                ControlNumber = cert.ControlNumber,
                Status = cert.Status,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath,
                DateRequested = cert.DateRequested,
                DateIssued = cert.DateIssued,
                IssuedBy = cert.IssuedBy
            };

            await PopulateResidentsDropDownList(model.ResidentId);
            return View(model);
        }

        // POST: Staff/Certificates/Edit/5
        [HttpPost("Edit/{id:int}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CertificateViewModel model)
        {
            if (id != model.CertificateId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var certToUpdate = await _context.Set<Certificate>().FindAsync(id);
                    if (certToUpdate == null) return NotFound();

                    certToUpdate.ResidentId = model.ResidentId > 0 ? model.ResidentId : null;
                    certToUpdate.CertificateType = model.CertificateType;
                    certToUpdate.Purpose = model.Purpose;
                    certToUpdate.Status = model.Status;
                    certToUpdate.FeePaid = model.FeePaid;

                    if (model.Status == "Approved" && certToUpdate.DateIssued == null)
                    {
                        certToUpdate.DateIssued = DateTime.Now;
                        certToUpdate.IssuedBy = User.Identity?.Name ?? "Barangay Staff";
                    }

                    _context.Update(certToUpdate);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Set<Certificate>().Any(e => e.CertificateId == model.CertificateId))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }

            await PopulateResidentsDropDownList(model.ResidentId);
            return View(model);
        }

        // GET: Staff/Certificates/Details/5
        [HttpGet("Details/{id:int}")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var cert = await _context.Set<Certificate>().FirstOrDefaultAsync(m => m.CertificateId == id);
            if (cert == null) return NotFound();

            var model = new CertificateViewModel
            {
                CertificateId = cert.CertificateId,
                ResidentId = cert.ResidentId ?? 0,
                ResidentName = cert.ResidentName,
                CertificateType = cert.CertificateType,
                Purpose = cert.Purpose,
                ControlNumber = cert.ControlNumber,
                Status = cert.Status,
                FeePaid = cert.FeePaid,
                PaymentReceiptPath = cert.PaymentReceiptPath,
                DateRequested = cert.DateRequested,
                DateIssued = cert.DateIssued,
                IssuedBy = cert.IssuedBy
            };

            return View(model);
        }

        // GET: Staff/Certificates/Delete/5
        [HttpGet("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var cert = await _context.Set<Certificate>().FirstOrDefaultAsync(m => m.CertificateId == id);
            if (cert == null) return NotFound();

            var model = new CertificateViewModel
            {
                CertificateId = cert.CertificateId,
                ResidentId = cert.ResidentId ?? 0,
                ResidentName = cert.ResidentName,
                CertificateType = cert.CertificateType,
                Purpose = cert.Purpose,
                ControlNumber = cert.ControlNumber,
                Status = cert.Status,
                FeePaid = cert.FeePaid,
                DateRequested = cert.DateRequested
            };

            return View(model);
        }

        // POST: Staff/Certificates/Delete/5
        [HttpPost("Delete/{id:int}"), ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var cert = await _context.Set<Certificate>().FindAsync(id);
            if (cert != null)
            {
                _context.Set<Certificate>().Remove(cert);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Staff/Certificates/Print/5
        [HttpGet("Print/{id:int}")]
        public async Task<IActionResult> Print(int? id)
        {
            if (id == null) return NotFound();

            var cert = await _context.Set<Certificate>().FirstOrDefaultAsync(m => m.CertificateId == id);
            if (cert == null) return NotFound();

            var model = new CertificateViewModel
            {
                CertificateId = cert.CertificateId,
                ResidentId = cert.ResidentId ?? 0,
                ResidentName = cert.ResidentName,
                CertificateType = cert.CertificateType,
                Purpose = cert.Purpose,
                ControlNumber = cert.ControlNumber,
                Status = cert.Status,
                FeePaid = cert.FeePaid,
                DateRequested = cert.DateRequested,
                DateIssued = cert.DateIssued ?? DateTime.Now,
                IssuedBy = string.IsNullOrEmpty(cert.IssuedBy) ? "Barangay Staff" : cert.IssuedBy
            };

            return View("PrintTemplate", model);
        }

        private async Task PopulateResidentsDropDownList(object? selectedResident = null)
        {
            var residentsQuery = await _context.Residents
                .Where(r => r.IsResident)
                .OrderBy(r => r.LastName)
                .Select(r => new { Id = r.ResidentId, FullName = r.LastName + ", " + r.FirstName })
                .ToListAsync();

            ViewBag.Residents = new SelectList(residentsQuery, "Id", "FullName", selectedResident);
        }
    }
}