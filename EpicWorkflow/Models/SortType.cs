using System.ComponentModel;

namespace EpicWorkflow.Models
{
    public enum SortType
    {
        [Description("по сроку")] Estimation,

        [Description("по ценности на сложность")]
        ValueByPoint,

        [Description("по ценности")] Value,
    }
}