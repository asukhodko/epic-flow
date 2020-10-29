using System;

namespace EpicWorkflow.Models
{
    public class EpicProgressElement
    {
        public DateTime Updated { get; set; }
        public string Updater { get; set; }
        public double Doneness { get; set; }
        public double Increment { get; set; }
    }
}