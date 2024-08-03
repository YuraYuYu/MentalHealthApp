using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
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
            var isAdmin = User.IsInRole("Admin");
            ViewBag.IsAdmin = isAdmin;
            return View(db.Psychologists.ToList());
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
