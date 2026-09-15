using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RailwayMenagmentSystem.Data;
using RailwayMenagmentSystem.Models;
using RailwayMenagmentSystem.Models.Enums;

namespace RailwayMenagmentSystem.Controllers
{
    public class TrainController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TrainController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Train
        public async Task<IActionResult> Index()
        {
            return View(await _context.Trains.ToListAsync());
        }

        // GET: Train/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var train = await _context.Trains
                .FirstOrDefaultAsync(m => m.Id == id);

            if (train == null)
            {
                return NotFound();
            }

            return View(train);
        }

        // GET: Train/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Train/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Model,Capacity")] Train train)
        {
            train.Status = TrainStatus.Available;

            if (ModelState.IsValid)
            {
                _context.Add(train);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(train);
        }

        // GET: Train/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var train = await _context.Trains.FindAsync(id);

            if (train == null)
            {
                return NotFound();
            }

            return View(train);
        }

        // POST: Train/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Model,Capacity")] Train train)
        {
            if (id != train.Id)
            {
                return NotFound();
            }

            var existingTrain = await _context.Trains.FindAsync(id);

            if (existingTrain == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                existingTrain.Name = train.Name;
                existingTrain.Model = train.Model;
                existingTrain.Capacity = train.Capacity;

                try
                {
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TrainExists(train.Id))
                    {
                        return NotFound();
                    }

                    throw;
                }

                return RedirectToAction(nameof(Index));
            }

            return View(train);
        }

        // POST: Train/SetBroken/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetBroken(int id)
        {
            var train = await _context.Trains.FindAsync(id);

            if (train == null)
            {
                return NotFound();
            }

            if (train.Status == TrainStatus.Available ||
                train.Status == TrainStatus.Scheduled)
            {
                train.Status = TrainStatus.Broken;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Train/SetAvailable/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetAvailable(int id)
        {
            var train = await _context.Trains.FindAsync(id);

            if (train == null)
            {
                return NotFound();
            }

            if (train.Status == TrainStatus.Broken)
            {
                train.Status = TrainStatus.Available;
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Train/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var train = await _context.Trains
                .FirstOrDefaultAsync(m => m.Id == id);

            if (train == null)
            {
                return NotFound();
            }

            return View(train);
        }

        // POST: Train/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var train = await _context.Trains.FindAsync(id);

            if (train != null)
            {
                _context.Trains.Remove(train);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        private bool TrainExists(int id)
        {
            return _context.Trains.Any(e => e.Id == id);
        }
    }
}