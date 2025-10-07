using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class Salary
    {
        [Key]
        public int Id { get; set; }

        public string IdentityUserId { get; set; } 

        public int Month { get; set; }

        public int Year {  get; set; }

        public List<Shift> Shifts { get; set; }

        public decimal TotalSalary { get; set; }

    }
}
