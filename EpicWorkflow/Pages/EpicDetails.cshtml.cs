using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EpicWorkflow.Helpers;
using EpicWorkflow.Models;
using EpicWorkflow.Repositories;
using EpicWorkflow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EpicWorkflow.Pages
{
    public class EpicDetailsModel : PageModel
    {
        private readonly IEpicFlowRepository _epicFlowRepository;
        private readonly IYouTrackService _youTrackService;

        public EpicDetailsModel(IYouTrackService youTrackService, IEpicFlowRepository epicFlowRepository)
        {
            _youTrackService = youTrackService;
            _epicFlowRepository = epicFlowRepository;
        }

        public Epic Epic { get; set; }
        public Product CurrentProduct { get; set; }
        public List<UserStoryVM> UserStories { get; set; }

        public DateTime Finished => new[]
        {
            UserStories.Max(p => p.Finished),
            Epic.ExpectedDateAligned ?? DateTime.MinValue,
            Epic.Deadline ?? DateTime.MinValue
        }.Max();

        public DateTime Started => UserStories.Min(p => p.Started);
        public int ScaleMinutes => (int) (Finished - Started).TotalMinutes;

        public async Task OnGetAsync([FromQuery(Name = "id")] int id,
            [FromQuery(Name = "project")] string projectShortName)
        {
            CurrentProduct = await _epicFlowRepository.GetProductAsync(projectShortName);
            Epic = await _youTrackService.GetEpic(id);
            UserStories = (await _youTrackService.GetUserStoriesByEpicIdAsync(id)).Select(p => (UserStoryVM) p)
                .ToList();
            var alignedUs = new List<UserStoryVM>();
            var unalignedUs = new List<UserStoryVM>();
            var epicExpectedDate = Epic.ExpectedDateAligned ?? Epic.Deadline ?? DateTime.Now.AddDays(20);
            foreach (var us in UserStories)
            {
                us.StateChanges = await _youTrackService.GetIssueStateChangesAsync(us.IssueNumber);
                DateTime? started = null;
                if (us.State != State.Open && us.State != State.Frozen)
                    started = us.StateChanges.FirstOrDefault(p => p.State != State.Open &&
                                                                  p.State != State.Frozen &&
                                                                  p.State != State.Done &&
                                                                  p.State != State.Rejected)?.Updated;
                if (started == null && us.State != State.Open &&
                    us.State != State.Frozen &&
                    us.State != State.Done &&
                    us.State != State.Rejected)
                {
                    started = us.CreatedDate;
                }

                if (started == null)
                    started = us.ResolvedDate;
                if (started == null)
                {
                    if (Epic.ExpectedDateAligned.HasValue && Epic.ExpectedDateAligned < DateTime.Now.AddDays(10))
                        started = Epic.ExpectedDateAligned;
                    if (started == null)
                        started = DateTime.Today.AddDays(10);
                    unalignedUs.Add(us);
                }
                else
                {
                    alignedUs.Add(us);
                }

                us.Started = started.Value;
                if (us.ResolvedDate.HasValue)
                {
                    us.Finished = us.ResolvedDate.Value;
                }
                else
                {
                    if (us.EstimationWorkingDays > 0)
                    {
                        var restBase = us.Started;
                        if (restBase < DateTime.Now)
                            restBase = DateTime.Now;
                        var restWorkingDays = us.EstimationWorkingDays * (100 - (us.DonePercent ?? 0)) / 100;
                        var restWeeks = restWorkingDays / 5;
                        us.Finished = restBase.AddDays(restWeeks > 1 ? restWeeks * 7 - 2 : restWeeks * 5);
                    }
                    else
                    {
                        us.Finished = epicExpectedDate;
                    }
                }
            }

            if (!string.IsNullOrEmpty(CurrentProduct.BacklogId))
            {
                var backlog = await _youTrackService.GetBacklogAsync(CurrentProduct.BacklogId);
                var orderedUs = new List<UserStoryVM>();
                foreach (var e in backlog)
                {
                    var us = unalignedUs.FirstOrDefault(p => p.IssueNumber == e.IssueNumber);
                    if (us != null)
                    {
                        orderedUs.Add(us);
                        unalignedUs.Remove(us);
                    }
                }

                orderedUs.AddRange(unalignedUs);
                unalignedUs = orderedUs;
            }

            var nextMonday = DateTime.Today.NextDayOfWeek(DayOfWeek.Monday);
            var weeksRest = Math.Floor((epicExpectedDate - nextMonday).TotalDays / 7);
            while (unalignedUs.Any())
            {
                var inProgressPoints = alignedUs
                    .Where(p => p.Started < nextMonday && p.Finished > nextMonday)
                    .Sum(p => p.PointsAdjusted * (p.Finished - nextMonday) / (p.Finished - p.Started));
                var pointsRest = inProgressPoints + unalignedUs.Sum(p => p.PointsAdjusted);
                var pointPerWeek = pointsRest / Math.Max(weeksRest, 1);

                var freePoints = pointPerWeek;
                freePoints -= inProgressPoints;
                while (freePoints > 0 && unalignedUs.Any())
                {
                    var us = unalignedUs[0];
                    unalignedUs.RemoveAt(0);
                    alignedUs.Add(us);
                    freePoints -= us.PointsAdjusted;
                    var delta = us.Started - nextMonday;
                    us.Started -= delta;
                    us.Finished -= delta;
                }

                nextMonday += TimeSpan.FromDays(7);
                weeksRest--;
            }

            UserStories = UserStories.OrderByDescending(p =>
            {
                switch (p.State)
                {
                    case State.Rejected:
                        return 1;
                    case State.Done:
                        return 2;
                    default:
                        return 3;
                }
            }).ThenByDescending(us => us.Started).ToList();
        }

        public class UserStoryVM
        {
            public string Summary { get; set; }
            public string IssueNumber { get; set; }
            public string Url { get; set; }
            public State State { get; set; }
            public double Points { get; set; }
            public double PointsAdjusted => Points == 0 ? 1 : Points;
            public IEnumerable<IssueStateChange> StateChanges { get; set; }
            public DateTime Started { get; set; }
            public DateTime Finished { get; set; }
            public DateTime CreatedDate { get; set; }
            public DateTime? ResolvedDate { get; set; }
            public string ProjectShortName { get; set; }
            public double? DonePercent { get; set; }
            public double EstimationWorkingDays { get; set; }

            public static explicit operator UserStoryVM(UserStory us)
            {
                return new UserStoryVM
                {
                    CreatedDate = us.CreatedDate,
                    Points = us.StoryPoints,
                    State = us.State,
                    Url = us.Url,
                    IssueNumber = us.IssueNumber,
                    Summary = us.Summary,
                    ResolvedDate = us.ResolvedDate,
                    ProjectShortName = us.Project.ShortName,
                    DonePercent = us.DonePercent,
                    EstimationWorkingDays = us.EstimationWorkingDays
                };
            }
        }
    }
}