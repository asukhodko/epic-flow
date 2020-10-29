using System;
using System.Collections.Generic;
using EpicWorkflow.Helpers;
using EpicWorkflow.Models.YT;
using Newtonsoft.Json.Linq;

namespace EpicWorkflow.Models
{
    public class Epic : ControlledIssue
    {
        private const int EveningHours = 18;

        public Epic(YTIssue ytIssue) : base(ytIssue)
        {
            var value = ytIssue.FieldsDictionary.GetValueOrDefault("Products");
            if (value != null)
            {
                Products = new List<string>();
                foreach (var p in (JArray) value)
                    Products.Add(((JObject) p).GetValue("name").ToString());
            }

            value = ytIssue.FieldsDictionary.GetValueOrDefault("Value");
            if (value != null && value is double) Value = value;

            value = ytIssue.FieldsDictionary.GetValueOrDefault("Value per story point");
            if (value != null && value is double) ValuePerPoint = value;

            value = ytIssue.FieldsDictionary.GetValueOrDefault("ExpectedDate");
            if (value != null) ExpectedDate = YTDataUtil.ParseYTDate(value);

            value = ytIssue.FieldsDictionary.GetValueOrDefault("Deadline");
            if (value != null) Deadline = YTDataUtil.ParseYTDate(value);

            value = ytIssue.FieldsDictionary.GetValueOrDefault("CycleTime");
            if (value != null)
            {
                CycleTime = new Period
                {
                    Minutes = ((JObject) value).GetValue("minutes").ToObject<int>(),
                    Presentation = ((JObject) value).GetValue("presentation").ToString()
                };
            }

            value = ytIssue.FieldsDictionary.GetValueOrDefault("Осталось работы");
            if (value != null)
            {
                EstimatedRestTime = new Period
                {
                    Minutes = ((JObject) value).GetValue("minutes").ToObject<int>(),
                    Presentation = ((JObject) value).GetValue("presentation").ToString()
                };
            }

            value = ytIssue.FieldsDictionary.GetValueOrDefault("% в день");
            if (value != null && value is double) PercentPerDay = value;
        }

        /// <summary>
        /// Продукты
        /// </summary>
        public List<string> Products { get; set; }

        /// <summary>
        /// Ценность
        /// </summary>
        public double? Value { get; set; }

        /// <summary>
        /// Ценность на 1 балл
        /// </summary>
        public double? ValuePerPoint { get; set; }

        /// <summary>
        /// Прогнозная дата завершения
        /// </summary>
        public DateTime? ExpectedDate { get; set; }

        /// <summary>
        ///  Прогнозная дата завершения, выровненная по пятницам
        /// </summary>
        public DateTime? ExpectedDateAligned
        {
            get
            {
                if (!ExpectedDate.HasValue || ExpectedDate.Value.DayOfWeek == DayOfWeek.Friday)
                    return ExpectedDate;
                var nextFriday =
                    ExpectedDate.Value.NextDayOfWeek(DayOfWeek.Friday);
                return nextFriday.Date.AddHours(EveningHours);
            }
        }

        /// <summary>
        /// Deadline
        /// </summary>
        public DateTime? Deadline { get; set; }

        /// <summary>
        /// Осталось работы
        /// </summary>
        public Period EstimatedRestTime { get; set; }

        /// <summary>
        /// Осталось работы, совсем грубо
        /// </summary>
        public Period DirtyEstimatedRestTime =>
            EstimatedRestTime ?? new Period {Presentation = "4w", Minutes = 4 * 5 * 8 * 60};

        /// <summary>
        /// Время, уже потраченное на эпик
        /// </summary>
        public Period CycleTime { get; set; }

        public int TotalMinutes => (CycleTime?.Minutes ?? 0) + DirtyEstimatedRestTime.Minutes;

        /// <summary>
        /// % в день
        /// </summary>
        public double? PercentPerDay { get; set; }

        /// <summary>
        /// Задача на контроле у руководства
        /// </summary>
        public bool IsVip => Tags.Exists(p => p == "VIP");
    }
}