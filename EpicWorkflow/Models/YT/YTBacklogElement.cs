namespace EpicWorkflow.Models.YT
{
    public class YTBacklogElement
    {
        public YTProject Project { get; set; }
        public int NumberInProject { get; set; }
        public string IssueNumber => Project.ShortName + "-" + NumberInProject;
    }
}