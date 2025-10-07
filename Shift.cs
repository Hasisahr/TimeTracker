namespace TimeTracker.Model
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc.ModelBinding;

    public class Shift
    {
        [Key]
        public int Id { get; set; }

        public string IdentityUserId { get; set; }


        //[ForeignKey(nameof(IdentityUserId))]
        [BindNever]
        public IdentityUser IdentityUser { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime StartTime { get; set; } 

        [Required]
        [DataType(DataType.Date)]
        public DateTime EndTime { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ShiftDate { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourlyRate { get; set; } 

        [Required]
        public bool IsOvertime { get; set; }

        public decimal Salary { get; set; }

        [Required]
        public Position Position { get; set; }

        public decimal calculateWageForShift()
        {
            decimal totalPay = 0;
            DateTime current = StartTime;
            Wage wage = new Wage(Position);

            if (StartTime.Day.Equals(DayOfWeek.Sunday))
            {
                return (decimal)(EndTime - StartTime).TotalHours * wage.OvertimeRate;
            }
            else { 

                while (current < EndTime)
                {
                    DateTime nextHour = current.AddHours(1);

                    if (current.Hour >= 22)
                    {
                        totalPay += wage.OvertimeRate;
                    }
                    else
                    {
                        totalPay += wage.HourlyRate;
                    }

                    current = nextHour;
                }
            }
            Salary = totalPay;
            return totalPay;

        }
    }
}
