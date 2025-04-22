using Application.Services.Implementations;
using Application.Services.Interfaces;
using DataLayer.Repositories;
using Domain.Entities;
using Domain.Enums;
using Domain.IRepositories;
using Domain.ViewModels.Ticket;
using Domain.ViewModels.TicketFlow;
using Domain.ViewModels.TicketSubject;
using Domain.ViewModels.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Samed.HttpManager;
using System.Net.NetworkInformation;
using System.Security.Claims;

namespace Samed.Areas.SAdmin.Controllers
{
    [Area("SAdmin")]
    [Authorize(Roles = "Admin")]
    public class TicketController : Controller
    {
        #region constructor

        private readonly ITicketService _ticketService;
        private readonly ITicketFlowService _ticketFlowService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IRoleService _roleService;

        public TicketController(ITicketService ticketService, ITicketFlowService ticketFlowService, UserManager<ApplicationUser> userManager, IRoleService roleService)
        {
            _ticketService = ticketService;
            _ticketFlowService = ticketFlowService;
            _userManager = userManager;
            _roleService = roleService;
        }

        #endregion
        public async Task<IActionResult> Index(FilterTicketViewModel filter)
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            TempData["path"] = "ارسال و دریافت تیکت";
            ViewData["subjects"] = new SelectList(await _ticketService.GetAllTicketSubject(), "Id", "Title");

            filter.IsSAdmin = true;

            if (filter != null)
            {
                if (filter.InsertDateFrom != null)
                    filter.InsertDateFrom = Application.Extension.ConvertExtension.ConverttoEn(filter.InsertDateFrom);

                if (filter.InsetDateTo != null)
                    filter.InsetDateTo = Application.Extension.ConvertExtension.ConverttoEn(filter.InsetDateTo);

            }
            //if (!User.IsInRole("TicketAdmin"))
            //{
            //    filter.UserId = User.GetUserId();
            //    filter.AssignUserId = User.GetUserId();
            //}
            //if (User.IsInRole("TicketAdmin"))
            //{
            //    filter.ReceiverUserId = User.GetUserId();

            //}


            filter.TicketStatusList.Add(TicketStatus.initial);
            filter.TicketStatusList.Add(TicketStatus.ClosedByTicketAdmin);
            filter.TicketStatusList.Add(TicketStatus.Checked);
            filter.TicketStatusList.Add(TicketStatus.AnsweredByTechnical);
            // filter.ReceiverRoleId = User.GetRoleId(_roleManager);
            var model = await _ticketService.GetAllTicket(filter);


            return View(model);

        }

        #region add ticketFlow

        public async Task<IActionResult> ReferTicket(int ticketId)
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            TempData["path"] = "ارجاع تیکت";
            var model = new AddTicketFlowViewModel();
            model.TicketFlows = await _ticketFlowService.GetTicketFlowByTicketId(ticketId, null, false);
            var ticket = await _ticketService.GetTicketById(ticketId);
            if (ticket != null)
            {
                model.Subject = ticket.Subject;
                model.Status = ticket.Status;
            }
            var users = _userManager.Users.Include(z => z.UserRoles).ThenInclude(z => z.Role)
        .Where(s => s.Id != User.GetUserId() && s.UserRoles.Any(z => z.Role.Name.Equals("Support")
        || z.Role.Name.Equals("TicketAdmin")
        || z.Role.Name.Equals("Technical")))
  .Select(u => new GetUserViewModel(u.FirstName, u.LastName)
  { Id = u.Id }).ToList();
            ViewData["users"] = new SelectList(users, "Id", "FullName");
            return View(model);
        }
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> ReferTicket(AddTicketFlowViewModel model)
        {
            var users = _userManager.Users.Include(z => z.UserRoles).ThenInclude(z => z.Role)
                .Where(s => s.Id != User.GetUserId() && s.UserRoles.Any(z => z.Role.Name.Equals("Support")
                || z.Role.Name.Equals("TicketAdmin")
                || z.Role.Name.Equals("Technical")))
          .Select(u => new GetUserViewModel(u.FirstName, u.LastName)
          { Id = u.Id }).ToList();
            ViewData["users"] = new SelectList(users, "Id", "FullName");

            model.TicketFlows = await _ticketFlowService.GetTicketFlowByTicketId(model.TicketId, null, false);
            var ticket = await _ticketService.GetTicketById(model.TicketId);
            if (ticket != null)
            {
                model.Subject = ticket.Subject;
                model.Status = ticket.Status;
            }

            if (ModelState.IsValid)
            {
                int stepId = 0;
                var lastTicketflow = await _ticketFlowService.GetLastTicketFlowByTicketId(model.TicketId);
                if (lastTicketflow != null)
                {

                    stepId = lastTicketflow.StepId;
                    lastTicketflow.AnswerDate = DateTime.Now;
                    await _ticketFlowService.UpdateTicketFlow(lastTicketflow);

                }

                var newTicketFlow = new TicketFlow()
                {
                    StepId = ++stepId,
                    Content = model.ContentA,
                    TicketId = model.TicketId,
                    StepDate = DateTime.Now,
                    UserId = User.GetUserId(),
                    ReceiverUserId = model.UserId,

                };
                //if (model.Closed)
                //{
                //    var FirstTicketflow = await _ticketFlowService.GetFirstTicketFlowByTicketId(model.TicketId);
                //    newTicketFlow.ReceiverUserId = FirstTicketflow.UserId;
                //   // newTicketFlow.ReceiverRoleId = _roleService.GetRoleIdByUserId(FirstTicketflow.UserId).Result?.FirstOrDefault();
                //    await _ticketService.UpdateTicket(new UpdateTicketViewModel() { Status = TicketStatus.Finished, Id = model.TicketId });



                //}
                //else
                //{
                //    newTicketFlow.ReceiverUserId = model.UserId;
                //   // newTicketFlow.ReceiverRoleId = _roleService.GetRoleIdByUserId(model.UserId).Result?.FirstOrDefault();
                //    await _ticketService.UpdateTicket(new UpdateTicketViewModel() { Status = TicketStatus.Checked, Id = model.TicketId });
                //}

                await _ticketService.UpdateTicket(new UpdateTicketViewModel() { Status = TicketStatus.Checked, Id = model.TicketId });


                var result = await _ticketFlowService.AddTicketFlow(newTicketFlow, null);
                switch (result)
                {
                    case AddResult.Error:
                        break;
                    case AddResult.Success:
                        TempData["SuccessMessage"] = "ارجاع تیکت با موفقیت ثبت شد";
                        return RedirectToAction("Index", "Ticket");
                }

            }
            return View(model);
        }
        #endregion
        #region add ticket

        [HttpGet]
        public async Task<IActionResult> CreateTicket()
        {
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();


            TempData["path"] = "ارسال تیکت";
            ViewData["subjects"] = new SelectList(await _ticketService.GetAllTicketSubject(), "Id", "Title");

            return View();
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateTicket(AddTicketViewModel ticket)
        {
            ViewData["subjects"] = new SelectList(await _ticketService.GetAllTicketSubject(), "Id", "Title");
            ticket.UserId = User.GetUserId();

            if (ModelState.IsValid)
            {
                var result = await _ticketService.AddTicket(ticket);
                switch (result)
                {
                    case AddResult.Error:
                        break;
                    case AddResult.Success:
                        TempData["SuccessMessage"] = "تیکت با موفقیت ثبت شد";
                        return RedirectToAction("Index", "Ticket");
                }
            }

            return View(ticket);
        }

        #endregion



        public async Task<IActionResult> AdminCloseTicket(int id)
        {
            TempData["UnreadMsg"] = HttpContext.Session.GetString("UnreadMsg");
            if (Request.GetTypedHeaders().Referer != null)
                TempData["back"] = Request.GetTypedHeaders().Referer.ToString();

            var uId = User.GetUserId();

            var FirstTicketflow = await _ticketFlowService.GetFirstTicketFlowByTicketId(id);


            await _ticketService.UpdateTicket(new UpdateTicketViewModel() { Id = id, Status = TicketStatus.Finished });
            var lastTicketflow = await _ticketFlowService.GetLastTicketFlowByTicketId(id);

            lastTicketflow.AnswerDate = DateTime.Now;
            lastTicketflow.ReadDate = DateTime.Now;
            lastTicketflow.IsRead = true;
            await _ticketFlowService.UpdateTicketFlow(lastTicketflow);
            var newTicketFlow = new TicketFlow()
            {
                StepId = ++lastTicketflow.StepId,
                Content = "مراحل انجام تیکت شما به پایان رسید و تیکت بسته شد.",
                TicketId = id,
                StepDate = DateTime.Now,
                UserId = uId,
                IsPublic = true,
                ReceiverUserId = FirstTicketflow.UserId

            };
            var result = await _ticketFlowService.AddTicketFlow(newTicketFlow, null);
            return new JsonResult(new
            {
                status = "successTicketAdmin"
            });


        }

    }
}
