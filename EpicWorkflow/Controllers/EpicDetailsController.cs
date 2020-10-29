using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EpicWorkflow.Models;
using EpicWorkflow.Services;
using Microsoft.AspNetCore.Mvc;

namespace EpicWorkflow.Controllers
{
    [Route("api/v1/EpicDetails")]
    public class EpicDetailsController : Controller
    {
        private readonly IYouTrackService _youTrackService;

        public EpicDetailsController(IYouTrackService youTrackService)
        {
            _youTrackService = youTrackService;
        }

        [Route("chart/{epicId}")]
        public async Task<IActionResult> Index(int epicId)
        {
            var epic = await _youTrackService.GetEpic(epicId);
            var epicProgress = await _youTrackService.GetEpicProgressAsync(epicId);

            var labels = GetTimeLabels(epicProgress);

            var contributions = await _youTrackService.GetEpicContributionsAsync(epicId);

            return Json(new
            {
                Labels = labels,
                Doneness = GetDonenessDataset(epicProgress),
                Expected = GetExpectedDataset(epic, labels),
                Deadline = GetDeadlineDataset(epic, labels),
                Today = GetTodayDataset(),
                Contributors = ContributorVM.Create(contributions),
                ContributorsWithTime = ContributorWithTimeVM.Create(contributions),
            });
        }

        private static IEnumerable<object> GetDonenessDataset(IEnumerable<EpicProgressElement> epicProgress)
        {
            var doneness = epicProgress.Select(p => new
            {
                X = p.Updated,
                Y = p.Doneness,
                p.Updater,
                p.Increment
            });
            return doneness;
        }

        private static List<DateTime> GetTimeLabels(IEnumerable<EpicProgressElement> epicProgress)
        {
            List<DateTime> labels;
            if (epicProgress.Any())
            {
                labels = new List<DateTime>
                {
                    epicProgress.Min(p => p.Updated),
                    epicProgress.Max(p => p.Updated)
                };
            }
            else
            {
                labels = new List<DateTime>();
            }

            return labels;
        }

        private static List<object> GetTodayDataset()
        {
            var today = new List<object>
            {
                new
                {
                    X = DateTime.Now,
                    Y = 0
                },
                new
                {
                    X = DateTime.Now,
                    Y = 100
                }
            };
            return today;
        }

        private static List<object> GetExpectedDataset(Epic epic, List<DateTime> labels)
        {
            var expected = new List<object>();
            if (epic.ExpectedDateAligned.HasValue)
            {
                labels.Add(epic.ExpectedDateAligned.Value);
                expected.Add(new
                {
                    X = labels.First(),
                    Y = 0
                });
                expected.Add(new
                {
                    X = epic.ExpectedDateAligned,
                    Y = 100
                });
            }

            return expected;
        }

        private static List<object> GetDeadlineDataset(Epic epic, List<DateTime> labels)
        {
            var deadline = new List<object>();
            if (epic.Deadline != null)
            {
                labels.Add(epic.Deadline.Value);
                deadline.Add(new
                {
                    X = epic.Deadline,
                    Y = 0
                });
                deadline.Add(new
                {
                    X = epic.Deadline,
                    Y = 100
                });
            }

            return deadline;
        }
    }
}