using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SharpMember.Core.Data;
using SharpMember.Core.Data.Models;
using SharpMember.Core.Data.Models.MemberSystem;
using SharpMember.Core.Views.MemberSystem;

namespace SharpMember.Controllers
{
    public class MembersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MembersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public Task<IActionResult> ProfileItems()
        {
            List<MemberProfileItem> model = new List<MemberProfileItem>();
            for (int i = 0; i < 5; i++)
            {
                model.Add(new MemberProfileItem()); // add 5 empty item slots
            }
            return Task.FromResult<IActionResult>(View(model));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public Task<IActionResult> ProfileItems(List<MemberProfileItem> data)
        {
            List<MemberProfileItem> model = new List<MemberProfileItem>();
            model.AddRange(data);
            for (int i = 0; i < 5; i++)
            {
                model.Add(new MemberProfileItem()); // add 5 empty item slots
            }
            return Task.FromResult<IActionResult>(View(model));
        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
            return View(await _context.Members.ToListAsync());
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            var model = new MemberCreateViewModel();
            for (int i = 0; i < 5; i++)
            {
                model.MemberProfile.MemberProfileItems.Add(new MemberProfileItem { ItemName = $"item {i}" }); // add 5 empty item slots
            }
            return View(model);
        }

        // POST: Members/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberCreateViewModel member)
        {
            //if (ModelState.IsValid)
            //{
            //    //_context.Add(member);
            //    //await _context.SaveChangesAsync();
            //    //return RedirectToAction("Index");
            //}
            return View(member);
        }

        // GET: Members/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members.SingleOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: Members/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Member member)
        {
            if (id != member.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(member);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MemberExists(member.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(member);
        }

        // GET: Members/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var member = await _context.Members
                .SingleOrDefaultAsync(m => m.Id == id);
            if (member == null)
            {
                return NotFound();
            }

            return View(member);
        }

        // POST: Members/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var member = await _context.Members.SingleOrDefaultAsync(m => m.Id == id);
            _context.Members.Remove(member);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool MemberExists(int id)
        {
            return _context.Members.Any(e => e.Id == id);
        }
    }
}
