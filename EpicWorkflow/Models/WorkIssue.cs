using System.Collections.Generic;
using System.Linq;
using EpicWorkflow.Models.YT;

namespace EpicWorkflow.Models
{
    public class WorkIssue : IssueBase
    {
        private const double DefaultTaskPoints = 3;

        public WorkIssue(YTIssue ytIssue) : base(ytIssue)
        {
            var value = ytIssue.FieldsDictionary.GetValueOrDefault("Task points") ??
                        ytIssue.FieldsDictionary.GetValueOrDefault("Points");
            if (value != null && value is double) Points = value;
        }

        public double Points { get; set; }

        public override List<Contribution> GetContributions()
        {
            List<string> contributors;
            contributors = (Performers?.Count ?? 0) != 0 ? Performers : new List<string> {Assignee};

            return contributors.Select(p => new Contribution
            {
                ContributorName = p,
                ImpactValue = Points == 0 ? DefaultTaskPoints : Points / contributors.Count,
                Date = ResolvedDate.GetValueOrDefault()
            }).ToList();
        }
    }
}