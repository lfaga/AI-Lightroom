using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using AILightroom.Ai;
using AILightroom.Properties;

namespace AILightroom
{
  /// <summary>
  ///   Interaction logic for ProvidersWindow.xaml
  /// </summary>
  public partial class ProvidersWindow : Window
  {
    private bool _elementIsDirty;
    private ListBoxItem _previousFileListItemSelected;
    private TreeViewItem _previousSchemaTreeItemSelected;
    private string _providerFilename;
    private bool _providerIsDirty;
    private ApiProvider _selectedProvider;

    public ProvidersWindow()
    {
      InitializeComponent();
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

      TxtName.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnProviderDataChanged));
      TxtEndpoint.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnProviderDataChanged));
      TxtApiKey.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnProviderDataChanged));
      ComboRequestType.AddHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(OnProviderDataChanged));
      ComboOutputType.AddHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(OnProviderDataChanged));

      TxtElementName.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
      TxtElementDescription.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
      ComboElementType.AddHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
      NumberElementMininmum.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
      NumberElementMaximum.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
      NumberElementStep.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
      ListElementChoices.AddHandler(Selector.SelectionChangedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
      TxtElementDefault.AddHandler(TextBoxBase.TextChangedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
      ChkElementRequired.AddHandler(ToggleButton.CheckedEvent, new RoutedEventHandler(OnSchemaElementDataChanged));
    }

    private void OnProviderDataChanged(object sender, RoutedEventArgs e)
    {
      _providerIsDirty = true;
    }

    private void OnSchemaElementDataChanged(object sender, RoutedEventArgs e)
    {
      _elementIsDirty = true;
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
      ListFiles.Items.Clear();

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
      var item = ListFiles.SelectedItem as ListBoxItem;
      if (item != null && item.Equals(_previousFileListItemSelected))
        return;

      if (_providerIsDirty && MessageBox.Show("Discard provider changes",
        "Warning: Selecting a different provider will discard currently unsaved changes.\nDo you wish to continue?",
        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
      {
        if (_previousFileListItemSelected != null)
          _previousFileListItemSelected.IsSelected = true;
        return;
      }

      if (item != null)
      {
        ClearProviderControls();

        _providerFilename = item.Tag.ToString();

        _selectedProvider = ApiProviderHelper.LoadFromFile(_providerFilename);

        TxtName.Text = _selectedProvider.Name;
        TxtEndpoint.Text = _selectedProvider.Endpoint;
        TxtApiKey.Text = _selectedProvider.ApiKey;
        ComboRequestType.SelectedItem = _selectedProvider.RequestType.ToString();
        ComboOutputType.SelectedItem = _selectedProvider.OutputType.ToString();

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
            tvi.IsExpanded = true;
          }
        }
        //If a new provider is loaded, the dirty flags should be false.
        _providerIsDirty = false;
        _elementIsDirty = false;
        _previousFileListItemSelected = item;
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

      if (tvi != null && tvi.Equals(_previousSchemaTreeItemSelected))
        return;

      if (_elementIsDirty && MessageBox.Show("Discard changes",
        "Warning: Changing selected nodes will discard current changes.\nDo you wish to continue?",
        MessageBoxButton.YesNo) != MessageBoxResult.Yes)
      {
        _previousSchemaTreeItemSelected.IsSelected = true;
        return;
      }

      MapElementFromTreeNode(tvi);
      _previousSchemaTreeItemSelected = tvi;
    }

    private void MapElementFromTreeNode(TreeViewItem tvi)
    {
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

          _elementIsDirty = false;
        }
      }
    }

    private void BtnInpSchAdd_OnClick(object sender, RoutedEventArgs e)
    {
      if (_selectedProvider == null)
        return;

      ClearElementControls();

      var nse = new SchemaElement
      {
        Name = "Unnamed",
        Type = SchemaElementType.String
      };

      var tvParent = TreeInputSchema.SelectedItem as TreeViewItem;
      var seParent = tvParent == null ? null : tvParent.Tag as SchemaElement;

      var tvi = new TreeViewItem
      {
        Header = nse.Name,
        Tag = nse
      };

      if (seParent != null && seParent.Type == SchemaElementType.ChildElements)
      {
        tvParent.Items.Add(tvi);
      }
      else
      {
        TreeInputSchema.Items.Add(tvi);
      }

      _providerIsDirty = true;
      //select the new node
      _previousSchemaTreeItemSelected = tvi;
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

          tvselected.IsSelected = false;

          if (selElement.Parent != null)
          {
            selElement.Parent.Children.Remove(selElement);
          }
          else
          {
            _selectedProvider.InputSchema.Remove(selElement);
          }
        }

        var tvp = tvselected.Parent as TreeViewItem;

        if (tvp != null)
        {
          tvp.Items.Remove(tvselected);
        }
        else
        {
          TreeInputSchema.Items.Remove(tvselected);
        }

        ClearElementControls();

        _providerIsDirty = true;
      }
    }

    private void BtnNewProvider_Click(object sender, RoutedEventArgs e)
    {
      using (var ib = new InputBox())
      {
        if (ib.Show(this, "New Provider", "New provider filename (no extensions)") == MsgBoxResult.Ok)
        {
          var f = ib.Response;

          if (!string.IsNullOrWhiteSpace(f))
          {
            ClearProviderControls();

            _selectedProvider = new ApiProvider {Name = ib.Response};
            _providerFilename = ib.Response + (!f.ToLowerInvariant().EndsWith(".xml") ? ".xml" : string.Empty);

            var n = new ListViewItem
            {
              Content = _providerFilename,
              Tag = Path.Combine(App.GetConfigPath(), _providerFilename)
            };

            ListFiles.Items.Add(n);
            _previousFileListItemSelected = n;

            n.IsSelected = true;

            TxtName.Text = _selectedProvider.Name;
            _providerIsDirty = true;
          }
        }
      }
    }

    private void BtnDeleteProvider_Click(object sender, RoutedEventArgs e)
    {
      var lbi = ListFiles.SelectedItem as ListBoxItem;

      if (lbi != null)
      {
        if (MessageBox.Show(string.Format("Confirm removing {0}.\nWarning: It cannot be undone.", lbi.Content),
          "Provider Deletion", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
          return;

        var fpath = lbi.Tag.ToString();
        lbi.IsSelected = false;
        ClearProviderControls();

        ListFiles.Items.Remove(lbi);
        File.Delete(fpath);

        _providerFilename = string.Empty;
        _selectedProvider = null;
        _providerIsDirty = false;
        _elementIsDirty = false;
      }
    }

    private void BtnRefrehProviderList_Click(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show("Warning: Refreshing the providers list will rest any unsaved data.\nDo you want to continue",
        "Refresh providers", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
        return;

      var lbi = ListFiles.SelectedItem as ListBoxItem;
      if (lbi != null)
        lbi.IsSelected = false;

      ClearProviderControls();
      RefreshList();
      _providerIsDirty = false;
      _elementIsDirty = false;
    }

    private void BtnChoiceAdd_Click(object sender, RoutedEventArgs e)
    {
      var tvi = TreeInputSchema.SelectedItem as TreeViewItem;

      if (tvi != null)
      {
        var t = (SchemaElementType) Enum.Parse(typeof (SchemaElementType), ComboElementType.Text);
        if (t == SchemaElementType.Choice)
        {
          using (var ib = new InputBox())
          {
            if (ib.Show(this, "Add Choice", "Input a string value") == MsgBoxResult.Ok
                && !string.IsNullOrWhiteSpace(ib.Response))
            {
              if (!ListElementChoices.Items.Contains(ib.Response))
              {
                ListElementChoices.Items.Add(ib.Response);
                _elementIsDirty = true;
              }
            }
          }
        }
      }
    }

    private void BtnChoiceRemove_Click(object sender, RoutedEventArgs e)
    {
      var tvi = TreeInputSchema.SelectedItem as TreeViewItem;

      if (tvi != null)
      {
        var sel = tvi.Tag as SchemaElement;

        if (sel != null && sel.Type == SchemaElementType.Choice
            && ListElementChoices.SelectedIndex >= 0)
        {
          var s = ListElementChoices.Items[ListElementChoices.SelectedIndex] as string;

          sel.Choices.Remove(s);
          ListElementChoices.Items.RemoveAt(ListElementChoices.SelectedIndex);
          _elementIsDirty = true;
        }
      }
    }

    private void ClearProviderControls()
    {
      TxtName.Text = string.Empty;
      TxtEndpoint.Text = string.Empty;
      TxtApiKey.Text = string.Empty;
      ComboRequestType.SelectedIndex = -1;
      ComboOutputType.SelectedIndex = -1;

      TreeInputSchema.Items.Clear();

      ClearElementControls();
    }

    private void ClearElementControls()
    {
      ListElementChoices.Items.Clear();
      TxtElementName.Text = string.Empty;
      TxtElementDescription.Text = string.Empty;
      ComboElementType.SelectedIndex = -1;
      NumberElementMininmum.Text = string.Empty;
      NumberElementMaximum.Text = string.Empty;
      NumberElementStep.Text = string.Empty;
    }

    private void BtnClose_Click(object sender, RoutedEventArgs e)
    {
      if ((_elementIsDirty || _providerIsDirty)
          && (MessageBox.Show("Discard changes",
            "Warning: Closing this window will discard any unsaved changes.\nDo you wish to continue?",
            MessageBoxButton.YesNo) != MessageBoxResult.Yes))
      {
        return;
      }
      Close();
    }

    private void BtnProviderSave_Click(object sender, RoutedEventArgs e)
    {
      if (!string.IsNullOrWhiteSpace(_providerFilename) && _selectedProvider != null)
      {
        _selectedProvider.Name = TxtName.Text;
        _selectedProvider.Endpoint = TxtEndpoint.Text;
        _selectedProvider.ApiKey = TxtApiKey.Text;
        _selectedProvider.RequestType = (RequestType) Enum.Parse(typeof (RequestType),
          ComboRequestType.SelectedValue.ToString());
        _selectedProvider.OutputType = (FileEncodingMethod) Enum.Parse(typeof (FileEncodingMethod),
          ComboOutputType.SelectedValue.ToString());

        //_selectedProvider.InputSchema should be already updated by the "schema elements form" and it's save button.
        //since every TreeViewItem has a tag that point to a SchemaElement in the _selectedProvider, the update
        //should have been automatic and transparent. BUT CHECK IF ITS WORKING PROPERLY!

        //_selectedProvider.OutputSchema
        //oh crap, i forgot i need to create some kind of editor to define the output schema when OutputType is
        //JsonSingleB64Image, JsonMultiB64Image or JsonUrls to know where to get the image from, like:
        //<OutputSchema><UrlExtractionPath>//data/item/url</UrlExtractionPath></OutputSchema>
        //maybe for now add just a textbox to input a XPath query... too bad if you are not a programmer... ha ha
        //Also remember that only Binary and JsonUrls is implemented in ApiProviderHelper.
        //for now:
        _selectedProvider.OutputSchema = new OutputSchema {UrlExtractionPath = "//data/item/url"};

        try
        {
          ApiProviderHelper.SaveToFile(_selectedProvider, Path.Combine(App.GetConfigPath(), _providerFilename));
          _providerIsDirty = false;
          _elementIsDirty = false;
          MessageBox.Show(string.Format("Provider file saved at:\n{0}", _providerFilename),
            "Save provider", MessageBoxButton.OK);
        }
        catch (Exception ex)
        {
          MessageBox.Show("Error saving", ex.Message, MessageBoxButton.OK);
        }
      }
    }

    private void BtnElementCancel_OnClick(object sender, RoutedEventArgs e)
    {
      if (MessageBox.Show("Discard changes", "Do you want to discard current schema element changes?",
        MessageBoxButton.YesNo) == MessageBoxResult.Yes)
      {
        ClearElementControls();
        _elementIsDirty = false;
      }
    }

    private void BtnElementOk_OnClick(object sender, RoutedEventArgs e)
    {
      var tvi = TreeInputSchema.SelectedItem as TreeViewItem;
      if (tvi != null)
      {
        var sel = tvi.Tag as SchemaElement;
        if (sel != null)
        {
          tvi.Header = TxtElementName.Text;
          sel.Name = TxtElementName.Text;
          sel.Description = TxtElementDescription.Text;
          sel.Type = (SchemaElementType) Enum.Parse(typeof (SchemaElementType),
            ComboElementType.SelectedValue.ToString());

          sel.Minimum = NumberElementMininmum.GetValue();
          sel.Maximum = NumberElementMaximum.GetValue();
          sel.Step = NumberElementStep.GetValue();

          if (sel.Type == SchemaElementType.Choice)
          {
            sel.Choices = new List<string>();

            foreach (string item in ListElementChoices.Items)
            {
              sel.Choices.Add(item);
            }
          }

          sel.Default = TxtElementDefault.Text;
          sel.Required = ChkElementRequired.IsChecked;

          var tvp = tvi.Parent as TreeViewItem;
          if (tvp != null)
          {
            var sep = tvp.Tag as SchemaElement;
            if (sep != null && sep.Type == SchemaElementType.ChildElements
                && !sep.Children.Contains(sel))
            {
              sep.Children.Add(sel);
              sel.Parent = sep;
            }
          }
          else
          {
            if (_selectedProvider.InputSchema == null)
              _selectedProvider.InputSchema = new List<SchemaElement>();

            if (!_selectedProvider.InputSchema.Contains(sel))
              _selectedProvider.InputSchema.Add(sel);
          }

          _providerIsDirty = true;
          _elementIsDirty = false;
        }
      }
    }
  }
}