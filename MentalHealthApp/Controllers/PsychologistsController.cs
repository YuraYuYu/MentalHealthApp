using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MentalHealthApp.Models;

namespace MentalHealthApp.Controllers
{
    public class PsychologistsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        // GET: Psychologists
        public ActionResult Index()
        {
            ViewBag.IsAdmin = User.IsInRole("Admin");
            var psychologists = db.Psychologists.ToList();
            foreach (var psychologist in psychologists)
            {
                var averageRating = db.Appointments
                    .Where(a => a.PsychologistId == psychologist.Id && a.RatingScore.HasValue)
                    .Average(a => (double?)a.RatingScore) ?? 0.0;
                psychologist.AverageRating = averageRating;
            }

            return View(psychologists);
        }



        // GET: Psychologists/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Psychologist psychologist = db.Psychologists.Find(id);
            if (psychologist == null)
            {
                return HttpNotFound();
            }
            var averageRating = db.Ratings
                                  .Where(r => r.PsychologistId == id)
                                  .Average(r => (int?)r.Score) ?? 0;
            ViewBag.AverageRating = averageRating == 0 ? "No ratings" : averageRating.ToString("0.0");
            return View(psychologist);
        }

        [Authorize(Roles = "Admin")]
        // GET: Psychologists/Create
        public ActionResult Create()
        {
            ViewBag.Images = GetImages();
            return View();
        }

        // POST: Psychologists/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Create([Bind(Include = "Id,FirstName,LastName,Specialization,Rating,Photo")] Psychologist psychologist)
        {
            if (ModelState.IsValid)
            {
                db.Psychologists.Add(psychologist);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.Images = GetImages();
            return View(psychologist);
        }


        // GET: Psychologists/Edit/5
        [Authorize(Roles = "Admin")]
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Psychologist psychologist = db.Psychologists.Find(id);
            if (psychologist == null)
            {
                return HttpNotFound();
            }
            ViewBag.Images = GetImages();
            return View(psychologist);
        }

        // POST: Psychologists/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult Edit([Bind(Include = "Id,FirstName,LastName,Specialization,Rating,Photo")] Psychologist psychologist)
        {
            if (ModelState.IsValid)
            {
                db.Entry(psychologist).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.Images = GetImages();
            return View(psychologist);
        }

        // GET: Psychologists/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Psychologist psychologist = db.Psychologists.Find(id);
            if (psychologist == null)
            {
                return HttpNotFound();
            }
            return View(psychologist);
        }

        // POST: Psychologists/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteConfirmed(int id)
        {
            Psychologist psychologist = db.Psychologists.Find(id);
            db.Psychologists.Remove(psychologist);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Export()
        {
            var appointments = db.Appointments
                                 .Include(a => a.Psychologist)
                                 .OrderBy(a => a.PsychologistId)
                                 .ThenBy(a => a.AppointmentDate)
                                 .ToList();

            var exportData = new List<ExportViewModel>();

            foreach (var appointment in appointments)
            {
                exportData.Add(new ExportViewModel
                {
                    PsychologistName = appointment.Psychologist.FirstName + " " + appointment.Psychologist.LastName,
                    AppointmentDate = appointment.AppointmentDate,
                    UserName = db.Users.Find(appointment.UserId).UserName,
                    Rating = appointment.RatingScore
                });
            }

            var csv = new StringBuilder();
            csv.AppendLine("Psychologist,Appointment Date,User,Rating");

            foreach (var item in exportData)
            {
                csv.AppendLine($"{item.PsychologistName},{item.AppointmentDate},{item.UserName},{item.Rating}");
            }

            return File(new System.Text.UTF8Encoding().GetBytes(csv.ToString()), "text/csv", "Appointments.csv");
        }

        public class ExportViewModel
        {
            public string PsychologistName { get; set; }
            public DateTime AppointmentDate { get; set; }
            public string UserName { get; set; }
            public double? Rating { get; set; }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
        private List<string> GetImages()
        {
            var dir = new DirectoryInfo(Server.MapPath("~/Content/Images"));
            var files = dir.GetFiles().Select(f => f.Name).ToList();
            return files;
        }
    }
}
