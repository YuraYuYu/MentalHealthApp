using MentalHealthApp.Models;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
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
            string currentUserId = User.Identity.GetUserId();
            var appointments = db.Appointments.Where(a => a.UserId == currentUserId).ToList();
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

        // POST: Appointments/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create(AppointmentViewModel model)
        {
            var currentUserId = User.Identity.GetUserId();
            var currentUserEmail = db.Users.Where(u => u.Id == currentUserId).Select(u => u.Email).FirstOrDefault();

            if (ModelState.IsValid)
            {
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

                var appointment = new Appointment
                {
                    PsychologistId = model.PsychologistId,
                    UserId = currentUserId,
                    AppointmentDate = model.AppointmentDate
                };

                db.Appointments.Add(appointment);
                db.SaveChanges();

                TempData["SuccessMessage"] = "Appointment created successfully!";
                return RedirectToAction("Index");
            }

            model.Psychologists = db.Psychologists.Select(p => new SelectListItem
            {
                Value = p.Id.ToString(),
                Text = p.FirstName + " " + p.LastName
            }).ToList();
            
            model.AvailableTimeSlots = GenerateTimeSlots(model.PsychologistId);

            return View(model);
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
            var model = new AppointmentViewModel
            {
                Id = appointment.Id,
                PsychologistId = appointment.PsychologistId,
                UserId = appointment.UserId,
                AppointmentDate = appointment.AppointmentDate,
                Psychologists = new SelectList(db.Psychologists, "Id", "FullName", appointment.PsychologistId),
                Users = new SelectList(db.Users, "Id", "Email", appointment.UserId)
            };
            return View(model);
        }

        // POST: Appointments/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(AppointmentViewModel model)
        {
            if (ModelState.IsValid)
            {
                var appointment = db.Appointments.Find(model.Id);
                if (appointment == null)
                {
                    return HttpNotFound();
                }
                appointment.PsychologistId = model.PsychologistId;
                appointment.UserId = model.UserId;
                appointment.AppointmentDate = model.AppointmentDate;
                db.Entry(appointment).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            model.Psychologists = new SelectList(db.Psychologists, "Id", "FullName", model.PsychologistId);
            model.Users = new SelectList(db.Users, "Id", "Email", model.UserId);
            return View(model);
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
            db.Appointments.Remove(appointment);
            db.SaveChanges();
            return RedirectToAction("Index");
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