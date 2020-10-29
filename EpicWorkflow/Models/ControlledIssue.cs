using System.Collections.Generic;
using System.Linq;
using EpicWorkflow.Models.YT;
using Newtonsoft.Json.Linq;

namespace EpicWorkflow.Models
{
    public class ControlledIssue : IssueBase
    {
        private const double DaysPerStoryPoint = 5;
        private const double DefaultStoryPoints = 1;

        private const double
            TaskPointsPerStoryPoint =
                29; // https://git.example.ru/portals/docs/wikis/Организация-работы/Корреляция-оценок-пользовательских-историй-и-тасков


        public ControlledIssue(YTIssue ytIssue) : base(ytIssue)
        {
            var value = ytIssue.FieldsDictionary.GetValueOrDefault("% готово");
            if (value != null && value is double) DonePercent = value;

            value = ytIssue.FieldsDictionary.GetValueOrDefault("Story points") ??
                    ytIssue.FieldsDictionary.GetValueOrDefault("Points");
            if (value != null && value is double) StoryPoints = value;

            value = ytIssue.FieldsDictionary.GetValueOrDefault("Customer");
            if (value != null) Customer = ((JObject) value).GetValue("name").ToString();
        }

        /// % готово
        public double? DonePercent { get; set; }

        /// Story points
        public double StoryPoints { get; set; }

        /// Customer
        public string Customer { get; set; }

        public double EstimationWorkingDays => (StoryPoints > 0 ? StoryPoints : DefaultStoryPoints) * DaysPerStoryPoint;

        public int EstimationMinutes => (int) (EstimationWorkingDays * 8 * 60);

        public override List<Contribution> GetContributions()
        {
            List<string> contributors;
            contributors = (Performers?.Count ?? 0) != 0 ? Performers : new List<string> {Assignee};

            return contributors.Select(p => new Contribution
            {
                ContributorName = p,
                ImpactValue = (StoryPoints == 0 ? DefaultStoryPoints : StoryPoints) * TaskPointsPerStoryPoint /
                              contributors.Count,
                Date = ResolvedDate.GetValueOrDefault()
            }).ToList();
        }
    }
}