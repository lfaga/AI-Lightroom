using System.Collections.Generic;
using System.IO;
using System.Windows.Media.Imaging;

namespace AILightroom.Ui
{
  public class BinaryFile
  {
    public string Key { get; set; }
    public byte[] Bytes { get; set; }
    public string Filename { get; set; }
    public string Description { get; set; }
  }

  public class BinaryFilesArray
  {
    public BinaryFilesArray()
    {
      Files = new List<BinaryFile>();
    }

    public List<BinaryFile> Files { get; private set; }

    public BinaryFile this[int index]
    {
      get { return Files[index]; }
      set { Files[index] = value; }
    }

    public int Count
    {
      get { return Files.Count; }
    }

    public void Add(byte[] bytes)
    {
      Add(string.Format("{0}", Files.Count + 1), bytes);
    }

    public void Add(string key, byte[] bytes, string fileName = null, string description = null)
    {
      Files.Add(new BinaryFile {Key = key, Bytes = bytes, Filename = fileName, Description = description});
    }

    public int GetIndexFromKey(string key)
    {
      return Files.FindIndex(row => row.Key == key);
    }

    public bool SaveImage(string fullpath, int index)
    {
      File.WriteAllBytes(fullpath, Files[index].Bytes);
      return true;
    }

    public BitmapImage GetBitmap(int index)
    {
      var bmp = new BitmapImage();
      using (var ms = new MemoryStream(this[index].Bytes))
      {
        ms.Position = 0;
        bmp.BeginInit();
        bmp.StreamSource = ms;
        bmp.CacheOption = BitmapCacheOption.OnLoad;
        bmp.EndInit();
      }
      bmp.Freeze();
      return bmp;
    }
  }
}