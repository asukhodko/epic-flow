using System;
using System.Collections.Generic;
using System.Linq;

namespace EpicWorkflow.Models
{
    public class ContributorWithTimeVM
    {
        private const int SmoothingDays = 2;

        public string Contributor { get; set; }
        public List<ContributionVM> Contributions { get; set; }

        public void InsertMissingDates()
        {
            if (Contributions.Count == 0)
                return;

            var newContributions = new List<ContributionVM>();

            var fromDate = Contributions.First().Date.AddDays(-1);
            var toDate = Contributions.Last().Date.AddDays(1);
            if (toDate > DateTime.Today)
                toDate = DateTime.Today;

            var i = 0;
            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
            {
                var c = Contributions[i];
                if (c.Date == d)
                {
                    newContributions.Add(c);
                    if (i < Contributions.Count - 1)
                        i++;
                }
                else
                {
                    newContributions.Add(new ContributionVM
                    {
                        Date = d
                    });
                }
            }

            Contributions = newContributions;
        }

        public void SmoothOnDays()
        {
            if (Contributions.Count == 0)
                return;

            var newContributions = new List<ContributionVM>();

            var firstDate = Contributions.First().Date;
            for (var i = 0; i < SmoothingDays; i++)
            {
                firstDate = firstDate.AddDays(-1);
                Contributions.Insert(0, new ContributionVM
                {
                    Date = firstDate
                });
            }

            for (var i = 0; i < Contributions.Count; i++)
            {
                var sum = 0d;
                for (var j = 0; j < SmoothingDays; j++)
                {
                    var x = i + j;
                    if (x < Contributions.Count)
                    {
                        sum += Contributions[x].ImpactValue;
                    }
                }

                newContributions.Add(new ContributionVM
                {
                    Date = Contributions[i].Date,
                    ImpactValue = sum / SmoothingDays
                });
            }

            Contributions = newContributions;
        }

        public static IEnumerable<ContributorWithTimeVM> Create(IEnumerable<Contribution> contributions)
        {
            return contributions.ToContributorWithTimeVMs()
                .AddTotalContributorVM()
                .InsertMissingDates()
                .SmoothOnDays();
        }
    }

    public static class ContributorWithTimeVMHelper
    {
        private const string UnknownContributorLabel = "[инкогнито (unassigned)]";
        private const string TotalContributorsLabel = "[всего]";

        public static List<ContributorWithTimeVM> ToContributorWithTimeVMs(
            this IEnumerable<Contribution> contributions)
        {
            return contributions.GroupBy(p => p.ContributorName)
                .Select(p => new ContributorWithTimeVM
                {
                    Contributor = p.Key ?? UnknownContributorLabel,
                    Contributions = p.GroupBy(x => x.Date.Date)
                        .Select(x => new ContributionVM
                        {
                            Date = x.Key,
                            ImpactValue = x.Sum(y => y.ImpactValue)
                        })
                        .OrderBy(x => x.Date)
                        .ToList()
                })
                .OrderByDescending(p => p.Contributions.Sum(q => q.ImpactValue))
                .ToList();
        }

        public static IEnumerable<ContributorWithTimeVM> AddTotalContributorVM(
            this IEnumerable<ContributorWithTimeVM> cwts)
        {
            return cwts.Append(CreateTotalContributorVM(cwts));
        }

        public static IEnumerable<ContributorWithTimeVM> InsertMissingDates(
            this IEnumerable<ContributorWithTimeVM> cwts)
        {
            foreach (var cwt in cwts)
            {
                cwt.InsertMissingDates();
            }

            return cwts;
        }

        public static IEnumerable<ContributorWithTimeVM> SmoothOnDays(
            this IEnumerable<ContributorWithTimeVM> cwts)
        {
            foreach (var cwt in cwts)
            {
                cwt.SmoothOnDays();
            }

            return cwts;
        }

        private static ContributorWithTimeVM CreateTotalContributorVM(IEnumerable<ContributorWithTimeVM> cwts)
        {
            return new ContributorWithTimeVM
            {
                Contributor = TotalContributorsLabel,
                Contributions = cwts
                    .SelectMany(p => p.Contributions)
                    .GroupBy(p => p.Date)
                    .Select(p => new ContributionVM
                    {
                        Date = p.Key,
                        ImpactValue = p.Sum(q => q.ImpactValue)
                    })
                    .OrderBy(p => p.Date)
                    .ToList()
            };
        }
    }
}