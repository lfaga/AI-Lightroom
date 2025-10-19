using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AILightroom.Ai;
using AILightroom.Properties;
using AILightroom.Ui;
using AILightroom.Ui.Controls;
using Microsoft.Win32;

namespace AILightroom
{
  /// <summary>
  ///   Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    private BinaryFilesArray _lastImages;

    private ApiProvider _provider;

    public MainWindow()
    {
      InitializeComponent();
    }

    private void BtnExecute_Click(object sender, RoutedEventArgs e)
    {
      var vplist = new Dictionary<string, object>();

      foreach (FrameworkElement child in PanelForm.Children)
      {
        var t = child.GetType();

        if (t.Name == "TextBox")
        {
          var o = (TextBox) child;
          vplist.Add(o.Tag.ToString(), o.Text);
        }
        else if (t.Name == "IntTextBox")
        {
          var o = (IntTextBox) child;
          vplist.Add(o.Tag.ToString(), int.Parse(o.Text));
        }
        else if (t.Name == "NumberTextBox")
        {
          var o = (NumberTextBox) child;
          vplist.Add(o.Tag.ToString(), double.Parse(o.Text));
        }
        else if (t.Name == "CustomSlider")
        {
          var o = (CustomSlider) child;
          vplist.Add(o.Tag.ToString(), o.Value);
        }
        else if (t.Name == "ComboBox")
        {
          var o = (ComboBox) child;
          vplist.Add(o.Tag.ToString(), o.SelectedValue.ToString());
        }
        else
        {
          if (t.Name != "Label")
            MessageBox.Show(string.Format("No rule for {0}", t.Name));
        }
      }

      BtnExecute.IsEnabled = false;

      Task.Factory.StartNew(() => { return ApiProviderHelper.CallImageApi(_provider, vplist); })
        .ContinueWith(task =>
        {
          try
          {
            if (task.IsFaulted)
            {
              // Extract and show the real exception message
              if (task.Exception != null)
              {
                var ex = task.Exception.GetBaseException();
                MessageBox.Show("An error occurred: " + ex.Message, "Error");
              }
              ImgResult.Source = (ImageSource) FindResource("BmpPlaceholder");
              BtnSave.Visibility = Visibility.Hidden;
              return; // Stop processing
            }

            _lastImages = task.Result;

            if (_lastImages != null && _lastImages.Count > 0)
            {
              ImgResult.Source = _lastImages.GetBitmap(0);
              BtnSave.Visibility = Visibility.Visible;
              Console.Beep();
            }
            else
            {
              ImgResult.Source = (ImageSource) FindResource("BmpPlaceholder");
              BtnSave.Visibility = Visibility.Hidden;
              MessageBox.Show("The operation completed, but no images were returned.", "Info");
            }
          }
          finally
          {
            BtnExecute.IsEnabled = true;
          }
        }, TaskScheduler.FromCurrentSynchronizationContext());
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
      if (Directory.Exists(App.GetConfigPath()))
      {
        var files = Directory.GetFiles(App.GetConfigPath(), "*.xml", SearchOption.TopDirectoryOnly);

        foreach (var file in files)
        {
          ComboProviders.Items.Add(Path.GetFileName(file));
        }
      }

      ComboProviders.SelectionChanged +=
        (o, args) =>
        {
          _provider =
            ApiProviderHelper.LoadFromFile(Path.Combine(App.GetConfigPath(), ComboProviders.SelectedItem.ToString()));
          PanelForm = UiHelper.CreateForm(this, _provider.InputSchema, PanelForm);
        };

      if (ComboProviders.Items.Count > 0)
      {
        ComboProviders.SelectedIndex = 0;
      }
    }

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);

      if (Settings.Default.MainWindowHeight >= 100.0)
      {
        Height = Settings.Default.MainWindowHeight;
      }

      if (Settings.Default.MainWindowWidth >= 100.0)
      {
        Width = Settings.Default.MainWindowWidth;
      }

      if (Settings.Default.MainWindowTop >= 0.0)
      {
        Top = Settings.Default.MainWindowTop;
      }

      if (Settings.Default.MainWindowLeft >= 0.0)
      {
        Left = Settings.Default.MainWindowLeft;
      }

      if (Settings.Default.MainWindowSplit >= 100.0)
      {
        ParametersColumn.Width = new GridLength(Settings.Default.MainWindowSplit);
      }
      else
      {
        ParametersColumn.Width = new GridLength(100);
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (WindowState == WindowState.Normal)
      {
        Settings.Default.MainWindowHeight = Height;
        Settings.Default.MainWindowWidth = Width;
        Settings.Default.MainWindowTop = Top;
        Settings.Default.MainWindowLeft = Left;
        Settings.Default.MainWindowSplit = ParametersColumn.ActualWidth;
      }
      Settings.Default.Save();

      base.OnClosing(e);
    }

    private void BtnSave_Click(object sender, RoutedEventArgs e)
    {
      var save = new SaveFileDialog
      {
        Title = "Save picture as ",
        Filter = "PNG Files|*.png",
        FileName = string.Format("{0}-{1:yyyyMMdd-hhmmss}.png", _provider.Name.Replace(" ", ""), DateTime.Now)
      };
      if (_lastImages[0] != null)
      {
        if (save.ShowDialog() == true)
        {
          _lastImages.SaveImage(save.FileName, 0);
        }
      }
    }

    private void BtnConfigProviders_Click(object sender, RoutedEventArgs e)
    {
      var w = new ProvidersWindow();
      if (w.ShowDialog().GetValueOrDefault(false))
      {
        MessageBox.Show("Clicked Ok, update providers combo");
      }
    }
  }
}