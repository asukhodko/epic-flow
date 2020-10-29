using System.Collections.Generic;
using System.Threading.Tasks;
using EpicWorkflow.Models;
using EpicWorkflow.Models.YT;

namespace EpicWorkflow.Services
{
    public interface IYouTrackService
    {
        Task<Epic> GetEpic(int epicId);
        Task<IEnumerable<Epic>> GetEpicsInprogressAsync(string productName);
        Task<IEnumerable<UserStory>> GetUserStoriesByEpicsAsync(int[] epicsNumbers);
        Task<IEnumerable<UserStory>> GetUserStoriesByEpicIdAsync(int epicId);
        Task<IEnumerable<Contribution>> GetEpicContributionsAsync(int epicId);
        Task<IEnumerable<EpicProgressElement>> GetEpicProgressAsync(int epicId);
        Task<IEnumerable<IssueStateChange>> GetIssueStateChangesAsync(string issueNumber);
        Task<IEnumerable<YTBacklogElement>> GetBacklogAsync(string savedSearchId);
    }
}