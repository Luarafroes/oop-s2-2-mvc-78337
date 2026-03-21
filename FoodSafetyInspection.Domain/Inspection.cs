using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodSafetyInspection.Domain
{
    public class Inspection
    {
        public int Id { get; set; }
        public int PremisesId { get; set; }
        public DateTime InspectionDate { get; set; }
        public int Score { get; set; } // 0-100
        public string Outcome { get; set; } = string.Empty;
        public string Notes { get; set; } = string.Empty;
        public Premises Premises { get; set; } = null!;
        public ICollection<FollowUp> FollowUps { get; set; } = new List<FollowUp>();
    }
}
