using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BarangayCMS.BLL.Interfaces;
using BarangayCMS.DAL.Context;
using BarangayCMS.DTO;
using BarangayCMS.Entities;

namespace BarangayManagementSystem.Controllers.Admin
{
    [Area("Admin")]
    public class AdminCertificateTypesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ICertificateRequirementService _requirementService;

        public AdminCertificateTypesController(
            ApplicationDbContext context,
            ICertificateRequirementService requirementService)
        {
            _context = context;
            _requirementService = requirementService;
        }

        // ==========================================
        // 1. LISTAHAN (INDEX - GET)
        // ==========================================
        public async Task<IActionResult> Index()
        {
            var certificateTypes = await _context.CertificateTypes.ToListAsync();
            return View(certificateTypes);
        }

        // ==========================================
        // 2. PAG-ADD NG BAGO (CREATE - GET)
        // ==========================================
        public IActionResult Create()
        {
            return View();
        }

        // ==========================================
        // 3. PAG-SAVE NG BAGO (CREATE - POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("CertificateTypeId,CertificateName,Price")] CertificateType certificateType,
            IFormFile? TemplateFile)
        {
            ModelState.Clear();

            if (certificateType.CertificateName != null)
            {
                if (TemplateFile != null && TemplateFile.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");
                    if (!Directory.Exists(uploadsFolder))
                    {
                        Directory.CreateDirectory(uploadsFolder);
                    }

                    var fileExtension = Path.GetExtension(TemplateFile.FileName);
                    var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await TemplateFile.CopyToAsync(fileStream);
                    }

                    certificateType.TemplateFileName = uniqueFileName;
                }

                _context.Add(certificateType);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(certificateType);
        }

        // ==========================================
        // 4. PAG-EDIT (EDIT - GET)
        // ==========================================
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificateType = await _context.CertificateTypes.FindAsync(id);
            if (certificateType == null)
            {
                return NotFound();
            }
            return View(certificateType);
        }

        // ==========================================
        // 5. PAG-SAVE NG BINAGO (EDIT - POST)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("CertificateTypeId,CertificateName,Price,TemplateFileName")] CertificateType certificateType,
            IFormFile? TemplateFile)
        {
            if (id == 0) id = certificateType.CertificateTypeId;

            if (id != certificateType.CertificateTypeId)
            {
                return NotFound();
            }

            ModelState.Clear();

            if (certificateType.CertificateName != null)
            {
                try
                {
                    if (TemplateFile != null && TemplateFile.Length > 0)
                    {
                        var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "templates");
                        if (!Directory.Exists(uploadsFolder))
                        {
                            Directory.CreateDirectory(uploadsFolder);
                        }

                        // A. Burahin ang lumang file nang may panangga sa lock at permission errors
                        if (!string.IsNullOrEmpty(certificateType.TemplateFileName))
                        {
                            var oldFilePath = Path.Combine(uploadsFolder, certificateType.TemplateFileName);

                            if (System.IO.File.Exists(oldFilePath))
                            {
                                try
                                {
                                    // Alisin muna ang Read-Only attribute sa file kung nakakabit
                                    System.IO.File.SetAttributes(oldFilePath, FileAttributes.Normal);

                                    // Subukang i-delete ang lumang file
                                    System.IO.File.Delete(oldFilePath);
                                }
                                catch (IOException ex)
                                {
                                    // Kapag naka-lock sa MS Word o ibang app, i-log lang at ituloy ang upload
                                    System.Diagnostics.Debug.WriteLine($"Hindi ma-delete ang lumang file dahil ginagamit pa: {ex.Message}");
                                }
                                catch (UnauthorizedAccessException ex)
                                {
                                    // Kapag may permission restriction
                                    System.Diagnostics.Debug.WriteLine($"Walang permission para idelete ang file: {ex.Message}");
                                }
                            }
                        }

                        // B. I-save ang bagong file
                        var fileExtension = Path.GetExtension(TemplateFile.FileName);
                        var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                        using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await TemplateFile.CopyToAsync(fileStream);
                        }

                        // C. I-update ang file name sa DB
                        certificateType.TemplateFileName = uniqueFileName;
                    }

                    _context.Update(certificateType);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CertificateTypeExists(certificateType.CertificateTypeId))
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
            return View(certificateType);
        }

        // ==========================================
        // 6. PAG-BURA (DELETE - GET / PROMPT PAGE)
        // ==========================================
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var certificateType = await _context.CertificateTypes
                .FirstOrDefaultAsync(m => m.CertificateTypeId == id);
            if (certificateType == null)
            {
                return NotFound();
            }

            return View(certificateType);
        }

        // ==========================================
        // 7. PAG-CONFIRM NG PAGBURA (DELETE - POST)
        // ==========================================
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var certificateType = await _context.CertificateTypes.FindAsync(id);
            if (certificateType != null)
            {
                _context.CertificateTypes.Remove(certificateType);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // REQUIREMENTS MANAGEMENT (per certificate type)
        // ==========================================

        // GET: /Admin/AdminCertificateTypes/Requirements/5
        public async Task<IActionResult> Requirements(int? id)
        {
            if (id == null) return NotFound();

            var certType = await _context.CertificateTypes.FindAsync(id);
            if (certType == null) return NotFound();

            ViewBag.CertificateType = certType;
            var requirements = await _requirementService.GetAllByCertificateTypeIdAsync(id.Value);
            return View(requirements);
        }

        // POST: /Admin/AdminCertificateTypes/AddRequirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRequirement(CertificateRequirementDTO model)
        {
            if (string.IsNullOrWhiteSpace(model.RequirementName))
            {
                TempData["Error"] = "Requirement name is required.";
            }
            else
            {
                bool ok = await _requirementService.AddAsync(model);
                TempData[ok ? "Success" : "Error"] = ok
                    ? "Requirement added."
                    : "That requirement already exists for this certificate.";
            }

            return RedirectToAction(nameof(Requirements), new { id = model.CertificateTypeId });
        }

        // POST: /Admin/AdminCertificateTypes/EditRequirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRequirement(CertificateRequirementDTO model)
        {
            bool ok = await _requirementService.UpdateAsync(model);
            TempData[ok ? "Success" : "Error"] = ok
                ? "Requirement updated."
                : "Unable to update requirement (name may be blank or duplicate).";

            return RedirectToAction(nameof(Requirements), new { id = model.CertificateTypeId });
        }

        // POST: /Admin/AdminCertificateTypes/DeleteRequirement
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRequirement(int id, int certificateTypeId)
        {
            bool ok = await _requirementService.DeleteAsync(id);
            TempData[ok ? "Success" : "Error"] = ok ? "Requirement removed." : "Requirement not found.";
            return RedirectToAction(nameof(Requirements), new { id = certificateTypeId });
        }

        // ==========================================
        // PRIVATE HELPER METHOD
        // ==========================================
        private bool CertificateTypeExists(int id)
        {
            return _context.CertificateTypes.Any(e => e.CertificateTypeId == id);
        }
    }
}