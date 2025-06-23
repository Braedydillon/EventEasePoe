using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EventEasePoe.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Humanizer.Localisation;
using EventEasePoe.Data;
using EventEasePoe.Models;

namespace EventEasePart2Sub.Controllers
{
    public class EventsController : Controller
    {
        private readonly EventEasePoeContext _context;
        private readonly BlobService _blobService;

        public EventsController(EventEasePoeContext context, BlobService blobService)
        {
            _context = context;
            _blobService = blobService;
        }

        // GET: Events
        public async Task<IActionResult> Index(string searchString)
        {
            if (_context.Event == null)
            {
                return Problem("Entity set 'EventEasePractice1Context.Event' is null.");
            }

            var events = from m in _context.Event
                         select m;

            if (!string.IsNullOrEmpty(searchString))
            {
                events = events.Where(s => s.EventName!.ToUpper().Contains(searchString.ToUpper()));
            }

            return View(await events.ToListAsync());
        }

        // GET: Events/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // GET: Events/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Events/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("EventID,EventName,Description,EventType,EventDate,StartTime,EndTime")] Event @event, IFormFile? imageFile)
        {

            // Check if an image is uploaded
            if (imageFile != null && imageFile.Length > 0)
            {
                // Upload the image to blob storage and assign the URL
                @event.EventImage = await _blobService.UploadFileAsync(imageFile, "eventimages");
            }

            // If model is valid, save the event
            if (ModelState.IsValid)
            {
                _context.Add(@event);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(@event);
        }

        // GET: Events/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event.FindAsync(id);
            if (@event == null)
            {
                return NotFound();
            }
            return View(@event);
        }

        // POST: Events/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("EventID,EventName,Description,EventType,EventDate,StartTime,EndTime,ImageURL")] Event @event, IFormFile? imageFile)
        {
            if (id != @event.EventID)
            {
                return NotFound();
            }

            var newStart = @event.StartTime.TimeOfDay;
            var newEnd = @event.EndTime.TimeOfDay;

            // 15-minute increment validation
            bool IsValid15MinuteIncrement(TimeSpan time) => time.Minutes % 15 == 0 && time.Seconds == 0;

            if (!IsValid15MinuteIncrement(newStart) || !IsValid15MinuteIncrement(newEnd))
            {
                ModelState.AddModelError("", "Start and End times must be in 15-minute intervals.");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingEvent = await _context.Event.AsNoTracking().FirstOrDefaultAsync(e => e.EventID == id);
                    if (existingEvent == null)
                    {
                        return NotFound();
                    }

                    if (imageFile != null && imageFile.Length > 0)
                    {
                        // Delete old image
                        if (!string.IsNullOrEmpty(existingEvent.EventImage))
                        {
                            await _blobService.DeleteFileAsync(existingEvent.EventImage, "eventimages");
                        }

                        // Upload new image
                        @event.EventImage = await _blobService.UploadFileAsync(imageFile, "eventimages");
                    }
                    else
                    {
                        @event.EventImage = existingEvent.EventImage;
                    }

                    _context.Update(@event);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EventExists(@event.EventID))
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

            return View(@event);
        }

        // GET: Events/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var @event = await _context.Event
                .FirstOrDefaultAsync(m => m.EventID == id);
            if (@event == null)
            {
                return NotFound();
            }

            return View(@event);
        }

        // POST: Events/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            bool exists = await _context.Booking.AnyAsync(booking => booking.EventID == id);
            if (exists)
            {
                var eventToDelete = await _context.Event.FindAsync(id);
                ModelState.AddModelError("", "Cannot delete this event; there are existing records in booking.");
                return View(eventToDelete);
            }

            var @event = await _context.Event.FindAsync(id);
            if (@event != null)
            {
                if (!string.IsNullOrEmpty(@event.EventImage))
                {
                    await _blobService.DeleteFileAsync(@event.EventImage, "eventimages");
                }

                _context.Event.Remove(@event);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
        private bool EventExists(int id)
        {
            return _context.Event.Any(e => e.EventID == id);
        }
    }
}
