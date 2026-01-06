namespace TejasCareConnect;

public partial class LoadingPage : ContentPage
{
    public LoadingPage()
    {
        InitializeComponent();
    }

    public void UpdateStatus(string message)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            StatusLabel.Text = message;
        });
    }

    public void ShowError(string error)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            ErrorLabel.Text = error;
            ErrorLabel.IsVisible = true;
            StatusLabel.IsVisible = false;
        });
    }
}
