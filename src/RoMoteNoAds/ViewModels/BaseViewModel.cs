using CommunityToolkit.Mvvm.ComponentModel;

namespace RoMoteNoAds.ViewModels;

/// <summary>
/// Base ViewModel with common functionality.
/// </summary>
public partial class BaseViewModel : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string? _errorMessage;

    public bool IsNotBusy => !IsBusy;

    protected void SetError(string message)
    {
        ErrorMessage = message;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Task.Delay(3000);
            if (ErrorMessage == message)
                ErrorMessage = null;
        });
    }

    protected void ClearError()
    {
        ErrorMessage = null;
    }
}
