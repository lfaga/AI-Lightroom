using System.Collections.Generic;

namespace AILightroom.Ai
{
  public class ApiProvider
  {
    public string Name { get; set; }

    public string Endpoint { get; set; }

    public string ApiKey { get; set; }

    public RequestType RequestType { get; set; }

    public List<SchemaElement> InputSchema { get; set; }

    public FileEncodingMethod OutputType { get; set; }

    public OutputSchema OutputSchema { get; set; }
  }
}