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
  ///   <MyNamespace:IntTextBox />
  /// </summary>
  public class IntTextBox : ValidatedTextBox
  {
    public static readonly DependencyProperty RequiredProperty =
      DependencyProperty.Register(
        "Required",
        typeof (bool?),
        typeof (IntTextBox),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty MinProperty =
      DependencyProperty.Register(
        "Min",
        typeof (int?),
        typeof (IntTextBox),
        new FrameworkPropertyMetadata(null));

    public static readonly DependencyProperty MaxProperty =
      DependencyProperty.Register(
        "Max",
        typeof (int?),
        typeof (IntTextBox),
        new FrameworkPropertyMetadata(null));


    public IntTextBox() : base(new IntegerValidationRule())
    {
    }

    public bool? Required
    {
      get { return (bool?) GetValue(RequiredProperty); }
      set
      {
        SetValue(RequiredProperty, value);
        ((IntegerValidationRule) Rule).Required = value;
      }
    }

    public int? Min
    {
      get { return (int?) GetValue(MinProperty); }
      set
      {
        SetValue(MinProperty, value);
        ((IntegerValidationRule) Rule).Min = value;
      }
    }

    public int? Max
    {
      get { return (int?) GetValue(MaxProperty); }
      set
      {
        SetValue(MaxProperty, value);
        ((IntegerValidationRule) Rule).Max = value;
      }
    }

    public int? GetValue()
    {
      if (IsValid())
      {
        return int.Parse(Text);
      }

      return null;
    }

    protected override void OnLostFocus(RoutedEventArgs e)
    {
      base.OnLostFocus(e);

      if (IsValid())
      {
        Text = string.Format("{0:D}", int.Parse(Text));
      }
    }
  }
}