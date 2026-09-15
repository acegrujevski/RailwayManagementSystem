using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RailwayMenagmentSystem.Data;
using RailwayMenagmentSystem.Models;
using Route = RailwayMenagmentSystem.Models.Route;

namespace RailwayMenagmentSystem.Controllers
{
    public class RouteController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RouteController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Route
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.Routes.Include(r => r.ArrivalStation).Include(r => r.DepartureStation);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Route/Details/5
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
        public IActionResult Create()
        {
            ViewData["ArrivalStationId"] = new SelectList(_context.Stations, "Id", "Name");
            ViewData["DepartureStationId"] = new SelectList(_context.Stations, "Id", "Name");
            return View();
        }

        // POST: Route/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,DepartureStationId,ArrivalStationId")] Route route)
        {
            var departureStation = await _context.Stations.FindAsync(route.DepartureStationId);
            var arrivalStation = await _context.Stations.FindAsync(route.ArrivalStationId);

            if (departureStation == null || arrivalStation == null)
            {
                return NotFound();
            }

            route.Name = departureStation.Name + " - " + arrivalStation.Name;

            ModelState.Remove("Name");

            if (ModelState.IsValid)
            {
                _context.Add(route);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["ArrivalStationId"] = new SelectList(_context.Stations, "Id", "Name", route.ArrivalStationId);
            ViewData["DepartureStationId"] = new SelectList(_context.Stations, "Id", "Name", route.DepartureStationId);
            return View(route);
        }

        // GET: Route/Edit/5
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

            ViewData["ArrivalStationId"] = new SelectList(_context.Stations, "Id", "Name", route.ArrivalStationId);
            ViewData["DepartureStationId"] = new SelectList(_context.Stations, "Id", "Name", route.DepartureStationId);
            return View(route);
        }

        // POST: Route/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,DepartureStationId,ArrivalStationId")] Route route)
        {
            if (id != route.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var departureStation = await _context.Stations.FindAsync(route.DepartureStationId);
                    var arrivalStation = await _context.Stations.FindAsync(route.ArrivalStationId);

                    if (departureStation == null || arrivalStation == null)
                    {
                        return NotFound();
                    }

                    route.Name = departureStation.Name + " - " + arrivalStation.Name;

                    _context.Update(route);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!RouteExists(route.Id))
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

            ViewData["ArrivalStationId"] = new SelectList(_context.Stations, "Id", "Name", route.ArrivalStationId);
            ViewData["DepartureStationId"] = new SelectList(_context.Stations, "Id", "Name", route.DepartureStationId);
            return View(route);
        }

        // GET: Route/Delete/5
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