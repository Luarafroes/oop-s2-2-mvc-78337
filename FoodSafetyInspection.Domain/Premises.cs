using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodSafetyInspection.Domain
{
    public class Premises
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Town { get; set; } = string.Empty;
        public string RiskRating { get; set; } = string.Empty;
        public ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();
    }
}