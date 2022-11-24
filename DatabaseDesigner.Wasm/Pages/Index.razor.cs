using Blazor.Diagrams.Core;
using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using Blazor.Diagrams.Core.Models.Base;
using DatabaseDesigner.Core.Models;
using DatabaseDesigner.Wasm.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace DatabaseDesigner.Wasm.Pages
{
    public partial class Index : IDisposable
    {
        [Inject]
        public IJSRuntime JSRuntime { get; set; }

        public Diagram Diagram { get; } = new Diagram(new DiagramOptions
        {
            GridSize = 40,
            AllowMultiSelection = false,
            Links = new DiagramLinkOptions
            {
                DefaultRouter = Routers.Orthogonal,
                DefaultPathGenerator = PathGenerators.Straight,
                Factory = (diagram, sourcePort) =>
                {
                    return new LinkModel(sourcePort, null)
                    {
                        Router = Routers.Orthogonal,
                        PathGenerator = PathGenerators.Straight
                    };
                }
            }
        });

        public void Dispose()
        {
            Diagram.Links.Added -= OnLinkAdded;
            Diagram.Links.Removed -= Diagram_LinkRemoved;
        }

        protected override void OnInitialized()
        {
            base.OnInitialized();

            Diagram.RegisterModelComponent<Table, TableNode>();
            var node1 = new Table("Table1", "Table 1", "fas fa-mobile", new List<Column>()
            {
                new Column
                {
                    Name = "Table1_id",
                    Primary = true,
                    Type = ColumnType.Integer
                },
                new Column
                {
                    Name = "Description",
                    Type = ColumnType.String,
                    ItemTemplate = true
                }
            });
            var node2 = new Table("Table2", "Table 2", "fas fa-phone", new List<Column>()
            {
                new Column
                {
                    Name = "Table2_id",
                    Primary = true,
                    Type = ColumnType.Integer
                },
                new Column
                {
                    Name = "Description",
                    Type = ColumnType.String,
                    ItemTemplate = true
                }
            });
            Diagram.Nodes.Add(node1);
            Diagram.Nodes.Add(node2);

            var childColumn = node2.Columns[1];
            var port = node2.GetPort(childColumn);
            var link = new LinkModel(port, node1.GetPort(PortAlignment.Right))
            {
                Router = Routers.Orthogonal,
                PathGenerator = PathGenerators.Straight
            };
            link.Labels.Add(new LinkLabelModel(link, childColumn.Name));
            Diagram.Links.Add(link);

            Diagram.Links.Added += OnLinkAdded;
            Diagram.Links.Removed += Diagram_LinkRemoved;
        }

        private void OnLinkAdded(BaseLinkModel link)
        {
            link.TargetPortChanged += OnLinkTargetPortChanged;
        }

        private void OnLinkTargetPortChanged(BaseLinkModel link, PortModel oldPort, PortModel newPort)
        {
            link.Labels.Add(new LinkLabelModel(link, "1..*", -40, new Point(0, -30)));
            link.Refresh();

            ((newPort ?? oldPort) as ColumnPort).Column.Refresh();
        }

        private void Diagram_LinkRemoved(BaseLinkModel link)
        {
            link.TargetPortChanged -= OnLinkTargetPortChanged;

            if (!link.IsAttached)
                return;

            var sourceCol = (link.SourcePort as ColumnPort).Column;
            var targetCol = (link.TargetPort as ColumnPort).Column;
            (sourceCol.Primary ? targetCol : sourceCol).Refresh();
        }


        private async Task ShowJson()
        {
            var json = JsonConvert.SerializeObject(new
            {
                Nodes = Diagram.Nodes.Cast<object>(),
                Links = Diagram.Links.Cast<object>()
            }, Formatting.Indented, new JsonSerializerSettings
            {
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            });

            await JSRuntime.InvokeVoidAsync("console.log", json);
        }

        private void Debug()
        {
            Console.WriteLine(Diagram.Container);
            foreach (var port in Diagram.Nodes.ToList()[0].Ports)
                Console.WriteLine(port.Position);
        }
    }
}
