using System;
using System.Collections.Generic;
using System.Linq;
using EpicWorkflow.Helpers;
using EpicWorkflow.Models.YT;
using Newtonsoft.Json.Linq;

namespace EpicWorkflow.Models
{
    public class IssueBase
    {
        public IssueBase(YTIssue ytIssue)
        {
            Id = ytIssue.Id;
            Project = ytIssue.Project;
            NumberInProject = ytIssue.NumberInProject;
            Summary = ytIssue.Summary;
            Tags = ytIssue.Tags.Select(p => p.Name).ToList();
            ParentId = ytIssue.Parent.Issues.FirstOrDefault()?.Id;
            CreatedDate = ytIssue.Created.ParseYTDate();
            if (ytIssue.Resolved != null)
                ResolvedDate = ytIssue.Resolved.Value.ParseYTDate();

            var value = ytIssue.FieldsDictionary.GetValueOrDefault("Type");
            if (value != null) IssueType = YTDataUtil.ParseEnum<IssueType>(value);

            value = ytIssue.FieldsDictionary.GetValueOrDefault("State");
            if (value != null) State = YTDataUtil.ParseEnum<State>(value);

            value = ytIssue.FieldsDictionary.GetValueOrDefault("Assignee");
            if (value != null) Assignee = ((JObject) value).GetValue("name").ToString();

            value = ytIssue.FieldsDictionary.GetValueOrDefault("Performers");
            if (value != null)
            {
                Performers = new List<string>();
                foreach (var p in (JArray) value)
                    Performers.Add(((JObject) p).GetValue("name").ToString());
            }
        }

        public string Id { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? ResolvedDate { get; set; }
        public string ParentId { get; set; }
        public YTProject Project { get; set; }
        public int NumberInProject { get; set; }
        public string IssueNumber => Project.ShortName + "-" + NumberInProject;
        public string Url => $"https://youtrack.example.ru/issue/{IssueNumber}";
        public IssueType IssueType { get; set; }
        public string Assignee { get; set; }
        public string Summary { get; set; }
        public State State { get; set; }
        public List<string> Tags { get; set; }
        public List<string> Performers { get; set; }


        public bool IsBelongFor(Product product)
        {
            return Project.ShortName.Equals(product.ProjectShortName, StringComparison.InvariantCultureIgnoreCase);
        }

        public virtual List<Contribution> GetContributions()
        {
            throw new NotSupportedException();
        }

        public static IssueBase CreateFromYTIssue(YTIssue issue)
        {
            IssueType issueType = IssueType.Unknown;
            var v = issue.FieldsDictionary.GetValueOrDefault("Type");
            if (v != null)
                issueType = YTDataUtil.ParseEnum<IssueType>(v);

            switch (issueType)
            {
                case IssueType.Epic:
                    return new Epic(issue);

                case IssueType.UserStory:
                    return new UserStory(issue);

                case IssueType.Task:
                case IssueType.Bug:
                    return new TaskIssue(issue);

                default:
                    return new IssueBase(issue);
            }
        }
    }
}