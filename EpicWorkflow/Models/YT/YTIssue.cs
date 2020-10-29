using System.Collections.Generic;

namespace EpicWorkflow.Models.YT
{
    public class YTIssue
    {
        private Dictionary<string, dynamic> _fieldsDictionary;
        public string Id { get; set; }
        public long Created { get; set; }
        public long? Resolved { get; set; }
        public YTProject Project { get; set; }
        public int NumberInProject { get; set; }
        public string Summary { get; set; }
        public YTParent Parent { get; set; }
        public List<YTCustomField> Fields { get; set; }

        public Dictionary<string, dynamic> FieldsDictionary
        {
            get
            {
                if (_fieldsDictionary == null)
                {
                    _fieldsDictionary = new Dictionary<string, dynamic>();
                    foreach (var field in Fields)
                    {
                        _fieldsDictionary[field.ProjectCustomField.Field.Name] = field.Value;
                    }
                }

                return _fieldsDictionary;
            }
        }

        public List<YTIssueTag> Tags { get; set; }
    }
}