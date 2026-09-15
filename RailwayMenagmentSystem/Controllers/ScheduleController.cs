using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
                    (date == null || s.DepartureTime.Date >= date.Value.Date)
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
                .Include(s => s.Route)
                .Include(s => s.Train)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            return View(schedule);
        }

        // GET: Schedule/Create
        public IActionResult Create()
        {
            ViewData["RouteId"] = new SelectList(_context.Routes, "Id", "Name");

            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t => t.Status == TrainStatus.Available),
                "Id",
                "Name"
            );

            return View();
        }

        // POST: Schedule/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,TrainId,RouteId,DepartureTime,ArrivalTime,Price")]
            Schedule schedule)
        {
            var train = await _context.Trains.FindAsync(schedule.TrainId);

            if (train == null)
            {
                return NotFound();
            }

            if (train.Status != TrainStatus.Available)
            {
                ModelState.AddModelError("TrainId", "This train is not available.");
            }

            schedule.AvailableSeats = train.Capacity;

            if (schedule.ArrivalTime <= schedule.DepartureTime)
            {
                ModelState.AddModelError(
                    "ArrivalTime",
                    "Arrival time must be after departure time."
                );
            }

            if (ModelState.IsValid)
            {
                train.Status = TrainStatus.Scheduled;

                _context.Add(schedule);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["RouteId"] = new SelectList(
                _context.Routes,
                "Id",
                "Name",
                schedule.RouteId
            );

            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t => t.Status == TrainStatus.Available || t.Id == schedule.TrainId),
                "Id",
                "Name",
                schedule.TrainId
            );

            return View(schedule);
        }

        // GET: Schedule/Edit/5
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

            ViewData["RouteId"] = new SelectList(
                _context.Routes,
                "Id",
                "Name",
                schedule.RouteId
            );

            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t =>
                    t.Status == TrainStatus.Available || t.Id == schedule.TrainId),
                "Id",
                "Name",
                schedule.TrainId
            );

            return View(schedule);
        }

        // POST: Schedule/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,TrainId,RouteId,DepartureTime,ArrivalTime,Price")]
            Schedule schedule)
        {
            if (id != schedule.Id)
            {
                return NotFound();
            }

            var existingSchedule = await _context.Schedules
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

            if (schedule.ArrivalTime <= schedule.DepartureTime)
            {
                ModelState.AddModelError(
                    "ArrivalTime",
                    "Arrival time must be after departure time."
                );
            }

            if (existingSchedule.TrainId != schedule.TrainId &&
                newTrain.Status != TrainStatus.Available)
            {
                ModelState.AddModelError(
                    "TrainId",
                    "This train is not available."
                );
            }

            if (ModelState.IsValid)
            {
                var oldTrain = await _context.Trains.FindAsync(existingSchedule.TrainId);

                existingSchedule.TrainId = schedule.TrainId;
                existingSchedule.RouteId = schedule.RouteId;
                existingSchedule.DepartureTime = schedule.DepartureTime;
                existingSchedule.ArrivalTime = schedule.ArrivalTime;
                existingSchedule.Price = schedule.Price;
                existingSchedule.AvailableSeats = newTrain.Capacity;

                if (oldTrain != null && oldTrain.Id != newTrain.Id)
                {
                    var oldTrainHasOtherSchedules = await _context.Schedules
                        .AnyAsync(s => s.TrainId == oldTrain.Id && s.Id != id);

                    if (!oldTrainHasOtherSchedules)
                    {
                        oldTrain.Status = TrainStatus.Available;
                    }

                    newTrain.Status = TrainStatus.Scheduled;
                }
                else
                {
                    newTrain.Status = TrainStatus.Scheduled;
                }

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["RouteId"] = new SelectList(
                _context.Routes,
                "Id",
                "Name",
                schedule.RouteId
            );

            ViewData["TrainId"] = new SelectList(
                _context.Trains.Where(t =>
                    t.Status == TrainStatus.Available || t.Id == schedule.TrainId),
                "Id",
                "Name",
                schedule.TrainId
            );

            return View(schedule);
        }

        // GET: Schedule/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var schedule = await _context.Schedules
                .Include(s => s.Route)
                .Include(s => s.Train)
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
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var schedule = await _context.Schedules
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
            {
                return NotFound();
            }

            var train = await _context.Trains.FindAsync(schedule.TrainId);

            _context.Schedules.Remove(schedule);

            await _context.SaveChangesAsync();

            if (train != null)
            {
                var hasOtherSchedules = await _context.Schedules
                    .AnyAsync(s => s.TrainId == train.Id);

                if (!hasOtherSchedules)
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