using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RailwayMenagmentSystem.Data;
using RailwayMenagmentSystem.Models;
using RailwayMenagmentSystem.Models.Enums;

namespace RailwayMenagmentSystem.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly ApplicationDbContext _context;


        public ScheduleController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Schedule
        public async Task<IActionResult> Index(int? fromStationId, int? toStationId, DateTime? date)
        {
            var schedules = await _context.Schedules
                .Where(s =>
                    (fromStationId == null || s.Route.DepartureStationId == fromStationId) &&
                    (toStationId == null || s.Route.ArrivalStationId == toStationId) &&
                    (date == null || (s.DepartureTime.HasValue && s.DepartureTime.Value.Date >= date.Value.Date))
                )
                .Include(s => s.Route)
                .ThenInclude(r => r.DepartureStation)
                .Include(s => s.Route)
                .ThenInclude(r => r.ArrivalStation)
                .Include(s => s.Train)
                .ToListAsync();

            ViewData["CurrentFromStation"] = fromStationId;
            ViewData["CurrentToStation"] = toStationId;
            ViewData["CurrentDate"] = date?.ToString("yyyy-MM-dd");

            ViewData["FromStations"] = new SelectList(
                _context.Stations,
                "Id",
                "Name",
                fromStationId
            );

            ViewData["ToStations"] = new SelectList(
                _context.Stations,
                "Id",
                "Name",
                toStationId
            );

            return View(schedules);
        }

        // GET: Schedule/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Train)
                .Include(s => s.Route)
                .ThenInclude(r => r.DepartureStation)
                .Include(s => s.Route)
                .ThenInclude(r => r.ArrivalStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // GET: Schedule/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t => t.Status != TrainStatus.Broken),
                "Id",
                "Name"
            );

            ViewData["RouteId"] = new SelectList(
                _context.Routes,
                "Id",
                "Name"
            );

            return View();
        }

        // GET: Schedule/CreateScheduleFromRoute/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateScheduleFromRoute(int routeId)
        {
            var route = await _context.Routes
                .Include(r => r.DepartureStation)
                .Include(r => r.ArrivalStation)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
            {
                return NotFound();
            }

            ViewData["Route"] = route;

            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t => t.Status != TrainStatus.Broken),
                "Id",
                "Name"
            );

            var schedule = new Schedule
            {
                RouteId = routeId
            };

            return View(schedule);
        }

        // POST: Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("Id,TrainId,RouteId,DepartureTime,ArrivalTime,Price,AvailableSeats")]
            Schedule schedule)
        {
            var train = await _context.Trains.FindAsync(schedule.TrainId);

            if (train == null)
            {
                return NotFound();
            }

            if (train.Status == TrainStatus.Broken)
            {
                ModelState.AddModelError(
                    "TrainId",
                    "A broken train cannot be scheduled."
                );
            }

            if (!schedule.DepartureTime.HasValue || !schedule.ArrivalTime.HasValue)
            {
                ModelState.AddModelError(
                    "",
                    "Departure and arrival time are required."
                );
            }
            else if (schedule.ArrivalTime <= schedule.DepartureTime)
            {
                ModelState.AddModelError(
                    "ArrivalTime",
                    "Arrival time must be after departure time."
                );
            }

            var hasConflict = await _context.Schedules.AnyAsync(s =>
                s.TrainId == schedule.TrainId &&
                s.DepartureTime.HasValue &&
                s.ArrivalTime.HasValue &&
                schedule.DepartureTime.HasValue &&
                schedule.ArrivalTime.HasValue &&
                schedule.DepartureTime.Value < s.ArrivalTime.Value &&
                schedule.ArrivalTime.Value > s.DepartureTime.Value
            );

            if (hasConflict)
            {
                ModelState.AddModelError(
                    "TrainId",
                    "This train already has a schedule during this time period."
                );
            }

            if (ModelState.IsValid)
            {
                schedule.AvailableSeats = train.Capacity;

                _context.Add(schedule);

                train.Status = TrainStatus.Scheduled;

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t => t.Status != TrainStatus.Broken),
                "Id",
                "Name",
                schedule.TrainId
            );

            ViewData["RouteId"] = new SelectList(
                _context.Routes,
                "Id",
                "Name",
                schedule.RouteId
            );

            return View(schedule);
        }

        // GET: Schedule/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules.FindAsync(id);

            if (schedule == null)
            {
                return NotFound();
            }

            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t =>
                    t.Status != TrainStatus.Broken || t.Id == schedule.TrainId),
                "Id",
                "Name",
                schedule.TrainId
            );

            ViewData["RouteId"] = new SelectList(
                _context.Routes,
                "Id",
                "Name",
                schedule.RouteId
            );

            return View(schedule);
        }

        // POST: Schedule/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,TrainId,RouteId,DepartureTime,ArrivalTime,Price,AvailableSeats")]
            Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return NotFound();
            }

            var existingSchedule = await _context.Schedules
                .Include(s => s.Train)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (existingSchedule == null)
            {
                return NotFound();
            }

            var newTrain = await _context.Trains.FindAsync(schedule.TrainId);

            if (newTrain == null)
            {
                return NotFound();
            }

            if (newTrain.Status == TrainStatus.Broken)
            {
                ModelState.AddModelError(
                    "TrainId",
                    "A broken train cannot be scheduled."
                );
            }

            if (!schedule.DepartureTime.HasValue || !schedule.ArrivalTime.HasValue)
            {
                ModelState.AddModelError(
                    "",
                    "Departure and arrival time are required."
                );
            }
            else if (schedule.ArrivalTime <= schedule.DepartureTime)
            {
                ModelState.AddModelError(
                    "ArrivalTime",
                    "Arrival time must be after departure time."
                );
            }

            var hasConflict = await _context.Schedules.AnyAsync(s =>
                s.Id != id &&
                s.TrainId == schedule.TrainId &&
                s.DepartureTime.HasValue &&
                s.ArrivalTime.HasValue &&
                schedule.DepartureTime.HasValue &&
                schedule.ArrivalTime.HasValue &&
                schedule.DepartureTime.Value < s.ArrivalTime.Value &&
                schedule.ArrivalTime.Value > s.DepartureTime.Value
            );

            if (hasConflict)
            {
                ModelState.AddModelError(
                    "TrainId",
                    "This train already has a schedule during this time period."
                );
            }

            if (ModelState.IsValid)
            {
                var oldTrainId = existingSchedule.TrainId;

                existingSchedule.TrainId = schedule.TrainId;
                existingSchedule.RouteId = schedule.RouteId;
                existingSchedule.DepartureTime = schedule.DepartureTime;
                existingSchedule.ArrivalTime = schedule.ArrivalTime;
                existingSchedule.Price = schedule.Price;
                existingSchedule.AvailableSeats = schedule.AvailableSeats;

                if (oldTrainId != schedule.TrainId)
                {
                    var oldTrain = await _context.Trains.FindAsync(oldTrainId);

                    if (oldTrain != null)
                    {
                        var oldTrainHasSchedules = await _context.Schedules.AnyAsync(s =>
                            s.Id != id &&
                            s.TrainId == oldTrainId
                        );

                        if (!oldTrainHasSchedules)
                        {
                            oldTrain.Status = TrainStatus.Available;
                        }
                    }

                    newTrain.Status = TrainStatus.Scheduled;
                }

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ScheduleExists(schedule.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t =>
                    t.Status != TrainStatus.Broken || t.Id == schedule.TrainId),
                "Id",
                "Name",
                schedule.TrainId
            );

            ViewData["RouteId"] = new SelectList(
                _context.Routes,
                "Id",
                "Name",
                schedule.RouteId
            );

            return View(schedule);
        }

        // GET: Schedule/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Train)
                .Include(s => s.Route)
                .ThenInclude(r => r.DepartureStation)
                .Include(s => s.Route)
                .ThenInclude(r => r.ArrivalStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // POST: Schedule/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Train)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule != null)
            {
                var train = schedule.Train;

                _context.Schedules.Remove(schedule);

                await _context.SaveChangesAsync();

                var trainHasSchedules = await _context.Schedules
                    .AnyAsync(s => s.TrainId == train.Id);

                if (!trainHasSchedules && train.Status == TrainStatus.Scheduled)
                {
                    train.Status = TrainStatus.Available;
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToAction(nameof(Index));
        }

        private bool ScheduleExists(int id)
        {
            return _context.Schedules.Any(e => e.Id == id);
        }
    }
}
