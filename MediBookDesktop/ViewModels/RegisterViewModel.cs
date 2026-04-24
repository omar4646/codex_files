using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class RegisterViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly Action<User> _loginSucceeded;
    private readonly Func<LoginViewModel> _loginFactory;
    private string _fullName = string.Empty;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private string _message = string.Empty;

    public RegisterViewModel(AuthService authService, NavigationService navigationService, Action<User> loginSucceeded, Func<LoginViewModel> loginFactory)
    {
        _authService = authService;
        _navigationService = navigationService;
        _loginSucceeded = loginSucceeded;
        _loginFactory = loginFactory;
        RegisterCommand = new AsyncRelayCommand(RegisterAsync);
        BackToLoginCommand = new RelayCommand(() => _navigationService.NavigateTo(_loginFactory()));
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand RegisterCommand { get; }
    public ICommand BackToLoginCommand { get; }

    private async Task RegisterAsync()
    {
        var result = await _authService.RegisterPatientAsync(FullName, Email, Password);
        Message = result.Message;
        if (result.Success && result.User is not null)
        {
            _loginSucceeded(result.User);
        }
    }
}
