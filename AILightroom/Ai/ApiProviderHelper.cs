using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Web.Script.Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.XPath;
using AILightroom.Ui;

namespace AILightroom.Ai
{
  public static class ApiProviderHelper
  {
    public static List<ApiProvider> ListProviders(string folderPath)
    {
      var ret = new List<ApiProvider>();

      if (Directory.Exists(folderPath))
      {
        var files = Directory.GetFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly);

        ret.AddRange(files.Select(file => LoadFromFile(Path.GetFileName(file))));
      }
      return ret;
    }

    public static string[] ListProviderFiles(string folderPath)
    {
      var ret = new List<string>();
      if (Directory.Exists(folderPath))
      {
        var files = Directory.GetFiles(folderPath, "*.xml", SearchOption.TopDirectoryOnly);
        ret.AddRange(files.Select(Path.GetFileName));
      }
      return ret.ToArray();
    }

    public static ApiProvider LoadFromFile(string filePath)
    {
      ApiProvider provider;
      var serializer = new XmlSerializer(typeof (ApiProvider));

      using (var fs = new FileStream(filePath, FileMode.Open))
      {
        using (var xr = new XmlTextReader(fs))
        {
          provider = (ApiProvider) serializer.Deserialize(xr);
        }
      }
      return provider;
    }

    public static void SaveToFile(ApiProvider provider, string filePath)
    {
      var serializer = new XmlSerializer(typeof (ApiProvider));

      using (var fs = new FileStream(filePath, FileMode.Create))
      {
        serializer.Serialize(fs, provider);
      }
    }

    public static BinaryFilesArray CallImageApi(ApiProvider provider, Dictionary<string, object> parameters)
    {
      var request = (HttpWebRequest) WebRequest.Create(provider.Endpoint);
      request.Method = "POST";
      request.ContentType = "application/json";
      request.Headers["Authorization"] = "Bearer " + provider.ApiKey;

      request.Timeout = 300000;
      request.ReadWriteTimeout = 300000;

      var json = new JavaScriptSerializer().Serialize(parameters);
      var data = Encoding.UTF8.GetBytes(json);
      request.ContentLength = data.Length;

      using (var stream = request.GetRequestStream())
      {
        stream.Write(data, 0, data.Length);
      }

      var ret = new BinaryFilesArray();

      try
      {
        using (var response = (HttpWebResponse) request.GetResponse())
        {
          // Get the response stream to pass to our parser
          using (var responseStream = response.GetResponseStream())
          {
            if (provider.OutputType == FileEncodingMethod.Binary)
            {
              using (var reader = new MemoryStream())
              {
                responseStream.CopyTo(reader);
                var binary = reader.ToArray();
                ret.Add(binary);
              }
            }
            else if (provider.OutputType == FileEncodingMethod.JsonUrls)
            {
              if (provider.OutputSchema == null || string.IsNullOrWhiteSpace(provider.OutputSchema.UrlExtractionPath))
              {
                throw new InvalidOperationException(
                  "Provider is configured for JsonUrlToImage but is missing the UrlExtractionPath in its configuration.");
              }

              var urls = GetUrlFromJsonResponse(responseStream, provider.OutputSchema.UrlExtractionPath);
              foreach (var url in urls)
              {
                ret.Add(GetBytesFromUrl(url));
              }
            }
          }
        }
      }
      catch (WebException ex)
      {
        if (ex.Response != null)
        {
          using (var errorResponse = (HttpWebResponse) ex.Response)
          {
            using (var responseStream = errorResponse.GetResponseStream())
            {
              using (var reader = new StreamReader(responseStream))
              {
                var errorText = reader.ReadToEnd();
                throw new Exception(
                  string.Format("Server returned error: {0}. Details: {1}",
                    errorResponse.StatusCode, errorText), ex);
              }
            }
          }
        }
      }
      catch (Exception ex)
      {
        throw ex;
      }
      return ret;
    }

    private static string[] GetUrlFromJsonResponse(Stream responseStream, string urlExtractionPath)
    {
      XDocument xdoc;
      using (var jsonReader = JsonReaderWriterFactory.CreateJsonReader(responseStream, new XmlDictionaryReaderQuotas()))
      {
        xdoc = XDocument.Load(jsonReader);
      }

      var urlElements = xdoc.XPathSelectElements(urlExtractionPath);

      var ret = new List<string>();
      foreach (var urlElement in urlElements)
      {
        var imageUrl = urlElement.Value;
        if (!string.IsNullOrEmpty(imageUrl))
        {
          ret.Add(imageUrl);
        }
      }

      if (ret.Count == 0)
      {
        throw new Exception("The configured XPath query did not find any image URLs in the response.");
      }
      return ret.ToArray();
    }

    private static byte[] GetBytesFromUrl(string imageUrl)
    {
      using (var client = new WebClient())
      {
        try
        {
          var imageData = client.DownloadData(imageUrl);
          //File.WriteAllBytes(Path.GetFileName(new Uri(imageUrl).AbsolutePath), imageData);
          return imageData;
        }
        catch (Exception ex)
        {
          throw ex;
        }
      }
    }
  }
}