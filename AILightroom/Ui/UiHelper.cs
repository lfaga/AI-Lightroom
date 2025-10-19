using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using AILightroom.Ai;
using AILightroom.Ui.Controls;

namespace AILightroom.Ui
{
  public static class UiHelper
  {
    public static StackPanel CreateForm(FrameworkElement window, List<SchemaElement> elements, StackPanel panel = null)
    {
      if (panel == null)
        panel = new StackPanel
        {
          Orientation = Orientation.Vertical,
          CanVerticallyScroll = true
        };

      panel.Children.Clear();

      foreach (var element in elements)
      {
        if (element.Type != SchemaElementType.Constant)
        {
          var lbl = new Label {Content = element.Name};
          panel.Children.Add(lbl);
        }

        FrameworkElement ctl = null;

        switch (element.Type)
        {
          case SchemaElementType.String:
            ctl = new TextBox
            {
              Text = element.Default,
              MinLines = 3,
              MaxLines = 6,
              TextWrapping = TextWrapping.Wrap,
              VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
              Tag = element.Name
            };
            break;
          case SchemaElementType.Integer:
            ctl = GetIntControl(window, element);
            break;
          case SchemaElementType.Number:
            ctl = GetNumberControl(window, element);
            break;
          case SchemaElementType.Choice:
            var cmb = new ComboBox
            {
              Tag = element.Name
            };
            foreach (var choice in element.Choices)
            {
              cmb.Items.Add(choice);
            }
            cmb.SelectedItem = element.Default;
            ctl = cmb;
            break;
          case SchemaElementType.ImageBinary:
          case SchemaElementType.ImageB64:
          case SchemaElementType.ImageUrl:
            ctl = new FileTextBox
            {
              FullFilePath = element.Default,
              Filter = FileTextBox.AllImageFilterPreset,
              FileEncodingMethod = element.Type,
              Tag = element.Name
            };
            break;
          case SchemaElementType.ChildElements:
            ctl = CreateForm(window, element.Children);
            ctl.Tag = element.Name;
            break;
          case SchemaElementType.Constant:
            ctl = new TextBox
            {
              Text = element.Default, 
              Tag = element.Name,
              Visibility = Visibility.Collapsed
            };
            break;
        }
        if (ctl != null)
        {
          panel.Children.Add(ctl);
        }
      }

      return panel;
    }

    public static Control GetIntControl(FrameworkElement window, SchemaElement element)
    {
      if (element.Step.HasValue)
      {
        return new CustomSlider
        {
          Orientation = Orientation.Horizontal,
          Minimum = Math.Floor(element.Minimum.GetValueOrDefault(0.0)),
          Maximum = Math.Floor(element.Maximum.GetValueOrDefault(int.MaxValue)),
          Step = Math.Floor(element.Step.GetValueOrDefault(1.0)),
          Value = int.Parse(element.Default),
          Tag = element.Name
        };
      }
      return new IntTextBox
      {
        Text = element.Default,
//        Style = (Style) window.FindResource("IntTextBoxStyle"),
        ErrorStyle = (Style) window.FindResource("ErrorUserControlStyle"),
        Required = element.Required,
        Min = (int?) element.Minimum,
        Max = (int?) element.Maximum,
        Tag = element.Name
      };
    }

    public static Control GetNumberControl(FrameworkElement window, SchemaElement element)
    {
      if (element.Step.HasValue)
      {
        return new CustomSlider
        {
          Orientation = Orientation.Horizontal,
          Minimum = element.Minimum.GetValueOrDefault(0.0),
          Maximum = element.Maximum.GetValueOrDefault(double.MaxValue),
          Step = element.Step.GetValueOrDefault(1.0),
          Value = double.Parse(element.Default),
          Tag = element.Name
        };
      }
      return new NumberTextBox
      {
        Text = element.Default,
//        Style = (Style) window.FindResource("NumberTextBoxStyle"),
        ErrorStyle = (Style) window.FindResource("ErrorUserControlStyle"),
        Required = element.Required,
        Min = element.Minimum,
        Max = element.Maximum,
        Tag = element.Name
      };
    }
  }
}