using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MentalHealthApp.Models
{
    public class Appointment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PsychologistId { get; set; }

        [ForeignKey("PsychologistId")]
        public virtual Psychologist Psychologist { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        public DateTime AppointmentDate { get; set; }

        public bool CanRate => AppointmentDate < DateTime.Now && !RatingScore.HasValue;
        public bool HasRated => RatingScore.HasValue;

        [Range(0, 5)]
        public double? RatingScore { get; set; }


    }
}