using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MentalHealthApp.Models
{
    public class PsychologistViewModel
    {
        public int Id { get; set; }

        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Specialization { get; set; }
        public string Photo { get; set; }

        [Display(Name = "Average Rating")]
        public double AverageRating { get; set; }
    }

}