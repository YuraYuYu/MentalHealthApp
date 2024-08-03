using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace MentalHealthApp.Models
{
    public class Psychologist
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Specialization { get; set; }

        public double Rating { get; set; }

        public string Photo { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }

        // Add FullName property
        [NotMapped]
        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }
    }
}