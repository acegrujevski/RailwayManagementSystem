using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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

        public class TrainLocationPayload
        {
            public int TrainId { get; set; }

            public int StationId { get; set; }
        }

        // GET: TrainLocation
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.TrainLocations
                .Include(t => t.Station)
                .Include(t => t.Train);

            return View(await applicationDbContext.ToListAsync());
        }

        [HttpPost]
        [Route("api/trainlocation")]
        public async Task<IActionResult> ReceiveLocation([FromBody] TrainLocationPayload payload)
        {
            if (payload == null)
            {
                return BadRequest(new
                {
                    message = "Invalid data packet structure."
                });
            }

            var trainExists = await _context.Trains
                .AnyAsync(t => t.Id == payload.TrainId);

            if (!trainExists)
            {
                return NotFound(new
                {
                    message = $"Train with ID '{payload.TrainId}' was not found."
                });
            }

            var stationExists = await _context.Stations
                .AnyAsync(s => s.Id == payload.StationId);

            if (!stationExists)
            {
                return NotFound(new
                {
                    message = $"Station with ID '{payload.StationId}' was not found."
                });
            }

            var trainLocation = new TrainLocation
            {
                TrainId = payload.TrainId,
                StationId = payload.StationId,
                RecordedAt = DateTime.Now
            };

            _context.TrainLocations.Add(trainLocation);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Train location recorded successfully.",
                timestamp = trainLocation.RecordedAt
            });
        }

        // GET: TrainLocation/Details/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            ViewData["StationId"] = new SelectList(_context.Stations, "Id", "Address");
            ViewData["TrainId"] = new SelectList(_context.Trains, "Id", "Model");

            return View();
        }

        // POST: TrainLocation/Create
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,TrainId,StationId,RecordedAt")] TrainLocation trainLocation)
        {
            if (ModelState.IsValid)
            {
                _context.Add(trainLocation);

                await _context.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["StationId"] = new SelectList(
                _context.Stations,
                "Id",
                "Address",
                trainLocation.StationId);

            ViewData["TrainId"] = new SelectList(
                _context.Trains,
                "Id",
                "Model",
                trainLocation.TrainId);

            return View(trainLocation);
        }

        // GET: TrainLocation/Edit/5
        [Authorize(Roles = "Admin")]
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

            ViewData["StationId"] = new SelectList(
                _context.Stations,
                "Id",
                "Address",
                trainLocation.StationId);

            ViewData["TrainId"] = new SelectList(
                _context.Trains,
                "Id",
                "Model",
                trainLocation.TrainId);

            return View(trainLocation);
        }

        // POST: TrainLocation/Edit/5
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Id,TrainId,StationId,RecordedAt")] TrainLocation trainLocation)
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

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            ViewData["StationId"] = new SelectList(
                _context.Stations,
                "Id",
                "Address",
                trainLocation.StationId);

            ViewData["TrainId"] = new SelectList(
                _context.Trains,
                "Id",
                "Model",
                trainLocation.TrainId);

            return View(trainLocation);
        }

        // GET: TrainLocation/Delete/5
        [Authorize(Roles = "Admin")]
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
        [Authorize(Roles = "Admin")]
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