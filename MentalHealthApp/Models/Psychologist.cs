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
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        public string Specialization { get; set; }

        [Display(Name = "Photo")]
        public string Photo { get; set; }
        public virtual ICollection<Appointment> Appointments { get; set; }

        // Add FullName property
        [NotMapped]
        public string FullName
        {
            get { return FirstName + " " + LastName; }
        }

        public double AverageRating { get; set; }
    }
}