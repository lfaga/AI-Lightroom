using System.Windows;

namespace AILightroom.Ui.Controls
{
  /// <summary>
  ///   Follow steps 1a or 1b and then 2 to use this custom control in a XAML file.
  ///   Step 1a) Using this custom control in a XAML file that exists in the current project.
  ///   Add this XmlNamespace attribute to the root element of the markup file where it is
  ///   to be used:
  ///   xmlns:MyNamespace="clr-namespace:AILightroom.Ui"
  ///   Step 2)
  ///   Go ahead and use your control in the XAML file.
  ///   <MyNamespace:NumberTextBox />
  /// </summary>
  public class NumberTextBox : ValidatedTextBox
  {
    public static readonly DependencyProperty RequiredProperty =
      DependencyProperty.Register(
        "Required",
        typeof (bool?),
        typeof (NumberTextBox),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty MinProperty =
      DependencyProperty.Register(
        "Min",
        typeof (double?),
        typeof (NumberTextBox),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty MaxProperty =
      DependencyProperty.Register(
        "Max",
        typeof (double?),
        typeof (NumberTextBox),
        new FrameworkPropertyMetadata(null));


    public NumberTextBox() : base(new NumberValidationRule())
    {
    }

    public bool? Required
    {
      get { return (bool?) GetValue(RequiredProperty); }
      set
      {
        SetValue(RequiredProperty, value);
        ((NumberValidationRule) Rule).Required = value;
      }
    }

    public double? Min
    {
      get { return (double?) GetValue(MinProperty); }
      set
      {
        SetValue(MinProperty, value);
        ((NumberValidationRule) Rule).Min = value;
      }
    }

    public double? Max
    {
      get { return (double?) GetValue(MaxProperty); }
      set
      {
        SetValue(MaxProperty, value);
        ((NumberValidationRule) Rule).Max = value;
      }
    }

    public double? GetValue()
    {
      if (IsValid())
      {
        return double.Parse(Text);
      }

      return null;
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
      base.OnLostFocus(e);

      if (IsValid())
      {
        Text = string.Format("{0:F1}", double.Parse(Text));
      }
    }
  }
}