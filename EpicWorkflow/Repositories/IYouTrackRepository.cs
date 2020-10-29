using System.Collections.Generic;
using System.Threading.Tasks;
using EpicWorkflow.Models.YT;

namespace EpicWorkflow.Repositories
{
    public interface IYouTrackRepository
    {
        Task<IEnumerable<YTIssue>> GetIssuesAsync(string query);
        Task<IEnumerable<YTChange>> GetIssueHistoryAsync(string issueNumber);
        Task<IEnumerable<YTBacklogElement>> GetBacklogAsync(string savedSearchId);
    }
}