using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayMenagmentSystem.Data;
using RailwayMenagmentSystem.Models;

namespace RailwayMenagmentSystem.Controllers
{
    [Authorize]
    public class StationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Station
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Index()
        {
            return View(await _context.Stations.ToListAsync());
        }

        // GET: Station/Details/5
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (station == null)
            {
                return NotFound();
            }

            var departureSchedules = await _context.Schedules
                .Where(s => s.Route.DepartureStationId == id)
                .Include(s => s.Route)
                .ThenInclude(r => r.ArrivalStation)
                .Include(s => s.Train)
                .ToListAsync();

            var arrivalSchedules = await _context.Schedules
                .Where(s => s.Route.ArrivalStationId == id)
                .Include(s => s.Route)
                .ThenInclude(r => r.DepartureStation)
                .Include(s => s.Train)
                .ToListAsync();

            ViewData["DepartureSchedules"] = departureSchedules;
            ViewData["ArrivalSchedules"] = arrivalSchedules;

            return View(station);
        }

        // GET: Station/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Station/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,City,Address")] Station station)
        {
            if (ModelState.IsValid)
            {
                _context.Add(station);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(station);
        }

        // GET: Station/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations.FindAsync(id);

            if (station == null)
            {
                return NotFound();
            }

            return View(station);
        }

        // POST: Station/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,Name,City,Address")] Station station)
        {
            if (id != station.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(station);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StationExists(station.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(station);
        }

        // GET: Station/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var station = await _context.Stations
                .FirstOrDefaultAsync(m => m.Id == id);

            if (station == null)
            {
                return NotFound();
            }

            return View(station);
        }

        // POST: Station/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var station = await _context.Stations.FindAsync(id);

            if (station == null)
            {
                return NotFound();
            }

            var isUsedInRoute = await _context.Routes
                .AnyAsync(r =>
                    r.DepartureStationId == id ||
                    r.ArrivalStationId == id);

            if (isUsedInRoute)
            {
                TempData["ErrorMessage"] =
                    "This station cannot be deleted because it is used by one or more routes.";

                return RedirectToAction(nameof(Index));
            }

            _context.Stations.Remove(station);

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool StationExists(int id)
        {
            return _context.Stations.Any(e => e.Id == id);
        }
    }
}