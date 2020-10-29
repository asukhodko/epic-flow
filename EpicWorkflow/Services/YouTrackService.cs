using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using EpicWorkflow.Helpers;
using EpicWorkflow.Models;
using EpicWorkflow.Models.YT;
using EpicWorkflow.Repositories;

namespace EpicWorkflow.Services
{
    public class YouTrackService : IYouTrackService
    {
        private readonly IYouTrackRepository _youTrackRepository;

        public YouTrackService(IYouTrackRepository youTrackRepository)
        {
            _youTrackRepository = youTrackRepository;
        }

        public async Task<Epic> GetEpic(int epicId)
        {
            var issues =
                await _youTrackRepository.GetIssuesAsync(
                    $"#CDT-{epicId}");

            return issues.Select(issue => new Epic(issue)).FirstOrDefault();
        }

        public async Task<IEnumerable<Epic>> GetEpicsInprogressAsync(string productName)
        {
            var issues =
                await _youTrackRepository.GetIssuesAsync(
                    $"project: CDT Type: Epic State: Frozen, {{In Progress}}, Testing Products: {{{productName}}}");

            return issues.Select(issue => new Epic(issue)).ToList();
        }

        public async Task<IEnumerable<UserStory>> GetUserStoriesByEpicsAsync(int[] epicsNumbers)
        {
            if (!epicsNumbers.Any())
            {
                return new List<UserStory>();
            }

            var subtasks = string.Join(",", epicsNumbers.Select(n => $"CDT-{n}"));
            var issues =
                await _youTrackRepository.GetIssuesAsync(
                    $"Subtask of: {subtasks} Type: {{User Story}}");

            return issues.Select(issue => new UserStory(issue)).ToList();
        }

        public async Task<IEnumerable<UserStory>> GetUserStoriesByEpicIdAsync(int epicId)
        {
            var issues =
                await _youTrackRepository.GetIssuesAsync(
                    $"Subtask of: CDT-{epicId} Type: {{User Story}}");

            return issues.Select(issue => new UserStory(issue));
        }

        public async Task<IEnumerable<Contribution>> GetEpicContributionsAsync(int epicId)
        {
            var result = new List<Contribution>();
            var userStories = await GetUserStoriesByEpicIdAsync(epicId);
            var usIssueNumbers = String.Join(", ", userStories.Select(p => p.IssueNumber));

            var issues = await _youTrackRepository.GetIssuesAsync(
                $"(Subtask of: {usIssueNumbers} State: Done) or (Subtask of: CDT-{epicId} State: Done has: -{{Parent for}})");

            result.AddRange(issues.SelectMany(p => IssueBase.CreateFromYTIssue(p).GetContributions()));

            return result;
        }

        public async Task<IEnumerable<EpicProgressElement>> GetEpicProgressAsync(int epicId)
        {
            var result = new List<EpicProgressElement>();
            var ytChanges = await _youTrackRepository.GetIssueHistoryAsync($"CDT-{epicId}");
            foreach (var change in ytChanges)
            {
                var doneness = change.Fileds.FirstOrDefault(p => p.Name == "% готово");
                if (doneness != null)
                {
                    var donenessValue = doneness.Value.FirstOrDefault() ?? doneness.NewValue;
                    var donenessOldValue = doneness.OldValue;
                    double donenessParsed;
                    double.TryParse(donenessValue, NumberStyles.Float, CultureInfo.InvariantCulture,
                        out donenessParsed);
                    double donenessOldParsed;
                    double.TryParse(donenessOldValue, NumberStyles.Float, CultureInfo.InvariantCulture,
                        out donenessOldParsed);
                    result.Add(new EpicProgressElement
                    {
                        Doneness = donenessParsed,
                        Increment = donenessParsed - donenessOldParsed,
                        Updater = change.Fileds.FirstOrDefault(p => p.Name == "updaterName").Value.First(),
                        Updated = change.Fileds.FirstOrDefault(p => p.Name == "updated").Value.First().ParseYTDate(),
                    });
                }
            }

            result = result.OrderBy(p => p.Updated).ToList();
            return result;
        }

        public async Task<IEnumerable<IssueStateChange>> GetIssueStateChangesAsync(string issueNumber)
        {
            var result = new List<IssueStateChange>();
            var ytChanges = await _youTrackRepository.GetIssueHistoryAsync(issueNumber);
            foreach (var change in ytChanges)
            {
                var state = change.Fileds.FirstOrDefault(p => p.Name == "State");
                if (state != null)
                {
                    var stateValue = state.Value.FirstOrDefault() ?? state.NewValue;
                    var stateParsed = EnumUtil.Parse<State>(stateValue.Replace(" ", ""));
                    result.Add(new IssueStateChange
                    {
                        State = stateParsed,
                        Updated = YTDataUtil.ParseYTDate(change.Fileds.FirstOrDefault(p => p.Name == "updated").Value
                            .First()),
                    });
                }
            }

            result = result.OrderBy(p => p.Updated).ToList();
            return result;
        }

        public async Task<IEnumerable<YTBacklogElement>> GetBacklogAsync(string savedSearchId)
        {
            return await _youTrackRepository.GetBacklogAsync(savedSearchId);
        }
    }
}