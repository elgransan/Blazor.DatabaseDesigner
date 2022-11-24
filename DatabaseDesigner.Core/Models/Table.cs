using Blazor.Diagrams.Core.Geometry;
using Blazor.Diagrams.Core.Models;
using DatabaseDesigner.Core.Models;
using System.Collections.Generic;
using System.Linq;
public class Table : NodeModel
{
    public Table(string name, string label, string icon, List<Column> columns, Point? position = null) : base(position, RenderLayer.HTML)
    {
        Name = name;
        Label = label;
        Icon = icon;
        Columns = columns;

        AddPort(Columns[0], PortAlignment.Right);
        foreach (var column in columns)
            AddPort(column, PortAlignment.Left);
    }

    public string Name { get; set; } = "Table";
    public string Label { get; set; } = "Table";
    public string Icon { get; set; }
    public List<Column> Columns { get; set; }
    public bool HasPrimaryColumn => Columns.Any(c => c.Primary);

    public ColumnPort? GetPort(Column column) => Ports.Cast<ColumnPort>().FirstOrDefault(p => p.Column == column);

    public void AddPort(Column column, PortAlignment alignment) => AddPort(new ColumnPort(this, column, alignment));
}
