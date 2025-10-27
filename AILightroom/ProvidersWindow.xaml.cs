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
    private ApiProvider _selectedProvider;

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
      foreach (var s in Enum.GetNames(typeof (RequestType)))
      {
        ComboRequestType.Items.Add(s);
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
      _selectedProvider = ApiProviderHelper.LoadFromFile(item.Tag.ToString());

      TxtName.Text = _selectedProvider.Name;
      TxtEndpoint.Text = _selectedProvider.Endpoint;
      TxtApiKey.Text = _selectedProvider.ApiKey;
      ComboRequestType.SelectedItem = _selectedProvider.RequestType.ToString();
      ComboOutputType.SelectedItem = _selectedProvider.OutputType.ToString();

      TreeInputSchema.Items.Clear();

      foreach (var element in _selectedProvider.InputSchema)
      {
        var tvi = new TreeViewItem
        {
          Header = element.Name,
          Tag = element
        };

        TreeInputSchema.Items.Add(tvi);

        if (element.Type == SchemaElementType.ChildElements)
        {
          AddTreeViewChildren(element, tvi);
        }
      }
    }

    private static void AddTreeViewChildren(SchemaElement parentSchemaElement, TreeViewItem parentItem)
    {
      foreach (var child in parentSchemaElement.Children)
      {
        var tvi = new TreeViewItem
        {
          Header = child.Name,
          Tag = child
        };
        parentItem.Items.Add(tvi);
      }
    }

    private void TreeInputSchema_SelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
    {
      var tvi = TreeInputSchema.SelectedItem as TreeViewItem;
      if (tvi != null)
      {
        var se = tvi.Tag as SchemaElement;

        if (se != null)
        {
          TxtElementName.Text = se.Name;
          TxtElementDescription.Text = se.Description;
          ComboElementType.SelectedValue = se.Type.ToString();
          NumberElementMininmum.Text = se.Minimum.ToString();
          NumberElementMaximum.Text = se.Maximum.ToString();
          NumberElementStep.Text = se.Step.ToString();

          ListElementChoices.Items.Clear();
          if (se.Choices != null)
          {
            foreach (var choice in se.Choices)
            {
              ListElementChoices.Items.Add(choice);
            }
          }

          TxtElementDefault.Text = se.Default;
          ChkElementRequired.IsChecked = se.Required;
        }
      }
    }

    private void BtnInpSchAdd_OnClick(object sender, RoutedEventArgs e)
    {
      var nse = new SchemaElement
      {
        Name = "Unnamed",
        Type = SchemaElementType.String
      };

      SchemaElement selElement = null;

      var tvParent = TreeInputSchema.SelectedItem as TreeViewItem;

      if (tvParent != null)
      {
        selElement = tvParent.Tag as SchemaElement;
      }

      var selCanHoldChildren = selElement != null && selElement.Type == SchemaElementType.ChildElements;

      if (selCanHoldChildren)
      {
        //add element as a child of the selected node if the node is of ChildElements type
        selElement.Children.Add(nse);
      }
      else
      {
        //add element to the root of the InputSchema
        _selectedProvider.InputSchema.Add(nse);
      }

      var tvi = new TreeViewItem
      {
        Header = nse.Name,
        Tag = nse
      };

      if (selCanHoldChildren)
      {
        tvParent.Items.Add(tvi);
      }
      else
      {
        TreeInputSchema.Items.Add(tvi);
      }

      //select the new node
      tvi.IsSelected = true;
    }

    private void BtnInpSchRemove_OnClick(object sender, RoutedEventArgs e)
    {
      var tvselected = TreeInputSchema.SelectedItem as TreeViewItem;

      if (tvselected != null)
      {
        var selElement = tvselected.Tag as SchemaElement;

        if (selElement != null)
        {
          if (selElement.Type == SchemaElementType.ChildElements
              && selElement.Children.Count > 0)
          {
            MessageBox.Show("Remove all children before deleting.", "Parent contains children", MessageBoxButton.OK);
            return;
          }

          if (MessageBox.Show(string.Format("Confirm removing {0}", selElement.Name),
            "Element Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            return;

          if (selElement.Parent != null)
          {
            selElement.Parent.Children.Remove(selElement);
          }
          else
          {
            _selectedProvider.InputSchema.Remove(selElement);
          }
        }

        TreeInputSchema.SelectedValuePath = string.Empty;

        var tvp = tvselected.Parent as TreeViewItem;

        if (tvp != null)
        {
          tvp.Items.Remove(tvselected);
        }
        else
        {
          TreeInputSchema.Items.Remove(tvselected);
        }
      }
    }
  }
}