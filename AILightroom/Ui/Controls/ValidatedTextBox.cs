using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace AILightroom.Ui.Controls
{
  public abstract class ValidatedTextBox : TextBox, IValidatedControl
  {
    public static readonly DependencyProperty ErrorStyleProperty =
      DependencyProperty.Register(
        "ErrorStyle",
        typeof (Style),
        typeof (IntTextBox),
        new FrameworkPropertyMetadata(null));

    private Style _defaultStyle;
    private object _defaultTooltip;

    protected ValidationRule Rule;

    protected ValidatedTextBox(ValidationRule rule)
    {
      Rule = rule;
    }

    public new Style Style
    {
      get { return base.Style; }

      set
      {
        if (_defaultStyle == null && value != null)
        {
          _defaultStyle = value;
          if (ToolTip != null) _defaultTooltip = ToolTip;
        }
        base.Style = value;
      }
    }

    public Style ErrorStyle
    {
      get { return (Style) GetValue(ErrorStyleProperty); }
      set { SetValue(ErrorStyleProperty, value); }
    }

    public bool IsValid()
    {
      var result = Rule.Validate(Text, CultureInfo.InvariantCulture);
      if (result.IsValid)
      {
        Style = _defaultStyle;
        ToolTip = _defaultTooltip;
      }
      else
      {
        Style = ErrorStyle;
        ToolTip = new ToolTip {Content = result.ErrorContent};
      }
      return result.IsValid;
    }

    protected override void OnTextChanged(TextChangedEventArgs e)
    {
      base.OnTextChanged(e);

      IsValid();
    }
  }
}