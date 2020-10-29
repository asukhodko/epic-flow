using EpicWorkflow.Models.YT;

namespace EpicWorkflow.Models
{
    public class UserStory : ControlledIssue
    {
        public UserStory(YTIssue ytIssue) : base(ytIssue)
        {
        }
    }
}