using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using EventEasePoe.Data;
using EventEasePoe.Models;
using EventEasePoe.Models;

namespace EventEasePart2Sub.Controllers
{
    public class VenuesController : Controller
    {
        private readonly EventEasePoeContext _context;
        private readonly BlobService _blobService;

        public VenuesController(EventEasePoeContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Venues
        public async Task<IActionResult> Index(string searchString)
        {
            if (_context.Event == null)
            {
                return Problem("Entity set 'EventEasePractice1Context.Venue'  is null.");
            }

            var Ven = from m in _context.Venue
                      select m;

            if (!String.IsNullOrEmpty(searchString))
            {
                Ven = Ven.Where(s => s.VenueName!.ToUpper().Contains(searchString.ToUpper()));
            }

            return View(await Ven.ToListAsync());
        }

        // GET: Venues/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        // GET: Venues/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Venues/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("VenueID,VenueName,Location,VenueCapcity")] Venue venue, IFormFile imageFile)
        {
            if (ModelState.IsValid)
            {
                if (imageFile != null && imageFile.Length > 0)
                {
                    venue.ImageURL = await _blobService.UploadFileAsync(imageFile, "venueimages");
                }

                _context.Add(venue);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(venue);
        }


        // GET: Venues/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue.FindAsync(id);
            if (venue == null)
            {
                return NotFound();
            }
            return View(venue);
        }

        // POST: Venues/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("VenueID,VenueName,Location,VenueCapcity,ImageURL")] Venue venue, IFormFile imageFile)
        {
            if (id != venue.VenueID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingVenue = await _context.Venue.AsNoTracking().FirstOrDefaultAsync(v => v.VenueID == id);
                    if (existingVenue == null)
                    {
                        return NotFound();
                    }

                    // If a new image is uploaded, replace the old one
                    if (imageFile != null && imageFile.Length > 0)
                    {
                        if (!string.IsNullOrEmpty(existingVenue.ImageURL))
                        {
                            await _blobService.DeleteFileAsync(existingVenue.ImageURL, "venueimages");
                        }

                        venue.ImageURL = await _blobService.UploadFileAsync(imageFile, "venueimages");
                    }
                    else
                    {
                        // Keep the existing image if no new one was uploaded
                        venue.ImageURL = existingVenue.ImageURL;
                    }

                    _context.Update(venue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!VenueExists(venue.VenueID))
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
            return View(venue);
        }

        // GET: Venues/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var venue = await _context.Venue
                .FirstOrDefaultAsync(m => m.VenueID == id);
            if (venue == null)
            {
                return NotFound();
            }

            return View(venue);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool exists = await _context.Booking.AnyAsync(booking => booking.VenueID == id);
            if (exists)
            {
                var venue = await _context.Venue.FindAsync(id);
                ModelState.AddModelError("", "Cannot delete this venue; there are existing records in booking.");
                return View(venue);
            }

            var venueToDelete = await _context.Venue.FindAsync(id);
            if (venueToDelete != null)
            {
                if (!string.IsNullOrEmpty(venueToDelete.ImageURL))
                {
                    await _blobService.DeleteFileAsync(venueToDelete.ImageURL, "venueimages");
                }

                _context.Venue.Remove(venueToDelete);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private bool VenueExists(int id)
        {
            return _context.Venue.Any(e => e.VenueID == id);
        }
    }
}
