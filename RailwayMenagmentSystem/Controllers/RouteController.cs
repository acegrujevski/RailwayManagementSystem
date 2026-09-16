using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RailwayMenagmentSystem.Data;
using RailwayMenagmentSystem.Models;
using Route = RailwayMenagmentSystem.Models.Route;

namespace RailwayMenagmentSystem.Controllers
{
    [Authorize]
    public class RouteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RouteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Route
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Routes
                .Include(r => r.ArrivalStation)
                .Include(r => r.DepartureStation);

            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Route/Details/5
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Routes
                .Include(r => r.ArrivalStation)
                .Include(r => r.DepartureStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        // GET: Route/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["ArrivalStationId"] =
                new SelectList(_context.Stations, "Id", "Name");

            ViewData["DepartureStationId"] =
                new SelectList(_context.Stations, "Id", "Name");

            return View();
        }

        // GET: Route/CreateRouteFromStation/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateRouteFromStation(
            int departureStationId)
        {
            var departureStation =
                await _context.Stations.FindAsync(departureStationId);

            if (departureStation == null)
            {
                return NotFound();
            }

            ViewData["DepartureStation"] = departureStation;

            ViewData["ArrivalStationId"] = new SelectList(
                _context.Stations.Where(s => s.Id != departureStationId),
                "Id",
                "Name"
            );

            var route = new Route
            {
                DepartureStationId = departureStationId
            };

            return View(route);
        }

        // POST: Route/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(
            [Bind("Id,DepartureStationId,ArrivalStationId")]
            Route route)
        {
            var departureStation =
                await _context.Stations.FindAsync(route.DepartureStationId);

            var arrivalStation =
                await _context.Stations.FindAsync(route.ArrivalStationId);

            if (departureStation == null || arrivalStation == null)
            {
                return NotFound();
            }

            var routeExists = await _context.Routes.AnyAsync(r =>
                r.DepartureStationId == route.DepartureStationId &&
                r.ArrivalStationId == route.ArrivalStationId);

            if (route.DepartureStationId == route.ArrivalStationId)
            {
                ModelState.AddModelError(
                    "ArrivalStationId",
                    "Departure and arrival stations must be different."
                );
            }

            if (routeExists)
            {
                ModelState.AddModelError(
                    "",
                    "A route between these stations already exists."
                );
            }

            route.Name =
                departureStation.Name + " - " + arrivalStation.Name;

            ModelState.Remove("Name");

            if (ModelState.IsValid)
            {
                _context.Add(route);
                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["ArrivalStationId"] =
                new SelectList(
                    _context.Stations,
                    "Id",
                    "Name",
                    route.ArrivalStationId
                );

            ViewData["DepartureStationId"] =
                new SelectList(
                    _context.Stations,
                    "Id",
                    "Name",
                    route.DepartureStationId
                );

            return View(route);
        }

        // GET: Route/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Routes.FindAsync(id);

            if (route == null)
            {
                return NotFound();
            }

            ViewData["ArrivalStationId"] =
                new SelectList(
                    _context.Stations,
                    "Id",
                    "Name",
                    route.ArrivalStationId
                );

            ViewData["DepartureStationId"] =
                new SelectList(
                    _context.Stations,
                    "Id",
                    "Name",
                    route.DepartureStationId
                );

            return View(route);
        }

        // POST: Route/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,DepartureStationId,ArrivalStationId")]
            Route route)
        {
            if (id != route.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var departureStation =
                        await _context.Stations.FindAsync(
                            route.DepartureStationId);

                    var arrivalStation =
                        await _context.Stations.FindAsync(
                            route.ArrivalStationId);

                    if (departureStation == null ||
                        arrivalStation == null)
                    {
                        return NotFound();
                    }

                    route.Name =
                        departureStation.Name +
                        " - " +
                        arrivalStation.Name;

                    _context.Update(route);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteExists(route.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["ArrivalStationId"] =
                new SelectList(
                    _context.Stations,
                    "Id",
                    "Name",
                    route.ArrivalStationId
                );

            ViewData["DepartureStationId"] =
                new SelectList(
                    _context.Stations,
                    "Id",
                    "Name",
                    route.DepartureStationId
                );

            return View(route);
        }

        // GET: Route/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var route = await _context.Routes
                .Include(r => r.ArrivalStation)
                .Include(r => r.DepartureStation)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (route == null)
            {
                return NotFound();
            }

            return View(route);
        }

        // POST: Route/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var route = await _context.Routes.FindAsync(id);

            if (route != null)
            {
                _context.Routes.Remove(route);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool RouteExists(int id)
        {
            return _context.Routes.Any(e => e.Id == id);
        }
    }

}
