using Application.Extension;
using Application.Services.Interfaces;
using Application.Statics;
using Domain.Entities;
using Domain.ViewModels.Request;
using Domain.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Samed.HttpManager;

namespace Samed.Areas.SAdmin.Controllers
{

    [Area("SAdmin")]
    [Authorize(Roles = "Admin,StrategicAdmin,TechAdmin")]
    public class RequestController : Controller
    {
        #region constructor
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IRoleService _roleService;

        private readonly IRequestService _RequestService;
        public RequestController(IRequestService RequestService, UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager, IRoleService roleService)
        {
            _RequestService = RequestService;
            _userManager = userManager;
            _roleManager = roleManager;
            _roleService = roleService;
        }

        #endregion

        public async Task<IActionResult> Index(FilterRequestViewModel filter)
        {
            TempData["path"] = "لیست درخواست ها";
            var model = await _RequestService.GetAllRequest(filter);
            return View(model);
        }

        public async Task<IActionResult> Cartable(FilterRequestViewModel filter)
        {
            TempData["path"] = "کارتابل";

            if (User.IsInRole("TechAdmin"))
                filter.StepList.Add((byte)Domain.Enums.RequestStep.Check);
            if (User.IsInRole("StrategicAdmin"))
			{
                filter.StepList.Add((byte)Domain.Enums.RequestStep.Final);
            }

            var model = await _RequestService.GetAllRequest(filter);
            return View(model);
        }



        [HttpGet]
        public IActionResult AddRequest()
        {
            TempData["path"] = "ثبت درخواست جدید";

            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            var model = new RequestViewModel();
            model.SystemTitles = _RequestService.GetSystemTitles();


            return View(model);
        }
        [HttpPost]
        public JsonResult GetSubSystems(int systemId)
        {
            var s = _RequestService.GetSubSystems(systemId);
            return Json(s);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public IActionResult AddRequest(RequestViewModel request)
        {

            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();
            if (ModelState.IsValid)
            {
                var result = _RequestService.AddRequest(request, request.FileNames, request.fileDes);

                return RedirectToAction("Index");
            }
            request.SystemTitles = _RequestService.GetSystemTitles();
            return View("AddRequest", request);
        }
        public IActionResult UpdateRequestByStAdmin(RequestViewModel request)
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

           // _RequestService.AddRequest(request, request.FileNames, request.fileDes);
            request.msgDetail = string.Format("{0}", PersianDate.Standard.ConvertDate.ToFa(DateTime.Now));
            var users = _userManager.Users.Where(s => s.Id == User.GetUserId()).FirstOrDefault();
            if (users != null)
                request.msgDetail = string.Format("{0} - {1}", PersianDate.Standard.ConvertDate.ToFa(DateTime.Now), users.FirstName + " " + users.LastName);

            if (request.DoneStatus == null)
                request.DoneStatus = false;

            _RequestService.UpdateRequest(request);
            if(request.FileNames != null)
            if (request.FileNames.Count > 0)
            {
               _RequestService.AddFiles(request.Id,request.FileNames,request.fileDes); 
            }


            TempData["msg"] = "ثبت تغییرات انجام شد ";
            return RedirectToAction("CheckByAdmin", new { requestId = request.Id, CurrentTab = request.CurrentTab });
        }
        
        public IActionResult UpdateComments(RequestViewModel request)
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();


            request.msgDetail = string.Format("{0}", PersianDate.Standard.ConvertDate.ToFa(DateTime.Now));
            var users = _userManager.Users.Where(s => s.Id == User.GetUserId()).FirstOrDefault();
            if (users != null)
                request.msgDetail = string.Format("{0} - {1}", PersianDate.Standard.ConvertDate.ToFa(DateTime.Now), users.FirstName + " " + users.LastName);


            _RequestService.UpdateRequest(request);
            TempData["msg"] = "ثبت تغییرات انجام شد ";
            return RedirectToAction("CheckByAdmin", new { requestId = request.Id, CurrentTab = request.CurrentTab });
        }

        public IActionResult UpdateChats(RequestViewModel request)
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();


            var msg = string.Format("{0}", PersianDate.Standard.ConvertDate.ToFa(DateTime.Now));
            var users = _userManager.Users.Where(s => s.Id == User.GetUserId()).FirstOrDefault();
            if (users != null)
                msg = string.Format("{0} - {1}", PersianDate.Standard.ConvertDate.ToFa(DateTime.Now), users.FirstName + " " + users.LastName);


            _RequestService.UpdateRequest(new RequestViewModel() {Id=request.Id,msgDetail= msg, Chat=request.Chat });
            TempData["msg"] = "ثبت تغییرات انجام شد ";

            return RedirectToAction("CheckByAdmin", new { requestId = request.Id, CurrentTab = request.CurrentTab });
       
        }


        public IActionResult CheckByAdmin(int requestId, string? CurrentTab)
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            TempData["path"] = "بررسی درخواست توسط مدیر";
            var model = _RequestService.GetRequestById(requestId);

            if (model.SeenDate == null && User.IsInRole("TechAdmin"))
            {
                var req = new RequestViewModel()
                {
                    Id = requestId,
                    SeenDate = DateTime.Now,
                };
                _RequestService.UpdateRequest(req);
            }
            model.SystemTitles = _RequestService.GetSystemTitles();

            if (model.Type == 1)
                model.TypeStr = "قابلیت";
            if (model.Type == 2)
                model.TypeStr = "گزارش";
            if (model.Type == 3)
                model.TypeStr = "داده خام";

            if (model.SystemId != null)
                model.SystemIdStr = _RequestService.GetSystemTitles().Where(x => x.Value == model.SystemId.Value.ToString()).Select(x => x.Text).FirstOrDefault();
            if (model.SubSystemId != null)
                model.SubSystemIdStr = _RequestService.GetSubSystems(model.SystemId.Value).Where(x => x.Value == model.SubSystemId.ToString()).Select(x => x.Text).FirstOrDefault();
            if (model.OfferDoneDate != null)
                model.OfferDoneDateStr = PersianDate.Standard.ConvertDate.ToFa(model.OfferDoneDate);
            if (model.DueDate != null)
                model.DueDateStr = PersianDate.Standard.ConvertDate.ToFa(model.DueDate);
            if (model.BeginDate != null)
                model.BeginDateStr = PersianDate.Standard.ConvertDate.ToFa(model.BeginDate);


            if (model.Priority == 1)
                model.PriorityStr = "زیاد";
            if (model.Priority == 2)
                model.PriorityStr = "متوسط";
            if (model.Priority == 3)
                model.PriorityStr = "کم";

            model.DoneByNames.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Value = "532", Text = "خانم امیری" });
            model.DoneByNames.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Value = "2", Text = "خانم نعمتی" });
            model.DoneByNames.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Value = "3", Text = "خانم دلاوری" });
            //model.DoneByNames.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Value = "4", Text = "خانم رودی" });
            model.ActiveTab = CurrentTab != null ? CurrentTab : "Detail";
            return View(model);
        }
        [HttpPost]
        public IActionResult SaveAdminIdea(RequestViewModel request)
        {

            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            request.SeenDate = DateTime.Now;

            request.msgDetail = string.Format("{0}", PersianDate.Standard.ConvertDate.ToFa(DateTime.Now));
            var users = _userManager.Users.Where(s => s.Id == User.GetUserId()).FirstOrDefault();
            if (users != null)
                request.msgDetail = string.Format("{0} - {1}", PersianDate.Standard.ConvertDate.ToFa(DateTime.Now), users.FirstName + " " + users.LastName);

            _RequestService.UpdateRequest(request);

            return RedirectToAction("CheckByAdmin", new { requestId=request.Id, CurrentTab=request.CurrentTab });
        }


        public async Task<IActionResult> Archive(FilterRequestViewModel filter)
        {
            TempData["path"] = "لیست درخواست های آرشیو شده";
            filter.stateList.Add((byte)Domain.Enums.RequestState.Cancel);
            filter.stateList.Add((byte)Domain.Enums.RequestState.Publish);

            var model = await _RequestService.GetAllRequest(filter);
            return View(model);
        }


    }
}
