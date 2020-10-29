namespace EpicWorkflow.Models
{
    public class Period
    {
        public int Minutes { get; set; }
        public string Presentation { get; set; }

        public override string ToString()
        {
            return Presentation;
        }
    }
}