using System.Windows;
using System.Windows.Controls;

namespace AILightroom.Ui.Controls
{
  /// <summary>
  ///   Interaction logic for CustomSlider.xaml
  /// </summary>
  public partial class CustomSlider : UserControl
  {
    public CustomSlider()
    {
      InitializeComponent();

      Sld.ValueChanged += SldOnValueChanged;
    }


    public Orientation Orientation
    {
      get { return Sld.Orientation; }
      set { Sld.Orientation = value; }
    }

    public double Minimum
    {
      get { return Sld.Minimum; }
      set { Sld.Minimum = value; }
    }

    public double Maximum
    {
      get { return Sld.Maximum; }
      set { Sld.Maximum = value; }
    }

    public double Step
    {
      get { return Sld.LargeChange; }
      set
      {
        Sld.LargeChange = value;
        Sld.SmallChange = value;
        Sld.TickFrequency = value;
        Sld.IsMoveToPointEnabled = true;
        Sld.IsSnapToTickEnabled = true;
      }
    }

    public double Value
    {
      get { return Sld.Value; }
      set { Sld.Value = value; }
    }

    private void SldOnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> routedPropertyChangedEventArgs)
    {
      Lbl.Content = string.Format("{0:F1}", Sld.Value);
    }
  }
}