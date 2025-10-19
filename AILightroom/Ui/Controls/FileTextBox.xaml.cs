using System.Windows;
using System.Windows.Controls;
using AILightroom.Ai;
using Microsoft.Win32;

namespace AILightroom.Ui.Controls
{
  /// <summary>
  ///   Interaction logic for FileTextBox.xaml
  /// </summary>
  public partial class FileTextBox : UserControl
  {
    public const string AllImageFilterPreset = "Images|*.png;*.jpg;*.jpeg";

    public static readonly DependencyProperty FilterProperty =
      DependencyProperty.Register(
        "Filter",
        typeof (string),
        typeof (FileTextBox),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty FileEncodingMethodProperty =
      DependencyProperty.Register(
        "FileEncodingMethod",
        typeof (SchemaElementType),
        typeof (FileTextBox),
        new FrameworkPropertyMetadata(null));

    public FileTextBox()
    {
      InitializeComponent();
    }

    public string FullFilePath
    {
      get { return TxtFilename.Text; }
      set { TxtFilename.Text = value; }
    }

    public string Filter
    {
      get { return (string) GetValue(FilterProperty); }
      set { SetValue(FilterProperty, value); }
    }

    public SchemaElementType FileEncodingMethod
    {
      get { return (SchemaElementType) GetValue(FileEncodingMethodProperty); }
      set { SetValue(FileEncodingMethodProperty, value); }
    }

    private void BtnBrowse_Click(object sender, RoutedEventArgs e)
    {
      var dialog = new OpenFileDialog
      {
        Filter = Filter
      };

      // Show open file dialog box
      var result = dialog.ShowDialog();

      // Process open file dialog box results
      if (result == true)
      {
        TxtFilename.Text = dialog.FileName;
      }
    }
  }
}