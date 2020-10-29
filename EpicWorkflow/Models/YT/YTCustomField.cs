namespace EpicWorkflow.Models.YT
{
    public class YTCustomField
    {
        public YTProjectCustomField ProjectCustomField { get; set; }
        public dynamic Value { get; set; }

        public class YTProjectCustomField
        {
            public YTFieldModel Field { get; set; }

            public class YTFieldModel
            {
                public string Name { get; set; }
            }
        }
    }
}