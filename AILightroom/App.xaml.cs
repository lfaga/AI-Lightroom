using System.Net;
using System.Windows;

namespace AILightroom
{
  /// <summary>
  ///   Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    public App()
    {
      // This enables TLS 1.2 and TLS 1.1 for all web requests in your application.
      // The enum for Tls12 doesn't exist in .NET 4.0, so we use its integer value.
      // Tls12 = 3072, Tls11 = 768
      // The |= operator adds these protocols without disabling others.
      ServicePointManager.SecurityProtocol |= (SecurityProtocolType) 3072 | (SecurityProtocolType) 768;
    }

    public static string GetConfigPath()
    {
#if DEBUG
      const string path = @"D:\Projects\AILightroom\Work";
#else
      var path = Settings.Default.ConfigPath;

      if (string.IsNullOrWhiteSpace(path))
      {
        path = AppDomain.CurrentDomain.BaseDirectory;
      }

      Settings.Default.ConfigPath = path;
      Settings.Default.Save();
#endif
      return path;
    }
  }
}