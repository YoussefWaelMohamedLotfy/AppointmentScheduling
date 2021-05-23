using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using AppointmentScheduling.Models.ViewModels;
using AppointmentScheduling.Services;
using AppointmentScheduling.Utility;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentScheduling.Controllers.API
{
    [Route("api/Appointment")]
    [ApiController]
    public class AppointmentApiController : Controller
    {
        private readonly IAppointmentService _service;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string loginUserId;
        private readonly string role;

        public AppointmentApiController(IAppointmentService service, IHttpContextAccessor httpContextAccessor)
        {
            _service = service;
            _httpContextAccessor = httpContextAccessor;
            loginUserId = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            role = _httpContextAccessor.HttpContext.User.FindFirstValue(ClaimTypes.Role);
        }

        [HttpPost]
        [Route("SaveCalendarData")]
        public IActionResult SaveCalendarData(AppointmentVM data)
        {
            CommonResponse<int> commonResponse = new CommonResponse<int>();

            try
            {
                commonResponse.Status = _service.AddUpdate(data).Result;

                if (commonResponse.Status == 1)
                    commonResponse.Message = Helper.appointmentUpdated;
                if (commonResponse.Status == 2)
                    commonResponse.Message = Helper.appointmentAdded;
            }
            catch (Exception e)
            {
                commonResponse.Message = e.Message;
                commonResponse.Status = Helper.failure_code;
            }

            return Ok(commonResponse);
        }

        [HttpGet]
        [Route("GetCalendarData")]
        public IActionResult GetCalendarData(string doctorId)
        {
            CommonResponse<List<AppointmentVM>> commonResponse = new CommonResponse<List<AppointmentVM>>();

            try
            {
                if (role == Helper.Patient)
                {
                    commonResponse.Status = Helper.success_code;
                    commonResponse.DataEnum = _service.PatientsEventsById(loginUserId);
                }
                else if (role == Helper.Doctor)
                {
                    commonResponse.Status = Helper.success_code;
                    commonResponse.DataEnum = _service.DoctorsEventsById(loginUserId);
                }
                else if (role == Helper.Admin)
                {
                    commonResponse.Status = Helper.success_code;
                    commonResponse.DataEnum = _service.DoctorsEventsById(doctorId);
                }
            }
            catch (Exception e)
            {
                commonResponse.Message = e.Message;
                commonResponse.Status = Helper.failure_code;
            }

            return Ok(commonResponse);
        }

        [HttpGet]
        [Route("GetCalendarDataById/{id}")]
        public IActionResult GetCalendarData(int id)
        {
            CommonResponse<AppointmentVM> commonResponse = new CommonResponse<AppointmentVM>();

            try
            {
                commonResponse.DataEnum = _service.GetById(id);
                commonResponse.Status = Helper.success_code;
            }
            catch (Exception e)
            {
                commonResponse.Message = e.Message;
                commonResponse.Status = Helper.failure_code;
            }

            return Ok(commonResponse);
        }

        [HttpGet]
        [Route("DeleteAppointment/{id}")]
        public async Task<IActionResult> DeleteAppointment(int id)
        {
            CommonResponse<int> commonResponse = new CommonResponse<int>();

            try
            {
                commonResponse.Status = await _service.Delete(id);
                commonResponse.Message = commonResponse.Status == 1 ? Helper.appointmentDeleted : Helper.somethingWentWrong;
            }
            catch (Exception e)
            {
                commonResponse.Message = e.Message;
                commonResponse.Status = Helper.failure_code;
            }

            return Ok(commonResponse);
        }

        [HttpGet]
        [Route("ConfirmEvent/{id}")]
        public IActionResult ConfirmEvent(int id)
        {
            CommonResponse<int> commonResponse = new CommonResponse<int>();

            try
            {
                var result = _service.ConfirmEvent(id).Result;

                if (result > 0)
                {
                    commonResponse.Status = Helper.success_code;
                    commonResponse.Message = Helper.meetingConfirm;
                }
                else
                {
                    commonResponse.Status = Helper.failure_code;
                    commonResponse.Message = Helper.meetingConfirmError;
                }

            }
            catch (Exception e)
            {
                commonResponse.Message = e.Message;
                commonResponse.Status = Helper.failure_code;
            }

            return Ok(commonResponse);
        }
    }
}
