using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Staff.ViewModels;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Web.Areas.Staff.Controllers
{
    [Area("Staff")]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AnnouncementsController(ApplicationDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: /Staff/Announcements/Index
        public IActionResult Index()
        {
            var list = _context.Announcements
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.AnnouncementId,
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    DatePosted = a.PublishDate,
                    Category = string.IsNullOrEmpty(a.Category) ? "General" : a.Category,
                    AuthorName = string.IsNullOrEmpty(a.AuthorName) ? "Staff" : a.AuthorName,
                    PublishDate = a.PublishDate,
                    ExpiryDate = a.ExpiryDate,
                    IsPinned = a.IsPinned,
                    ImageUrl = a.ImageUrl
                })
                .OrderByDescending(a => a.IsPinned)
                .ThenByDescending(a => a.PublishDate)
                .ToList();

            return View(list);
        }

        // GET: /Staff/Announcements/Details/5
        public IActionResult Details(int id)
        {
            var item = _context.Announcements
                .Where(a => a.AnnouncementId == id)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.AnnouncementId,
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    DatePosted = a.PublishDate,
                    Category = a.Category,
                    AuthorName = a.AuthorName,
                    PublishDate = a.PublishDate,
                    ExpiryDate = a.ExpiryDate,
                    IsPinned = a.IsPinned,
                    ImageUrl = a.ImageUrl
                })
                .FirstOrDefault();

            if (item == null) return NotFound();
            return View(item);
        }

        // GET: /Staff/Announcements/Create
        public IActionResult Create()
        {
            return View(new AnnouncementViewModel());
        }

        // 🔴 AJAX UPLOAD ENDPOINT FOR FEATURED IMAGE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return Json(new { success = false, error = "Walang napiling file." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return Json(new { success = false, error = "PNG, JPG, o WEBP lamang ang pinapayagan." });

            if (file.Length > 5 * 1024 * 1024)
                return Json(new { success = false, error = "Ang larawan ay dapat na hindi hihigit sa 5 MB." });

            try
            {
                string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "announcements");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                string uniqueFileName = $"{Guid.NewGuid()}{extension}";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Generates relative path for public serving
                string relativeUrl = $"/uploads/announcements/{uniqueFileName}";
                return Json(new { success = true, url = relativeUrl });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = "Nagkaroon ng problema sa pag-save: " + ex.Message });
            }
        }

        // POST: /Staff/Announcements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(AnnouncementViewModel model)
        {
            if (ModelState.IsValid)
            {
                var newAnnouncement = new Announcement
                {
                    Title = model.Title,
                    Content = model.Content,
                    Category = model.Category ?? "General",
                    IsPinned = model.IsPinned,
                    PublishDate = model.DatePosted != default ? model.DatePosted : DateTime.Now,
                    ExpiryDate = model.ExpiryDate,
                    AuthorName = User.Identity?.Name ?? "Staff Duty",
                    ImageUrl = model.ImageUrl ?? string.Empty // 🔑 INAYOS: Isina-save na ang ImageUrl mula sa form
                };

                _context.Announcements.Add(newAnnouncement);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Staff/Announcements/Edit/5
        public IActionResult Edit(int id)
        {
            var item = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
            if (item == null) return NotFound();

            var viewModel = new AnnouncementViewModel
            {
                Id = item.AnnouncementId,
                AnnouncementId = item.AnnouncementId,
                Title = item.Title,
                Content = item.Content,
                DatePosted = item.PublishDate,
                Category = item.Category,
                ExpiryDate = item.ExpiryDate,
                IsPinned = item.IsPinned,
                AuthorName = item.AuthorName,
                ImageUrl = item.ImageUrl
            };

            return View(viewModel);
        }

        // POST: /Staff/Announcements/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, AnnouncementViewModel model)
        {
            if (id == 0) id = model.AnnouncementId != 0 ? model.AnnouncementId : model.Id;

            if (ModelState.IsValid)
            {
                var existing = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
                if (existing == null) return NotFound();

                existing.Title = model.Title;
                existing.Content = model.Content;
                existing.Category = model.Category ?? "General";
                existing.ExpiryDate = model.ExpiryDate;
                existing.IsPinned = model.IsPinned;
                existing.ImageUrl = model.ImageUrl ?? existing.ImageUrl;
                existing.AuthorName = User.Identity?.Name ?? existing.AuthorName;

                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: /Staff/Announcements/Delete/5
        public IActionResult Delete(int id)
        {
            var item = _context.Announcements
                .Where(a => a.AnnouncementId == id)
                .Select(a => new AnnouncementViewModel
                {
                    Id = a.AnnouncementId,
                    AnnouncementId = a.AnnouncementId,
                    Title = a.Title,
                    Content = a.Content,
                    PublishDate = a.PublishDate
                })
                .FirstOrDefault();

            if (item == null) return NotFound();
            return View(item);
        }

        // POST: /Staff/Announcements/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var item = _context.Announcements.FirstOrDefault(a => a.AnnouncementId == id);
            if (item != null)
            {
                _context.Announcements.Remove(item);
                _context.SaveChanges();
            }
            return RedirectToAction(nameof(Index));
        }
    }
}