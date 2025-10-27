using System;
using System.Windows;
using System.Windows.Input;

namespace AILightroom
{
  public enum MsgBoxResult
  {
    Ok = 1,
    Cancel = 2,
    Abort = 3,
    Retry = 4,
    Ignore = 5,
    Yes = 6,
    No = 7
  }

  /// <summary>
  ///   Interaction logic for InputBox.xaml
  /// </summary>
  public partial class InputBox : Window, IDisposable
  {
    private MsgBoxResult _return;
    public string Response;

    public InputBox()
    {
      InitializeComponent();
    }

    public void Dispose()
    {
      Close();
    }

    protected override void OnInitialized(EventArgs e)
    {
      base.OnInitialized(e);
      TxtInput.Focus();
    }

    public MsgBoxResult Show(Window owner, string caption, string message, string defaultValue = "")
    {
      Title = caption;
      LblMessage.Content = message;
      TxtInput.Text = defaultValue;

      BtnCancel.Click += (sender, args) => { Cancel(); };
      BtnOk.Click += (sender, args) => { Ok(); };

      WindowStartupLocation = WindowStartupLocation.CenterOwner;
      Owner = owner;

      ShowDialog();

      return _return;
    }

    private void TxtInput_OnKeyDown(object sender, KeyEventArgs e)
    {
      if (e.Key == Key.Enter)
      {
        Ok();
      }

      if (e.Key == Key.Escape)
      {
        Cancel();
      }
    }

    private void Cancel()
    {
      Response = string.Empty;
      _return = MsgBoxResult.Cancel;
      Hide();
    }

    private void Ok()
    {
      Response = TxtInput.Text;
      _return = MsgBoxResult.Ok;
      Hide();
    }
  }
}