using EpicWorkflow.Models.YT;

namespace EpicWorkflow.Models
{
    public class Bug : WorkIssue
    {
        public Bug(YTIssue ytIssue) : base(ytIssue)
        {
        }
    }
}