﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Outlook.Models.Core.Models;
using Outlook.Models.Data;
using Outlook.Models.Services;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Outlook.Server.Controllers
{
    [Authorize(Roles = "Web-Editor, Editor-In-Chief, Admin", AuthenticationSchemes = "Identity.Application")]
    public class CategoriesController : Controller
    {
        private readonly OutlookContext context;
        private readonly Logger.Logger logger;

        public CategoriesController(
            OutlookContext context)
        {
            this.context = context;
            logger = Logger.Logger.Instance(Logger.Logger.LogField.server);
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = await context.Category
                .Include(c => c.Editors)
                .ToListAsync();

            return View(await context.Category.ToListAsync());
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await context.Category
                .Include(c => c.Editors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Tag")] Category category)
        {
            if (ModelState.IsValid)
            {
                context.Add(category);
                category.SetLanguage(Regex.IsMatch(category.Name, @"^[a-zA-Z.\-+\s]*$") ? OutlookConstants.Language.English : OutlookConstants.Language.Arabic);
                await context.SaveChangesAsync();

                logger.Log($"{HttpContext.User.Identity.Name} created Category `{category.Name}` and ID `{category.Id}`.");

                return RedirectToAction(nameof(Index));
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await context.Category
                .FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Category category)
        {
            if (id != category.Id)
            {
                return NotFound();
            }

            ModelState.Remove("Name");
            if (ModelState.IsValid)
            {
                try
                {
                    var originalCategory = await context.Category
                        .FindAsync(category.Id);

                    originalCategory
                        .SetLanguage(category.Language)
                        .SetTag(category.Tag);

                    await context.SaveChangesAsync();

                    logger.Log($"{HttpContext.User.Identity.Name} editted Category `{category.Name}`");
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.Id))
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
            return View(category);
        }

        // GET: Categories/Delete/5
        [Authorize(Roles = "Editor-In-Chief, Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await context.Category
                .Include(c => c.Editors)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // POST: Categories/Delete/5
        [Authorize(Roles = "Editor-In-Chief, Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var category = await context.Category.FindAsync(id);

                logger.Log($"{HttpContext.User.Identity.Name} attempts to delete Category `{category.Name}`");

                context.Category.Remove(category);
                await context.SaveChangesAsync();

                logger.Log($"Delete Completed.");

                return RedirectToAction(nameof(Index));
            }
            catch (DbUpdateException)
            {
                logger.Log($"Delete Failed, because of DbUpdateException.");

                var juniorEditors = context.Member
                    .Where(m => m.Category.Id == id)
                    .Select(e => e.Name);

                var errorMessage = "You cannot delete the following category before deleting its junior editors: ";
                var errorDetail = new StringBuilder();
                foreach (var editor in juniorEditors)
                {
                    errorDetail.Append($"{editor} --- ");
                }

                return RedirectToAction("ServerError", "", new { message = errorMessage.ToString(), detail = errorDetail });
            }
        }

        private bool CategoryExists(int id)
        {
            return context.Category.Any(e => e.Id == id);
        }
    }
}
