using System.Collections.Generic;

namespace AILightroom.Ai
{
  public class SchemaElement
  {
    public string Name { get; set; }
    public string Description { get; set; }
    public SchemaElementType Type { get; set; }
    public double? Minimum { get; set; }
    public double? Maximum { get; set; }
    public double? Step { get; set; }
    public List<string> Choices { get; set; }
    public string Default { get; set; }
    public bool? Required { get; set; }
    public List<SchemaElement> Children { get; set; }
  }
}