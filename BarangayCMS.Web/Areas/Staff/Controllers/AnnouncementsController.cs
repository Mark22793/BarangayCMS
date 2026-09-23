using System;
using System.Collections.Generic;
using System.Linq;
using BarangayCMS.DAL.Context;
using BarangayCMS.Entities;
using BarangayCMS.Web.Areas.Staff.ViewModels; // 🔑 INAYOS: Dinagdagan ng .Web
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BarangayCMS.Web.Areas.Staff.Controllers // 🔑 INAYOS: Dinagdagan ng .Web
{
    [Area("Staff")]
    public class AnnouncementsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AnnouncementsController(ApplicationDbContext context)
        {
            _context = context;
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
                    IsPinned = a.IsPinned
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
                    IsPinned = a.IsPinned
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
                    PublishDate = DateTime.Now,
                    ExpiryDate = model.ExpiryDate,
                    AuthorName = User.Identity?.Name ?? "Staff Duty",
                    ImageUrl = string.Empty
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
                AuthorName = item.AuthorName
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