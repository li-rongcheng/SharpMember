using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharpMember.Core.Data;
using SharpMember.Core.Data.Models.MemberSystem;
using SharpMember.Core.Views.ViewModels;
using SharpMember.Core.Views.ViewServices;

namespace SharpMember.Controllers
{
    public class MembersController : Controller
    {
        ApplicationDbContext _context;
        IMemberIndexViewService _memberIndexViewService;
        IMemberCreateViewService _memberCreateViewService;

        public MembersController(
            ApplicationDbContext context,
            IMemberIndexViewService memberIndexViewService,
            IMemberCreateViewService memberCreateViewService
        ){
            this._context = context;
            this._memberIndexViewService = memberIndexViewService;
            this._memberCreateViewService = memberCreateViewService;
        }

        // GET: Members
        public IActionResult Index(int orgId)
        {
            return View(_memberIndexViewService.Get(orgId));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Index(MemberIndexVM model, int orgId)
        {
            _memberIndexViewService.Post(model);
            return View(model);
        }

        // GET: Members/Create
        public IActionResult Create()
        {
            return View(this._memberCreateViewService.Get());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(MemberCreateVM data)
        {
            if (ModelState.IsValid)
            {
                this._memberCreateViewService.Post(data);
                //return RedirectToAction("Index");
            }
            return View(data);
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
