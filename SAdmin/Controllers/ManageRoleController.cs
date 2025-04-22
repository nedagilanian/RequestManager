using Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Samed.Areas.SAdmin.Controllers
{
    [Area("SAdmin")]
    [Authorize(Roles = "Admin")]
    public class ManageRoleController : Controller
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public ManageRoleController(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            TempData["path"] = "لیست نقش ها";

            var roles = _roleManager.Roles.ToList();
            return View(roles);
        }

        [HttpGet]
        public IActionResult AddRole()
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            TempData["path"] = "افزودن نقش جدید";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddRole(string name, string nameFa)
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            TempData["path"] = "افزودن نقش جدید";

            if (string.IsNullOrEmpty(name)) return NotFound();
            var role = new ApplicationRole() { Name=name,NameFa=nameFa };
            var result = await _roleManager.CreateAsync(role);
            if (result.Succeeded) return RedirectToAction("Index");

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteRole(string id)
        {
            if (string.IsNullOrEmpty(id)) return NotFound();
            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            await _roleManager.DeleteAsync(role);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> EditRole(string id)
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            TempData["path"] = "ویرایش نقش";

            if (string.IsNullOrEmpty(id)) return NotFound();

            var role = await _roleManager.FindByIdAsync(id);

            if (role == null) return NotFound();

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditRole(string id, string name,string nameFa)
        {
            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name)) return NotFound();
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            TempData["path"] = "ویرایش نقش";

            var role = await _roleManager.FindByIdAsync(id);
            if (role == null) return NotFound();
            role.Name = name;
            role.NameFa = nameFa;
            var result = await _roleManager.UpdateAsync(role);
            if (result.Succeeded) return RedirectToAction("Index");

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(role);
        }
    }
}
