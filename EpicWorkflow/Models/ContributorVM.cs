using System.Collections.Generic;
using System.Linq;

namespace EpicWorkflow.Models
{
    public class ContributorVM
    {
        private const string UnknownContributorLabel = "[инкогнито (unassigned)]";

        public string Contributor { get; set; }
        public double ImpactValue { get; set; }

        public static IEnumerable<ContributorVM> Create(IEnumerable<Contribution> contributions)
        {
            return contributions.GroupBy(p => p.ContributorName)
                .Select(p => new ContributorVM
                {
                    Contributor = p.Key ?? UnknownContributorLabel,
                    ImpactValue = p.Sum(v => v.ImpactValue)
                })
                .OrderByDescending(p => p.ImpactValue);
        }
    }
}