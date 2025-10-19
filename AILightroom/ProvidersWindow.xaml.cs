using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using AILightroom.Ai;
using AILightroom.Properties;

namespace AILightroom
{
  /// <summary>
  ///   Interaction logic for ProvidersWindow.xaml
  /// </summary>
  public partial class ProvidersWindow : Window
  {
    public ProvidersWindow()
    {
      InitializeComponent();
    }

    private void BtnAccpet_OnClick(object sender, RoutedEventArgs e)
    {
      DialogResult = true;
      Close();
    }

    private void BtnCancel_OnClick(object sender, RoutedEventArgs e)
    {
      Close();
    }


    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);

      if (Settings.Default.ProvidersWindowHeight >= 100.0)
      {
        Height = Settings.Default.ProvidersWindowHeight;
      }

      if (Settings.Default.ProvidersWindowWidth >= 100.0)
      {
        Width = Settings.Default.ProvidersWindowWidth;
      }

      if (Settings.Default.ProvidersWindowTop >= 0.0)
      {
        Top = Settings.Default.ProvidersWindowTop;
      }

      if (Settings.Default.ProvidersWindowLeft >= 0.0)
      {
        Left = Settings.Default.ProvidersWindowLeft;
      }

      if (Settings.Default.ProvidersWindowSplit >= 100.0)
      {
        ProvidersListColumn.Width = new GridLength(Settings.Default.ProvidersWindowSplit);
      }
      else
      {
        ProvidersListColumn.Width = new GridLength(100);
      }
    }

    protected override void OnClosing(CancelEventArgs e)
    {
      if (WindowState == WindowState.Normal)
      {
        Settings.Default.ProvidersWindowHeight = Height;
        Settings.Default.ProvidersWindowWidth = Width;
        Settings.Default.ProvidersWindowTop = Top;
        Settings.Default.ProvidersWindowLeft = Left;
        Settings.Default.ProvidersWindowSplit = ProvidersListColumn.ActualWidth;
      }
      Settings.Default.Save();

      base.OnClosing(e);
    }

    private void ProvidersWindow_OnLoaded(object sender, RoutedEventArgs e)
    {
      ListFiles.Items.Clear();

      foreach (var s in Enum.GetNames(typeof (FileEncodingMethod)))
      {
        ComboOutputType.Items.Add(s);
      }
      foreach (var s in Enum.GetNames(typeof (SchemaElementType)))
      {
        ComboElementType.Items.Add(s);
      }

      RefreshList();
    }

    private void RefreshList()
    {
      var list = ApiProviderHelper.ListProviderFiles(App.GetConfigPath());
      foreach (var fileName in list)
      {
        var i = new ListBoxItem
        {
          Content = fileName,
          Tag = Path.Combine(App.GetConfigPath(), fileName)
        };
        ListFiles.Items.Add(i);
      }
    }

    private void ListFiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
      var item = (ListBoxItem) ListFiles.SelectedItem;
      var provider = ApiProviderHelper.LoadFromFile(item.Tag.ToString());

      TxtName.Text = provider.Name;
      TxtEndpoint.Text = provider.Endpoint;
      TxtApiKey.Text = provider.ApiKey;
      ComboOutputType.SelectedItem = provider.OutputType.ToString();

      TreeInputSchema.Items.Clear();

      foreach (var element in provider.InputSchema)
      {
        var tvi = new TreeViewItem
        {
          Header = element.Name
        };

        TreeInputSchema.Items.Add(tvi);

        if (element.Type == SchemaElementType.ChildElements)
        {
          AddTreeViewChildren(element, tvi);
        }
      }
    }

    private void AddTreeViewChildren(SchemaElement parentSchemaElement, TreeViewItem parentItem)
    {
      foreach (var child in parentSchemaElement.Children)
      {
        var tvi = new TreeViewItem
        {
          Header = child.Name
        };
        parentItem.Items.Add(tvi);
      }
    }
  }
}