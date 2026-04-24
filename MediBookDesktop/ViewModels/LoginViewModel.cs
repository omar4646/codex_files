using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class LoginViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly NavigationService _navigationService;
    private readonly Action<User> _loginSucceeded;
    private readonly Func<RegisterViewModel> _registerFactory;
    private string _email = "admin@medibook.local";
    private string _password = "Admin123!";
    private string _message = string.Empty;

    public LoginViewModel(AuthService authService, NavigationService navigationService, Action<User> loginSucceeded, Func<RegisterViewModel> registerFactory)
    {
        _authService = authService;
        _navigationService = navigationService;
        _loginSucceeded = loginSucceeded;
        _registerFactory = registerFactory;
        LoginCommand = new AsyncRelayCommand(LoginAsync);
        ShowRegisterCommand = new RelayCommand(() => _navigationService.NavigateTo(_registerFactory()));
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

    public ICommand LoginCommand { get; }
    public ICommand ShowRegisterCommand { get; }

    private async Task LoginAsync()
    {
        var result = await _authService.LoginAsync(Email, Password);
        Message = result.Message;
        if (result.Success && result.User is not null)
        {
            _loginSucceeded(result.User);
        }
    }
}
