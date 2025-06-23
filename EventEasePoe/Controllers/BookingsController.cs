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
    public class BookingsController : Controller
    {

        private readonly EventEasePoeContext _context;

        public BookingsController(EventEasePoeContext context)
        {
            _context = context;
        }

        // GET: Bookings 
        public async Task<IActionResult> Index(string searchString)
        {
            if (_context.Booking == null)
            {
                return Problem("Entity set 'EventEasePractice1Context.Booking' is null.");
            }

            var book = _context.Booking
                          .Include(b => b.Event)
                          .Include(b => b.Venue)
                          .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                // First, try to parse the search string into an integer for BookingID matching
                bool isNumeric = int.TryParse(searchString, out int bookingId);

                book = book.Where(s =>
                    (s.Event != null &&
                     s.Event.EventName != null &&
                     s.Event.EventName.ToUpper().Contains(searchString.ToUpper()))
                     || (isNumeric && s.BookingID == bookingId)
                );
            }


            return View(await book.ToListAsync());
        }

        // GET: Bookings/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // GET: Bookings/Create
        public IActionResult Create()
        {
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName");
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName");
            return View();
        }

        // POST: Bookings/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("BookingID,VenueID,EventID,EventBooking")] Booking booking)
        {
            var selectedEvent = await _context.Event.FirstOrDefaultAsync(e => e.EventID == booking.EventID);
            if (selectedEvent == null)
            {
                ModelState.AddModelError("", "Selected event not found.");
            }

            // Validation 1: Check for duplicate event + venue booking
            var duplicateBooking = await _context.Booking
                .AnyAsync(b => b.EventID == booking.EventID && b.VenueID == booking.VenueID);

            if (duplicateBooking)
            {
                ModelState.AddModelError("", "This event has already been booked at the selected venue.");
            }

            // Validation 2: Overlapping event time on same venue and date
            var overlappingBooking = await _context.Booking
                .Include(b => b.Event)
                .Where(b =>
                    b.VenueID == booking.VenueID &&
                    b.EventBooking == booking.EventBooking &&
                    b.EventID != booking.EventID &&
                    b.Event != null &&
                    selectedEvent != null &&
                    b.Event.StartTime < selectedEvent.EndTime &&
                    b.Event.EndTime > selectedEvent.StartTime)
                .AnyAsync();

            if (overlappingBooking)
            {
                ModelState.AddModelError("", "The venue is already booked for another event that overlaps in time.");
            }

            if (ModelState.IsValid)
            {
                _context.Add(booking);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            // Repopulate dropdowns on failure
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            return View(booking);
        }


        // GET: Bookings/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking.FindAsync(id);
            if (booking == null)
            {
                return NotFound();
            }
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }

        // POST: Bookings/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("BookingID,EventID,VenueID,EventBooking")] Booking booking)
        {
            if (id != booking.BookingID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(booking);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!BookingExists(booking.BookingID))
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
            ViewData["EventID"] = new SelectList(_context.Event, "EventID", "EventName", booking.EventID);
            ViewData["VenueID"] = new SelectList(_context.Venue, "VenueID", "VenueName", booking.VenueID);
            return View(booking);
        }

        // GET: Bookings/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        // POST: Bookings/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var booking = await _context.Booking.FindAsync(id);
            if (booking != null)
            {
                _context.Booking.Remove(booking);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool BookingExists(int id)
        {
            return _context.Booking.Any(e => e.BookingID == id);
        }



        // GET: Show more details
        public async Task<IActionResult> BookingDetails(int id)
        {

            if (id == null)
            {
                return NotFound();
            }

            var booking = await _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .FirstOrDefaultAsync(m => m.BookingID == id);
            if (booking == null)
            {
                return NotFound();
            }

            return View(booking);
        }

        public async Task<IActionResult> AdvancedSearch(string searchString, int? venueId, DateTime? startDate, DateTime? endDate)
        {
            if (_context.Booking == null)
            {
                return Problem("Entity set 'Booking' is null.");
            }

            var bookings = _context.Booking
                .Include(b => b.Event)
                .Include(b => b.Venue)
                .AsQueryable();

            if (!string.IsNullOrEmpty(searchString))
            {
                bool isNumeric = int.TryParse(searchString, out int bookingId);
                bookings = bookings.Where(b =>
                    (b.Event != null && b.Event.EventType.ToUpper().Contains(searchString.ToUpper())) ||
                    (isNumeric && b.BookingID == bookingId)
                );
            }

            if (venueId.HasValue)
            {
                bookings = bookings.Where(b => b.VenueID == venueId.Value);
            }

            if (startDate.HasValue && endDate.HasValue)
            {
                bookings = bookings.Where(b => b.Event.EventDate >= startDate.Value && b.Event.EventDate <= endDate.Value);
            }

            // Pass venues as SelectList for the dropdown in the view
            ViewData["Venues"] = new SelectList(await _context.Venue.ToListAsync(), "VenueID", "VenueName");

            return View(await bookings.ToListAsync());
        }

    }
}
