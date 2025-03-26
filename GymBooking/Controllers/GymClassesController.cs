using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using GymBooking.Data;
using GymBooking.Models;
using Microsoft.AspNetCore.Identity;
using System.Net.NetworkInformation;
using GymBooking.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace GymBooking.Controllers
{
    [Authorize]
    public class GymClassesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public GymClassesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: GymClasses
        [AllowAnonymous]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var gymClasses = await _context.GymClasses
                .Include(g => g.AttendingMembers)
                .ThenInclude(a => a.User)
                .Where(g => g.StartTime > DateTime.Now)
                .ToListAsync();

            var model = gymClasses.Select(g => new IndexGymClassViewModel
            {
                Id = g.Id,
                Name = g.Name,
                StartTime = g.StartTime,
                Duration = g.Duration,
                Description = g.Description,
                Attending = g.AttendingMembers.Any(a => a.ApplicationUserId == userId)

            }).ToList();


            return View(model);
        }

        // GET: GymClasses/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymClasswithAttendees = await _context.GymClasses
                .Where(g => g.Id == id)
                .Include(c => c.AttendingMembers)
                .ThenInclude(u => u.User).FirstOrDefaultAsync();
               
            if (gymClasswithAttendees == null)
            {
                return NotFound();
            }

            return View(gymClasswithAttendees);
        }

        // GET: GymClasses/Create
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        // POST: GymClasses/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateGymClassViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var gymClass = new GymClass
                {
                    Name = viewModel.Name,
                    StartTime = viewModel.StartTime,
                    Duration = viewModel.Duration,
                    Description = viewModel.Description
                };
                _context.Add(gymClass);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(viewModel);
        }

        // GET: GymClasses/Edit/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymClass = await _context.GymClasses.FindAsync(id);
            if (gymClass == null)
            {
                return NotFound();
            }
            return View(gymClass);
            
        }
        public async Task<IActionResult> BookingToggle(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);

            var attending = await _context.ApplicationUserGymClass.FindAsync(userId, id);

            if (attending == null)
            {
                var booking = new ApplicationUserGymClass
                {
                    ApplicationUserId = userId,
                    GymClassId = (int)id
                };
                _context.ApplicationUserGymClass.Add(booking);
            }
            else
            {
                _context.Remove(attending);
            }

            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            var bookedClasses = _context.ApplicationUserGymClass
                .Where(ug => ug.ApplicationUserId == userId && ug.GymClass.StartTime > DateTime.Now)
                .Select(ug => ug.GymClass)
                .ToList();
            var model = bookedClasses.Select(g => new IndexGymClassViewModel
            {
                Id = g.Id,
                Name = g.Name,
                StartTime = g.StartTime,
                Duration = g.Duration,
                Description = g.Description,
                Attending = g.AttendingMembers.Any(a => a.ApplicationUserId == userId)


            }).ToList();


            return View(model);

          



        }

        public async Task<IActionResult> MyHistory()
        {
            var userId = _userManager.GetUserId(User);
            var bookedClasses = _context.ApplicationUserGymClass
                .Where(ug => ug.ApplicationUserId == userId && ug.GymClass .StartTime <DateTime .Now)
                .Select(ug => ug.GymClass)
                .ToList();
            var model = bookedClasses.Select(g => new IndexGymClassViewModel
            {
                Id = g.Id,
                Name = g.Name,
                StartTime = g.StartTime,
                Duration = g.Duration,
                Description = g.Description,
                Attending = g.AttendingMembers.Any(a => a.ApplicationUserId == userId)


            }).ToList();


            return View(model);

        }

            // POST: GymClasses/Edit/5
            // To protect from overposting attacks, enable the specific properties you want to bind to.
            // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
            [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,StartTime,Duration,Description")] GymClass gymClass)
        {
            if (id != gymClass.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(gymClass);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!GymClassExists(gymClass.Id))
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
            return View(gymClass);
        }

        // GET: GymClasses/Delete/5
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var gymClass = await _context.GymClasses
                .FirstOrDefaultAsync(m => m.Id == id);
            if (gymClass == null)
            {
                return NotFound();
            }

            return View(gymClass);
        }

        // POST: GymClasses/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var gymClass = await _context.GymClasses.FindAsync(id);
            if (gymClass != null)
            {
                _context.GymClasses.Remove(gymClass);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool GymClassExists(int id)
        {
            return _context.GymClasses.Any(e => e.Id == id);
        }
    }
}
