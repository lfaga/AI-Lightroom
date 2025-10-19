using System.Globalization;
using System.Windows.Controls;

namespace AILightroom.Ui
{
  public class NumberValidationRule : ValidationRule
  {
    public bool? Required { get; set; }
    public double? Min { get; set; }
    public double? Max { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
      double number;

      if (string.IsNullOrEmpty((string) value)
          && Required.GetValueOrDefault(false))
      {
        return new ValidationResult(false, string.Format("{0} is required.", value));
      }

      if (!double.TryParse((string) value, out number))
      {
        return new ValidationResult(false, string.Format("{0} is not a number.", value));
      }

      if (number < Min.GetValueOrDefault(double.MinValue)
          || number > Max.GetValueOrDefault(double.MaxValue))
      {
        return new ValidationResult(false, string.Format("{0} is out of bounds.", value));
      }

      return new ValidationResult(true, null);
    }
  }
}