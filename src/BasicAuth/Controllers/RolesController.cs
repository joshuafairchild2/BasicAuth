using BasicAuth.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using System.Linq;
using BasicAuth.ViewModels;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BasicAuth.Controllers
{
    [Authorize]
    public class RolesController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;


        public RolesController(ApplicationDbContext db, UserManager<ApplicationUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }
        public IActionResult Index()
        {
            List<IdentityRole> roles = _db.Roles.ToList();
            return View(roles);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateRoleViewModel model)
        {
            try
            {
                _db.Roles.Add(new IdentityRole()
                {
                    Name = model.RoleName
                });
                _db.SaveChanges();
                return RedirectToAction("Index");
            }
            catch
            {
                return View();
            }
        }

        public IActionResult Delete(string RoleName)
        {
            IdentityRole thisRole = _db.Roles.Where(r => r.Name.Equals(RoleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();
            _db.Roles.Remove(thisRole);
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult Edit(string roleName)
        {
            IdentityRole thisRole = _db.Roles.Where(r => r.Name.Equals(roleName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            return View(thisRole);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(IdentityRole role)
        {
            IdentityRole oldRole = _db.Roles.Where(r => r.Id == role.Id).FirstOrDefault();
            oldRole.Name = role.Name;
            _db.SaveChanges();
            return RedirectToAction("Index");
        }

        public IActionResult ManageUserRoles()
        {
            List<SelectListItem> list = _db.Roles.OrderBy(role => role.Name).ToList().Select(r =>
                new SelectListItem { Value = r.Name.ToString(), Text = r.Name }).ToList();
            ViewBag.Roles = list;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRoleToUser(string UserName, string RoleName)
        {
            //try
            //{
            ApplicationUser user = _db.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

            await _userManager.AddToRoleAsync(user, RoleName);

            ViewBag.ResultMessage = "Role created successfully !";

            //List<SelectListItem> list = _db.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
            //ViewBag.Roles = list;

            return View("ManageUserRoles");
            //}
            //catch
            //{
                //return View("ManageUserRoles");
            //}
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> GetRoles(string UserName)
        {
            if (!string.IsNullOrWhiteSpace(UserName))
            {
                ApplicationUser user = _db.Users.Where(u => u.UserName.Equals(UserName, StringComparison.CurrentCultureIgnoreCase)).FirstOrDefault();

                ViewBag.RolesForThisUser = await _userManager.GetRolesAsync(user);

                List<SelectListItem> list = _db.Roles.OrderBy(r => r.Name).ToList().Select(rr => new SelectListItem { Value = rr.Name.ToString(), Text = rr.Name }).ToList();
                ViewBag.Roles = list;
            }

            return View("ManageUserRoles");
        }

    }
}
