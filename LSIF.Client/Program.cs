using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Buildalyzer;
using Buildalyzer.Workspaces;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LSIF.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var solutionFilePath = args[0];

            var manager = new AnalyzerManager(solutionFilePath);
            var workspace = manager.GetWorkspace();

            var builder = new Graph.Builder();

            foreach (var project in workspace.CurrentSolution.Projects)
            {
                Console.WriteLine(project.FilePath);

                foreach (var file in project.Documents)
                {
                    builder.AddDocument(new Uri(file.FilePath));

                    // var symbols = await (new DocumentSymbolProvider()).GetCodeElementsAsync(file);

                    // foreach (var symbol in symbols)
                    // {
                    //     Console.WriteLine(symbol);
                    // }
                }
            }

            Console.WriteLine(builder.Build().ToString());
        }
    }

    public abstract class GraphElement
    {
        public GraphElement(int id, string type)
        {
            Id = id;
            Type = type;
        }
        public int Id { get; }
        public string Type { get; }
    }

    public abstract class Vertex : GraphElement
    {
        protected Vertex(int id) : base(id, "vertex")
        {
        }
        public abstract string Label { get; }
    }

    public sealed class DocumentVertex : Vertex
    {
        internal DocumentVertex(int id, Uri uri) : base(id)
        {
            Uri = uri;
        }

        public string LanguageId => "csharp";

        public override string Label => "project";

        public Uri Uri { get; }
    }

    public class Graph
    {
        private List<GraphElement> _graphElements;

        public Graph(List<GraphElement> graphElements)
        {
            _graphElements = graphElements;
        }

        public override string ToString()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver()
            };
            return $"{{{string.Join(",", _graphElements.Select(x => JsonConvert.SerializeObject(x, settings)))}}}";
        }

        public class Builder
        {
            private List<GraphElement> _graphElements = new List<GraphElement>();
            public DocumentVertex AddDocument(Uri uri)
            {
                var document = new DocumentVertex(GetNextId(), uri);
                _graphElements.Add(document);
                return document;
            }

            public Graph Build() => new Graph(_graphElements);

            private int GetNextId() => _graphElements.Count + 1;
        }
    }
}
