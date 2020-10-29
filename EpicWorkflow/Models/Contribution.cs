using System;

namespace EpicWorkflow.Models
{
    public class Contribution
    {
        public string ContributorName { get; set; }
        public double ImpactValue { get; set; }
        public DateTime Date { get; set; }
    }
}