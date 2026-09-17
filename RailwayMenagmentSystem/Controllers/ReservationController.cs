using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayMenagmentSystem.Data;
using RailwayMenagmentSystem.Models;
using RailwayMenagmentSystem.Models.Enums;

namespace RailwayMenagmentSystem.Controllers
{
    [Authorize]
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ReservationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Reservation
        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Challenge();
            }

            var query = _context.Reservations
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Train)
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Route)
                .ThenInclude(r => r.DepartureStation)
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Route)
                .ThenInclude(r => r.ArrivalStation)
                .Include(r => r.User);

            List<Reservation> reservations;

            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                reservations = await query
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();

                ViewData["Title"] = "All Reservations";
            }
            else
            {
                reservations = await query
                    .Where(r => r.UserId == userId)
                    .OrderByDescending(r => r.ReservationDate)
                    .ToListAsync();

                ViewData["Title"] = "My Reservations";
            }
            
            return View(reservations);
        }

        // GET: Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = _context.Reservations
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Train)
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Route)
                .ThenInclude(r => r.DepartureStation)
                .Include(r => r.Schedule)
                .ThenInclude(s => s.Route)
                .ThenInclude(r => r.ArrivalStation);

            Reservation? reservation;

            if (User.IsInRole("Admin") || User.IsInRole("Employee"))
            {
                reservation = await query
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            else
            {
                reservation = await query
                    .FirstOrDefaultAsync(r =>
                        r.Id == id &&
                        r.UserId == userId);
            }

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // GET: Reservation/Create?scheduleId=5
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create(int scheduleId)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Train)
                .Include(s => s.Route)
                .ThenInclude(r => r.DepartureStation)
                .Include(s => s.Route)
                .ThenInclude(r => r.ArrivalStation)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
            {
                return NotFound();
            }

            if (schedule.Train == null ||
                schedule.Train.Status == TrainStatus.Broken)
            {
                TempData["ErrorMessage"] =
                    "This train is currently unavailable.";

                return RedirectToAction(
                    "Details",
                    "Schedule",
                    new { id = scheduleId });
            }

            if (schedule.AvailableSeats <= 0)
            {
                TempData["ErrorMessage"] =
                    "There are no available seats for this schedule.";

                return RedirectToAction(
                    "Details",
                    "Schedule",
                    new { id = scheduleId });
            }

            ViewData["Schedule"] = schedule;

            return View(new Reservation
            {
                ScheduleId = scheduleId
            });
        }

        // POST: Reservation/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Create(
            int scheduleId,
            [Bind("NumberOfSeats")] Reservation reservation)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (userId == null)
            {
                return Challenge();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Train)
                .Include(s => s.Route)
                .ThenInclude(r => r.DepartureStation)
                .Include(s => s.Route)
                .ThenInclude(r => r.ArrivalStation)
                .FirstOrDefaultAsync(s => s.Id == scheduleId);

            if (schedule == null)
            {
                return NotFound();
            }

            if (schedule.Train == null ||
                schedule.Train.Status == TrainStatus.Broken)
            {
                ModelState.AddModelError(
                    "",
                    "This train is currently unavailable.");
            }

            if (reservation.NumberOfSeats > schedule.AvailableSeats)
            {
                ModelState.AddModelError(
                    "NumberOfSeats",
                    $"Only {schedule.AvailableSeats} seats are available.");
            }

            if (ModelState.IsValid)
            {
                reservation.ScheduleId = scheduleId;
                reservation.UserId = userId;
                reservation.ReservationDate = DateTime.Now;
                reservation.Status = ReservationStatus.Active;

                schedule.AvailableSeats -= reservation.NumberOfSeats;

                _context.Reservations.Add(reservation);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["Schedule"] = schedule;

            return View(reservation);
        }

        // POST: Reservation/Cancel/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservation = await _context.Reservations
                .Include(r => r.Schedule)
                .FirstOrDefaultAsync(r =>
                    r.Id == id &&
                    r.UserId == userId);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status == ReservationStatus.Active)
            {
                reservation.Status = ReservationStatus.Cancelled;

                reservation.Schedule.AvailableSeats +=
                    reservation.NumberOfSeats;

                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Reservation/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> Delete(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            Reservation? reservation;

            if (User.IsInRole("Admin"))
            {
                reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r => r.Id == id);
            }
            else
            {
                reservation = await _context.Reservations
                    .FirstOrDefaultAsync(r =>
                        r.Id == id &&
                        r.UserId == userId);
            }

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != ReservationStatus.Cancelled)
            {
                return BadRequest(
                    "Only cancelled reservations can be deleted.");
            }

            _context.Reservations.Remove(reservation);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        // POST: Reservation/DeleteAllCancelled
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteAllCancelled()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            List<Reservation> reservations;

            if (User.IsInRole("Admin"))
            {
                reservations = await _context.Reservations
                    .Where(r =>
                        r.Status == ReservationStatus.Cancelled)
                    .ToListAsync();
            }
            else
            {
                reservations = await _context.Reservations
                    .Where(r =>
                        r.UserId == userId &&
                        r.Status == ReservationStatus.Cancelled)
                    .ToListAsync();
            }

            if (reservations.Any())
            {
                _context.Reservations.RemoveRange(reservations);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
