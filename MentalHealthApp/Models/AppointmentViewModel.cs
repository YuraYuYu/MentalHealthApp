using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MentalHealthApp.Models
{
    public class AppointmentViewModel
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Psychologist")]
        public int PsychologistId { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> Psychologists { get; set; }

        [Required]
        [Display(Name = "User")]
        public string UserId { get; set; }

        public IEnumerable<System.Web.Mvc.SelectListItem> Users { get; set; }

        [Required]
        [Display(Name = "Appointment Date")]
        public DateTime AppointmentDate { get; set; }

        public List<TimeSlotViewModel> AvailableTimeSlots { get; set; }

        public class TimeSlotViewModel
        {
            public DateTime Date { get; set; }
            public string Time { get; set; }
            public bool IsAvailable { get; set; }

            public string Display
            {
                get { return $"{Date.ToShortDateString()} {Time}"; }
            }
            public DateTime StartDateTime
            {
                get
                {
                    var date = Date.Date;
                    var timeParts = Time.Split('-');
                    var startTimeParts = timeParts[0].Trim().Split(':');
                    date = date.AddHours(int.Parse(startTimeParts[0])).AddMinutes(int.Parse(startTimeParts[1]));
                    return date;
                }
            }
        }
    }
}