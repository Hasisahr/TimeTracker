using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Model
{
    public class Schedule
    {
        public int Id { get; set; }

        [Required]
        public DateTime ScheduleDate { get; set; }

        [Required]
        public TimeSpan StartTime { get; set; }

        [Required]
        public TimeSpan EndTime { get; set; }

        [Required]
        [Range(0, 400)]
        public int TicketsSold { get; set; }
        
        public List<ScheduleAssignment> Assignments { get; set; } = new();
    }
}
