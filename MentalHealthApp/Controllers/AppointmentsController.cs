using MentalHealthApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using System.Web.Mvc;
using static MentalHealthApp.Models.AppointmentViewModel;

namespace MentalHealthApp.Controllers
{
    public class AppointmentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Appointments
        public ActionResult Index()
        {
            var userId = User.Identity.GetUserId();
            var appointments = db.Appointments
                                .Where(a => a.UserId == userId)
                                .Include(a => a.Psychologist)
                                .ToList();

            return View(appointments);
        }

        // GET: Appointments/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }

            var psychologist = db.Psychologists.Find(appointment.PsychologistId);
            var model = new AppointmentViewModel
            {
                Id = appointment.Id,
                PsychologistId = appointment.PsychologistId,
                UserId = appointment.UserId,
                AppointmentDate = appointment.AppointmentDate,
                Psychologists = new SelectList(db.Psychologists, "Id", "FullName", appointment.PsychologistId),
                Users = new SelectList(db.Users, "Id", "Email", appointment.UserId),
                CaseFilePath = appointment.CaseFilePath // 添加附件路径
            };

            return View(appointment);
        }

        // GET: Appointments/Create
        [Authorize]
        public ActionResult Create()
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUserEmail = db.Users.Where(u => u.Id == currentUserId).Select(u => u.Email).FirstOrDefault();

            var model = new AppointmentViewModel();
            model.Psychologists = db.Psychologists.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FirstName + " " + p.LastName
            }).ToList();
            model.UserId = User.Identity.GetUserId(); // 设置当前用户ID
            model.AvailableTimeSlots = new List<TimeSlotViewModel>();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(AppointmentViewModel model, HttpPostedFileBase CaseFile)
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUserEmail = db.Users.Where(u => u.Id == currentUserId).Select(u => u.Email).FirstOrDefault();

            if (ModelState.IsValid)
            {
                // 检查预约时间段是否已经被预定
                if (db.Appointments.Any(a => a.PsychologistId == model.PsychologistId && a.AppointmentDate == model.AppointmentDate))
                {
                    ModelState.AddModelError("", "This time slot has already been booked.");
                    model.Psychologists = db.Psychologists.Select(p => new SelectListItem
                    {
                        Value = p.Id.ToString(),
                        Text = p.FirstName + " " + p.LastName
                    }).ToList();

                    model.AvailableTimeSlots = GenerateTimeSlots(model.PsychologistId);

                    return View(model);
                }

                // 保存文件路径
                string filePath = null;
                if (CaseFile != null && CaseFile.ContentLength > 0)
                {
                    var fileName = Path.GetFileName(CaseFile.FileName);
                    filePath = Path.Combine(Server.MapPath("~/uploads/CaseFiles/"), Guid.NewGuid().ToString() + Path.GetExtension(fileName));
                    CaseFile.SaveAs(filePath);
                }

                // 创建新的预约
                var appointment = new Appointment
                {
                    PsychologistId = model.PsychologistId,
                    UserId = currentUserId,
                    AppointmentDate = model.AppointmentDate,
                    CaseFilePath = filePath
                };

                db.Appointments.Add(appointment);
                db.SaveChanges();

                // 发送邮件通知
                SendAppointmentEmailNotification(currentUserEmail, appointment, filePath);

                TempData["SuccessMessage"] = "Appointment created successfully!";
                return RedirectToAction("Index", "Manage");
            }

            // 重新加载Psychologist列表和可用时间段
            model.Psychologists = db.Psychologists.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FirstName + " " + p.LastName
            }).ToList();

            model.AvailableTimeSlots = GenerateTimeSlots(model.PsychologistId);

            return View(model);
        }

        private void SendAppointmentEmailNotification(string userEmail, Appointment appointment, string filePath)
        {
            try
            {
                // 获取心理咨询师的详细信息
                var psychologist = db.Psychologists.Find(appointment.PsychologistId);
                var psychologistName = psychologist != null ? $"{psychologist.FirstName} {psychologist.LastName}" : "Unknown";

                // 构建邮件内容
                var mail = new MailMessage
                {
                    From = new MailAddress("yurayu0222@outlook.com", "MentalHealthWeb"),
                    Subject = "Appointment Confirmation",
                    Body = $"Dear {userEmail},\n\n" +
                           $"Your appointment has been successfully booked.\n\n" +
                           $"Appointment Details:\n" +
                           $"Psychologist: {psychologistName}\n" +
                           $"User Email: {userEmail}\n" +
                           $"Start Time: {appointment.AppointmentDate.ToString("f")}\n" +
                           $"End Time: {appointment.AppointmentDate.AddHours(2).ToString("f")}\n\n" + // 假设预约时长为2小时
                           $"Thank you for using our service.",
                    IsBodyHtml = false
                };

                // 添加收件人
                mail.To.Add(userEmail);

                // 如果有附件则添加
                if (!string.IsNullOrEmpty(filePath))
                {
                    mail.Attachments.Add(new Attachment(filePath));
                }

                // 配置 SMTP 客户端
                var smtpClient = new SmtpClient("smtp-mail.outlook.com", 587)
                {
                    Credentials = new NetworkCredential("yurayu0222@outlook.com", "13325134268yy"),
                    EnableSsl = true
                };

                // 发送邮件
                smtpClient.Send(mail);
            }
            catch (Exception ex)
            {
                // 处理发送邮件时的异常
                Console.WriteLine("Error sending email: " + ex.Message);
            }
        }

        // GET: Appointments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var appointment = db.Appointments.Find(id);

            if (appointment == null)
            {
                return HttpNotFound();
            }

            // 检查预约时间是否已过
            if (appointment.AppointmentDate <= DateTime.Now)
            {
                TempData["ErrorMessage"] = "You cannot edit an appointment that has already passed.";
                return RedirectToAction("Index", "Manage"); // 重定向到管理页面或其他合适页面
            }

            var model = new AppointmentViewModel
            {
                Id = appointment.Id,
                PsychologistId = appointment.PsychologistId,
                UserId = appointment.UserId,
                AppointmentDate = appointment.AppointmentDate,
                Psychologists = new SelectList(db.Psychologists, "Id", "FullName", appointment.PsychologistId),
                Users = new SelectList(db.Users, "Id", "Email", appointment.UserId),
                CaseFilePath = appointment.CaseFilePath // 添加附件路径
            };

            return View(model);
        }



        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentViewModel model, HttpPostedFileBase caseFile)
        {
            if (ModelState.IsValid)
            {
                var appointment = db.Appointments.Find(model.Id);
                if (appointment == null)
                {
                    return HttpNotFound();
                }

                // 处理上传的附件（如果有新的文件上传）
                if (caseFile != null && caseFile.ContentLength > 0)
                {
                    // 删除旧文件
                    if (!string.IsNullOrEmpty(appointment.CaseFilePath))
                    {
                        var oldFilePath = Server.MapPath(appointment.CaseFilePath);
                        if (System.IO.File.Exists(oldFilePath))
                        {
                            System.IO.File.Delete(oldFilePath);
                        }
                    }

                    // 保存新文件
                    var fileName = Path.GetFileName(caseFile.FileName);
                    var filePath = Path.Combine(Server.MapPath("~/uploads/CaseFiles/"), fileName);
                    caseFile.SaveAs(filePath);

                    // 更新文件路径
                    appointment.CaseFilePath = "~/uploads/CaseFiles/" + fileName;
                }

                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index", "Manage");
            }

            model.Psychologists = new SelectList(db.Psychologists, "Id", "FullName", model.PsychologistId);
            model.Users = new SelectList(db.Users, "Id", "Email", model.UserId);
            return RedirectToAction("Index", "Manage");
        }

        // GET: Appointments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Appointment appointment = db.Appointments.Find(id);
            if (appointment == null)
            {
                return HttpNotFound();
            }
            return View(appointment);
        }

        // POST: Appointments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Appointment appointment = db.Appointments.Find(id);

            // 删除文件（如果存在）
            if (!string.IsNullOrEmpty(appointment.CaseFilePath))
            {
                var filePath = Server.MapPath(appointment.CaseFilePath);
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }

            db.Appointments.Remove(appointment);
            db.SaveChanges();
            return RedirectToAction("Index", "Manage");
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Rate(int appointmentId, double score)
        {
            if (score < 0 || score > 5)
            {
                ModelState.AddModelError("score", "The score must be between 0 and 5.");
                return RedirectToAction("Index", "Manage");
            }

            var appointment = db.Appointments.Find(appointmentId);
            if (appointment == null || !appointment.CanRate)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            appointment.RatingScore = score;
            db.SaveChanges();

            var psychologist = db.Psychologists.Find(appointment.PsychologistId);
            psychologist.AverageRating = db.Appointments
                .Where(a => a.PsychologistId == psychologist.Id && a.RatingScore.HasValue)
                .Average(a => (double?)a.RatingScore) ?? 0.0;
            db.SaveChanges();

            return RedirectToAction("Index", "Manage");
        }


        public JsonResult GetAvailableTimeSlots(int psychologistId)
        {
            var timeSlots = GenerateTimeSlots(psychologistId);
            var formattedSlots = timeSlots.Select(slot => new
            {
                Date = slot.Date.ToString("yyyy-MM-dd"),
                Time = slot.Time,
                IsAvailable = slot.IsAvailable,
                Display = $"{slot.Date.ToShortDateString()} {slot.Time}"
            });
            return Json(formattedSlots, JsonRequestBehavior.AllowGet);
        }


        private List<TimeSlotViewModel> GenerateTimeSlots(int psychologistId)
        {
            List<TimeSlotViewModel> timeSlots = new List<TimeSlotViewModel>();
            DateTime today = DateTime.Today;
            for (int i = 0; i < 7; i++)
            {
                DateTime date = today.AddDays(i);
                timeSlots.Add(new TimeSlotViewModel { Date = date, Time = "09:00 - 11:00" });
                timeSlots.Add(new TimeSlotViewModel { Date = date, Time = "14:00 - 16:00" });
            }

            var allAppointments = db.Appointments.Where(a => a.PsychologistId == psychologistId).ToList();
            foreach (var slot in timeSlots)
            {
                var slotStartTime = DateTime.Parse(slot.Time.Split('-')[0].Trim());
                var slotEndTime = DateTime.Parse(slot.Time.Split('-')[1].Trim());

                slot.IsAvailable = !allAppointments.Any(a => a.AppointmentDate.Date == slot.Date.Date &&
                                                             a.AppointmentDate.TimeOfDay >= slotStartTime.TimeOfDay &&
                                                             a.AppointmentDate.TimeOfDay < slotEndTime.TimeOfDay);
            }

            return timeSlots;
        }



        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}