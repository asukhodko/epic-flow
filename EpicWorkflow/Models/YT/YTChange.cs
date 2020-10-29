using System.Collections.Generic;

namespace EpicWorkflow.Models.YT
{
    public class YTChange
    {
        public YTChange()
        {
            Fileds = new List<YTChangeField>();
        }

        public List<YTChangeField> Fileds { get; set; }
    }
}