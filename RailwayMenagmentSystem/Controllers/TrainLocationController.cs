using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RailwayMenagmentSystem.Data;
using RailwayMenagmentSystem.Models;

namespace RailwayMenagmentSystem.Controllers
{
    public class TrainLocationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainLocationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: TrainLocation
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TrainLocations.Include(t => t.Station).Include(t => t.Train);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: TrainLocation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainLocation = await _context.TrainLocations
                .Include(t => t.Station)
                .Include(t => t.Train)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainLocation == null)
            {
                return NotFound();
            }

            return View(trainLocation);
        }

        // GET: TrainLocation/Create
        public IActionResult Create()
        {
            ViewData["StationId"] = new SelectList(_context.Stations, "Id", "Address");
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "Model");
            return View();
        }

        // POST: TrainLocation/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,TrainId,StationId,RecordedAt")] TrainLocation trainLocation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainLocation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["StationId"] = new SelectList(_context.Stations, "Id", "Address", trainLocation.StationId);
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "Model", trainLocation.TrainId);
            return View(trainLocation);
        }

        // GET: TrainLocation/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainLocation = await _context.TrainLocations.FindAsync(id);
            if (trainLocation == null)
            {
                return NotFound();
            }
            ViewData["StationId"] = new SelectList(_context.Stations, "Id", "Address", trainLocation.StationId);
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "Model", trainLocation.TrainId);
            return View(trainLocation);
        }

        // POST: TrainLocation/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,TrainId,StationId,RecordedAt")] TrainLocation trainLocation)
        {
            if (id != trainLocation.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(trainLocation);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainLocationExists(trainLocation.Id))
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
            ViewData["StationId"] = new SelectList(_context.Stations, "Id", "Address", trainLocation.StationId);
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "Model", trainLocation.TrainId);
            return View(trainLocation);
        }

        // GET: TrainLocation/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var trainLocation = await _context.TrainLocations
                .Include(t => t.Station)
                .Include(t => t.Train)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (trainLocation == null)
            {
                return NotFound();
            }

            return View(trainLocation);
        }

        // POST: TrainLocation/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var trainLocation = await _context.TrainLocations.FindAsync(id);
            if (trainLocation != null)
            {
                _context.TrainLocations.Remove(trainLocation);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TrainLocationExists(int id)
        {
            return _context.TrainLocations.Any(e => e.Id == id);
        }
    }
}
