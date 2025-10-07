using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TimeTracker.Model
{
    public class ScheduleAssignment
    {
        public int Id { get; set; }

        public int ScheduleId { get; set; }
        public Schedule Schedule { get; set; }

        public string WorkerId { get; set; }
        public IdentityUser Worker { get; set; }

        public string Position { get; set; }
        public int SlotNumber { get; set; }
    }
}
