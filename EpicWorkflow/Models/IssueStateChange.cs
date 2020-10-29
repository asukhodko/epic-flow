using System;

namespace EpicWorkflow.Models
{
    public class IssueStateChange
    {
        public DateTime Updated { get; set; }
        public State State { get; set; }
    }
}