using System;
using System.ComponentModel.DataAnnotations;

namespace WeddingPlanner.Models
{
    public class Attendance
    {
        [Key]

        public int AttendanceId { get; set; }

        public int UserId { get; set; }

        public int WeddingId { get; set; }

        public User User { get; set; }

        public Wedding Wedding { get; set; }
    }
}