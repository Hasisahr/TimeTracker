using TimeTracker.Model;

namespace TimeTracker.Web.Models
{
    public class ShiftFilterModel
    {
        public DateTime? Date { get; set; }
        public DateTime? FromTime { get; set; }
        public DateTime? ToTime { get; set; }
        public Position? Position { get; set; }

    }
}

