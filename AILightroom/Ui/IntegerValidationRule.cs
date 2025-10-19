using System.Globalization;
using System.Windows.Controls;

namespace AILightroom.Ui
{
  public class IntegerValidationRule : ValidationRule
  {
    public bool? Required { get; set; }
    public int? Min { get; set; }
    public int? Max { get; set; }

    public override ValidationResult Validate(object value, CultureInfo cultureInfo)
    {
      int number;

      if (string.IsNullOrEmpty((string) value)
          && Required.GetValueOrDefault(false))
      {
        return new ValidationResult(false, string.Format("{0} is required.", value));
      }

      if (!int.TryParse((string) value, out number))
      {
        return new ValidationResult(false, string.Format("{0} is not a number.", value));
      }

      if (number < Min.GetValueOrDefault(int.MinValue)
          || number > Max.GetValueOrDefault(int.MaxValue))
      {
        return new ValidationResult(false, string.Format("{0} is out of bounds.", value));
      }

      return new ValidationResult(true, null);
    }
  }
}