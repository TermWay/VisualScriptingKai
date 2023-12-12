using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UIElements;

namespace CHM.VisualScriptingKai.Editor
{
    public class QueryResultsListView : ListView
    {
        public new class UxmlFactory : UxmlFactory<QueryResultsListView, UxmlTraits>
        {
        }
        private class QueryResultsEntry
        {
            private Label infoLabel;
            private VisualElement icon;
            private VisualElement container;

            public string FilterPattern { get; set; }
            public QueryResultsEntry(VisualElement visualElement, VisualElement listContainer)
            {
                infoLabel = visualElement.Q<Label>("info");
                icon = visualElement.Q("icon");
                container = visualElement.Q("container");
                // It's sad that I have to use this hack to replace CSS's nth-child :/
                // TODO: Make the alpha here configurable?
                if(listContainer.childCount % 2 == 1)
                {
                    container.style.backgroundColor = new Color(0, 0, 0, 0.1f);
                }
                else
                {
                    container.style.backgroundColor = new Color(0, 0, 0, 0f);
                }
            }
            public void SetData(IGraphElementTrace nodeTrace)
            {
                infoLabel.text = nodeTrace.GetInfo();
                if (!string.IsNullOrWhiteSpace(FilterPattern))
                {
                    string coloredReplace = $"<color=#{ColorUtility.ToHtmlStringRGBA(Color.cyan)}><b>{FilterPattern}</b></color>";
                    infoLabel.text = Regex.Replace(input: infoLabel.text, pattern: FilterPattern, replacement: coloredReplace, RegexOptions.IgnoreCase);
                }
                icon.style.backgroundImage = nodeTrace.GetIcon(60);
            }
        }
        private readonly List<IGraphElementTrace> queryResults = new();
        public QueryResultsListView() : base()
        {
            var listContainer = this.Q("unity-content-container");
            Debug.Assert(listContainer != null);
            var queryResultEntryAsset = PackageUtility.LoadPackageAsset<VisualTreeAsset>("Editor/Resources/QueryResultEntry.uxml");
            makeItem = () => {
                var queryResultEntry = queryResultEntryAsset.CloneTree();
                queryResultEntry.userData = new QueryResultsEntry(queryResultEntry, listContainer);
                return queryResultEntry;
            };
            bindItem += (item, index) =>
            {
                QueryResultsEntry entry = item.userData as QueryResultsEntry;
                entry.FilterPattern = FilterPattern;
                entry.SetData(queryResults[index]);
            };
            onSelectionChange += items => {
                int index = selectedIndex;
                if(queryResults.Count == 0 || index >= queryResults.Count)
                    return;
                queryResults[index].GraphWindowJumpToLocation();
            };
            itemsSource = queryResults;
            visible = false;
            // TODO: Add a different class name to the uss class list if needed?
        }
        public int Count => queryResults.Count;
        public void FlushQueryResults()
        {
            queryResults.Clear();
            visible = false;
            selectedIndex = -1;
            Rebuild();
        }
        public void LoadQueryResults(IEnumerable<IGraphElementTrace> nodeTraces)
        {
            queryResults.Clear();
            queryResults.AddRange(nodeTraces);
            queryResults.Sort();
            visible = queryResults.Count > 0;
            Rebuild();
        }
        public string FilterPattern { get; set; }
        public void UpdateQueryResults(GraphSource source, IEnumerable<IGraphElementTrace> traces)
        {
            int removeIndexStart = queryResults.FindIndex(x => (Object)x.Source == (Object)source);
            int removeIndexEnd = queryResults.FindLastIndex(x => (Object)x.Source == (Object)source) + 1;
            // TODO: Optimize this (merge two sorted lists? but nodeTraces isn't necessarily sorted)
            if(removeIndexStart >= 0)
            {
                queryResults.RemoveRange(removeIndexStart, removeIndexEnd - removeIndexStart);
                // queryResults.InsertRange(removeIndexStart, nodeTraces);
            }
            queryResults.AddRange(traces);
            queryResults.Sort();
            visible = queryResults.Count > 0;
            Rebuild();
        }
    }
}
