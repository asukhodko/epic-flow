using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EpicWorkflow.Models;
using EpicWorkflow.Repositories;
using EpicWorkflow.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace EpicWorkflow.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IEpicFlowRepository _epicFlowRepository;
        private readonly IYouTrackService _youTrackService;

        public IndexModel(IYouTrackService youTrackService, IEpicFlowRepository epicFlowRepository)
        {
            _youTrackService = youTrackService;
            _epicFlowRepository = epicFlowRepository;
        }

        public IEnumerable<EpicVM> EpicsInProgress { get; set; }
        public int MaxInProgressMinutes { get; set; }
        public int MaxRestMinutes { get; set; }
        public int ScaleMinutes => MaxInProgressMinutes + MaxRestMinutes;
        public Product CurrentProduct { get; set; }
        public SortType CurrentSort { get; set; }
        public bool VipFirst { get; set; }
        public int RemainingTeamMinutes { get; set; }
        public IEnumerable<Product> Products { get; set; }

        public async Task OnGetAsync([FromQuery(Name = "project")] string projectShortName,
            SortType sort,
            bool vipFirst = true)
        {
            Products = await _epicFlowRepository.GetProductsAsync();
            CurrentProduct = await _epicFlowRepository.GetProductAsync(projectShortName);
            if (CurrentProduct == null)
            {
                CurrentProduct = Products.First();
            }

            VipFirst = vipFirst;
            CurrentSort = sort;

            var epics = await _youTrackService.GetEpicsInprogressAsync(CurrentProduct.Name);
            var epicsUserStories =
                await _youTrackService.GetUserStoriesByEpicsAsync(epics.Select(e => e.NumberInProject).ToArray());

            EpicsInProgress = epics.Select(e =>
            {
                var epicVM = (EpicVM) e;
                epicVM.SetUserStories(epicsUserStories);
                return epicVM;
            }).Where(epicVM => epicVM.WillFutureWorkWith(CurrentProduct)).ToList();

            if (EpicsInProgress.Any())
            {
                MaxInProgressMinutes = EpicsInProgress.Max(p => p.CycleTime?.Minutes ?? 0);
                MaxRestMinutes = EpicsInProgress.Max(p => p.DirtyEstimatedRestTime?.Minutes ?? 0);
            }

            var orderedEpics = EpicsInProgress.OrderByDescending(p => VipFirst && p.IsVip);
            switch (CurrentSort)
            {
                case SortType.Estimation:
                    orderedEpics = orderedEpics.ThenByDescending(p => p.State)
                        .ThenBy(p => p.EstimatedRestTime?.Minutes ?? int.MaxValue);
                    break;
                case SortType.ValueByPoint:
                    orderedEpics =
                        orderedEpics.ThenByDescending(p => (p.Value ?? -1) / (p.StoryPoints ?? double.MaxValue));
                    break;
                case SortType.Value:
                    orderedEpics = orderedEpics.ThenByDescending(p => p.Value ?? -1);
                    break;
            }

            EpicsInProgress = orderedEpics;
        }

        public class EpicVM
        {
            public string Id { get; set; }
            public int NumberInProject { get; set; }
            public string Url { get; set; }
            public string Summary { get; set; }
            public State State { get; set; }

            /// % готово
            public double? DonePercent { get; set; }

            /// Story points
            public double? StoryPoints { get; set; }

            /// Customer
            public string Customer { get; set; }

            /// Value
            public double? Value { get; set; }

            /// Дедлайн
            public DateTime? Deadline { get; set; }

            /// Осталось работы
            public Period EstimatedRestTime { get; set; }

            /// Осталось работы совсем грубо
            public Period DirtyEstimatedRestTime { get; set; }

            /// CycleTime
            public Period CycleTime { get; set; }

            public int TotalMinutes { get; set; }

            /// Задача на контроле у руководства
            public bool IsVip { get; set; }

            public List<UserStory> UserStories { get; set; }
            public List<UserStory> DoneUserStories { get; set; }
            public List<UserStory> FutureUserStories { get; set; }
            public List<UserStory> LastWeekResolvedUserStories { get; set; }

            public int AproxValue { get; set; }

            public bool WillFutureWorkWith(Product product)
            {
                return FutureUserStories.Any(us => us.IsBelongFor(product));
            }

            public bool IsParentFor(UserStory us)
            {
                return us.ParentId == Id;
            }

            public void SetUserStories(IEnumerable<UserStory> allUserStories)
            {
                UserStories = allUserStories.Where(us => IsParentFor(us)).ToList();
                DoneUserStories = UserStories.Where(us => us.State == State.Done).ToList();
                FutureUserStories = UserStories.Where(us => us.State != State.Done && us.State != State.Rejected)
                    .ToList();
                LastWeekResolvedUserStories =
                    DoneUserStories.Where(us => us.ResolvedDate > DateTime.Now.AddDays(-7)).ToList();
            }

            public double GetEstimationDays()
            {
                var estimationDays = FutureUserStories.Sum(us => us.EstimationWorkingDays);
                return estimationDays;
            }

            public double GetLastWeekWorkDays()
            {
                var estimationDays = LastWeekResolvedUserStories.Sum(us => us.EstimationWorkingDays);
                return estimationDays;
            }

            public double GetEstimationDaysBy(Product product)
            {
                var uses = FutureUserStories.Where(us => us.IsBelongFor(product)).ToList();
                var estimationDays = uses.Sum(us => us.EstimationWorkingDays);
                return estimationDays;
            }

            public static explicit operator EpicVM(Epic epic)
            {
                var epicVM = new EpicVM
                {
                    Id = epic.Id,
                    NumberInProject = epic.NumberInProject,
                    Url = epic.Url,
                    Summary = epic.Summary,
                    State = epic.State,
                    DonePercent = epic.DonePercent,
                    StoryPoints = epic.StoryPoints,
                    Customer = epic.Customer,
                    Value = epic.Value,
                    Deadline = epic.Deadline,
                    EstimatedRestTime = epic.EstimatedRestTime,
                    DirtyEstimatedRestTime = epic.DirtyEstimatedRestTime,
                    CycleTime = epic.CycleTime,
                    TotalMinutes = epic.TotalMinutes,
                    IsVip = epic.IsVip
                };

                if (epicVM.Value != null)
                {
                    epicVM.AproxValue = (int) epicVM.Value.Value;
                    if (epicVM.AproxValue >= 0 && epicVM.AproxValue < 3)
                    {
                        epicVM.AproxValue = 1;
                    }

                    if (epicVM.AproxValue >= 3 && epicVM.AproxValue < 8)
                    {
                        epicVM.AproxValue = 2;
                    }

                    if (epicVM.AproxValue >= 8)
                    {
                        epicVM.AproxValue = 3;
                    }
                }

                return epicVM;
            }
        }
    }
}