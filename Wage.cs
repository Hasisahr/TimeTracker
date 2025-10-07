using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimeTracker.Model
{
    public class Wage
    {
        [Key]
        public int Id { get; set; }
        public Position Position { get; set; }
        public decimal HourlyRate { get; set; }
        public decimal OvertimeRate { get; set; }

        public Wage(Position position)
        {
            Position = position;
            switch (position) {
                case Position.Garderoba: 
                    HourlyRate = 6.06M;
                    OvertimeRate = HourlyRate * 1.5M;
                    break;
                case Position.Ulaz:
                    HourlyRate = 6.50M;
                    OvertimeRate = HourlyRate * 1.5M;
                    break;
                case Position.Dvorana:
                    HourlyRate = 7M;
                    OvertimeRate = HourlyRate * 1.5M;
                    break;
                case Position.Programi:
                    HourlyRate = 6.10M;
                    OvertimeRate = HourlyRate * 1.5M;
                    break;

            }
        }
    }
}
