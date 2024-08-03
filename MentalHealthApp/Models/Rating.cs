using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MentalHealthApp.Models
{
    public class Rating
    {
        public int Id { get; set; }
        public int PsychologistId { get; set; }
        public string UserId { get; set; }
        public int Score { get; set; }
        public DateTime DateRated { get; set; }

        public virtual Psychologist Psychologist { get; set; }
        public virtual ApplicationUser User { get; set; }
    }

}