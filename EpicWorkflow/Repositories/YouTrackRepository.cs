using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using EpicWorkflow.Models.YT;
using Newtonsoft.Json.Linq;

namespace EpicWorkflow.Repositories
{
    public class YouTrackRepository : IYouTrackRepository
    {
        private const string _token = "not-here-ha-ha";
        private const string _ytUrl = "https://youtrack.example.ru";

        public async Task<IEnumerable<YTIssue>> GetIssuesAsync(string query)
        {
            const string fields =
                "id,created,resolved,parent(issues(id)),project(id,shortName),numberInProject,summary,fields(projectCustomField(field(name)),value(name,minutes,presentation)),tags(name)";

            var resultJsonString = await GetAsync("/api/issues", fields, query);
            var parsedJson = JArray.Parse(resultJsonString);
            return parsedJson.ToObject<IEnumerable<YTIssue>>();
        }

        public async Task<IEnumerable<YTChange>> GetIssueHistoryAsync(string issueNumber)
        {
            var result = new List<YTChange>();
            var url = new Uri(
                $"{_ytUrl}/rest/issue/{issueNumber}/changes");
            string resultXml;
            using (var c = new HttpClient())
            {
                c.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                using (var r = await c.GetAsync(url))
                {
                    r.EnsureSuccessStatusCode();
                    resultXml = await r.Content.ReadAsStringAsync();
                }
            }

            var parsedXml = XDocument.Parse(resultXml);
            YTChange issue = null;
            foreach (var node in parsedXml.Root.Elements())
            {
                if (node.Name != "change" && node.Name != "issue")
                {
                    continue;
                }

                var change = new YTChange();
                switch (node.Name.ToString())
                {
                    case "change":
                        result.Add(change);
                        break;
                    case "issue":
                        issue = change;
                        break;
                }

                foreach (var fieldElement in node.Elements())
                {
                    if (fieldElement.Name != "field")
                    {
                        continue;
                    }

                    var field = new YTChangeField();
                    change.Fileds.Add(field);
                    field.Name = fieldElement.Attribute("name").Value;

                    foreach (var valueElement in fieldElement.Elements())
                    {
                        switch (valueElement.Name.ToString())
                        {
                            case "value":
                                field.Value.Add(valueElement.Value);
                                break;
                            case "valueId":
                                field.ValueId = valueElement.Value;
                                break;
                            case "oldValue":
                                field.OldValue = valueElement.Value;
                                break;
                            case "newValue":
                                field.NewValue = valueElement.Value;
                                break;
                        }
                    }
                }
            }

            return result;
        }

        public async Task<IEnumerable<YTBacklogElement>> GetBacklogAsync(string savedSearchId)
        {
            const string fields = "numberInProject,project(id,shortName),summary";

            var resultJsonString = await GetAsync($"/api/savedQueries/{savedSearchId}/issues", fields);
            var parsedJson = JArray.Parse(resultJsonString);
            return parsedJson.ToObject<IEnumerable<YTBacklogElement>>();
        }

        private static async Task<string> GetAsync(string path, string fields, string query = "")
        {
            var url = new Uri(
                $"{_ytUrl}{path}?fields={fields}&query={Uri.EscapeDataString(query)}");

            using (var c = new HttpClient())
            {
                c.DefaultRequestHeaders.Add("Authorization", "Bearer " + _token);
                using (var r = await c.GetAsync(url))
                {
                    r.EnsureSuccessStatusCode();
                    return await r.Content.ReadAsStringAsync();
                }
            }
        }
    }
}