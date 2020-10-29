using EpicWorkflow.Models.YT;

namespace EpicWorkflow.Models
{
    public class TaskIssue : WorkIssue
    {
        public TaskIssue(YTIssue ytIssue) : base(ytIssue)
        {
        }
    }
}