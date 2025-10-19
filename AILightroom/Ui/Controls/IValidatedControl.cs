using System.Windows;

namespace AILightroom.Ui.Controls
{
  public interface IValidatedControl
  {
    Style ErrorStyle { get; set; }
    bool IsValid();
  }
}