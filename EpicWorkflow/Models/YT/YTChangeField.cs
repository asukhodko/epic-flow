using System.Collections.Generic;

namespace EpicWorkflow.Models.YT
{
    public class YTChangeField
    {
        public YTChangeField()
        {
            Value = new List<string>();
        }

        public string Name { get; set; }
        public List<string> Value { get; set; }
        public string ValueId { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
    }
}