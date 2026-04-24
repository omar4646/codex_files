using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class MainViewModel : ViewModelBase
{
    private readonly AuthService _authService;
    private readonly DoctorService _doctorService;
    private readonly AvailabilityService _availabilityService;
    private readonly AppointmentService _appointmentService;
    private readonly SpecialtyService _specialtyService;
    private readonly NavigationService _navigationService;
    private ViewModelBase? _currentViewModel;
    private User? _currentUser;

    public MainViewModel(AuthService authService, DoctorService doctorService, AvailabilityService availabilityService, AppointmentService appointmentService, SpecialtyService specialtyService, NavigationService navigationService)
    {
        _authService = authService;
        _doctorService = doctorService;
        _availabilityService = availabilityService;
        _appointmentService = appointmentService;
        _specialtyService = specialtyService;
        _navigationService = navigationService;
        _navigationService.Navigated += vm => CurrentViewModel = vm;

        NavigatePatientDashboardCommand = new RelayCommand(ShowPatientDashboard);
        NavigateDoctorSearchCommand = new RelayCommand(ShowDoctorSearch);
        NavigateMyAppointmentsCommand = new RelayCommand(ShowMyAppointments);
        NavigateAdminDashboardCommand = new RelayCommand(ShowAdminDashboard);
        NavigateManageDoctorsCommand = new RelayCommand(ShowManageDoctors);
        NavigateManageAvailabilityCommand = new RelayCommand(ShowManageAvailability);
        NavigateManageAppointmentsCommand = new RelayCommand(ShowManageAppointments);
        NavigateManageSpecialtiesCommand = new RelayCommand(ShowManageSpecialties);
        LogoutCommand = new RelayCommand(Logout);

        CurrentViewModel = CreateLoginViewModel();
    }

    public ViewModelBase? CurrentViewModel
    {
        get => _currentViewModel;
        set => SetProperty(ref _currentViewModel, value);
    }

    public User? CurrentUser
    {
        get => _currentUser;
        set
        {
            if (SetProperty(ref _currentUser, value))
            {
                OnPropertyChanged(nameof(IsAuthenticated));
                OnPropertyChanged(nameof(IsPatient));
                OnPropertyChanged(nameof(IsAdmin));
                OnPropertyChanged(nameof(UserDisplayName));
            }
        }
    }

    public bool IsAuthenticated => CurrentUser is not null;
    public bool IsPatient => CurrentUser?.Role == UserRole.Patient;
    public bool IsAdmin => CurrentUser?.Role == UserRole.Admin;
    public string UserDisplayName => CurrentUser?.FullName ?? "Guest";

    public ICommand NavigatePatientDashboardCommand { get; }
    public ICommand NavigateDoctorSearchCommand { get; }
    public ICommand NavigateMyAppointmentsCommand { get; }
    public ICommand NavigateAdminDashboardCommand { get; }
    public ICommand NavigateManageDoctorsCommand { get; }
    public ICommand NavigateManageAvailabilityCommand { get; }
    public ICommand NavigateManageAppointmentsCommand { get; }
    public ICommand NavigateManageSpecialtiesCommand { get; }
    public ICommand LogoutCommand { get; }

    private LoginViewModel CreateLoginViewModel()
    {
        LoginViewModel? login = null;
        RegisterViewModel RegisterFactory() => new(_authService, _navigationService, HandleLoginSucceeded, () => login ?? CreateLoginViewModel());
        login = new LoginViewModel(_authService, _navigationService, HandleLoginSucceeded, RegisterFactory);
        return login;
    }

    private void HandleLoginSucceeded(User user)
    {
        CurrentUser = user;
        if (user.Role == UserRole.Admin)
        {
            ShowAdminDashboard();
        }
        else
        {
            ShowPatientDashboard();
        }
    }

    private void Logout()
    {
        _authService.Logout();
        CurrentUser = null;
        CurrentViewModel = CreateLoginViewModel();
    }

    private void ShowPatientDashboard()
    {
        if (CurrentUser is not null)
        {
            CurrentViewModel = new PatientDashboardViewModel(CurrentUser, _appointmentService, ShowDoctorSearch, ShowMyAppointments);
        }
    }

    private void ShowDoctorSearch()
    {
        if (CurrentUser is not null)
        {
            CurrentViewModel = new DoctorSearchViewModel(CurrentUser, _doctorService, _availabilityService, _appointmentService, doctor => CurrentViewModel = new DoctorDetailsViewModel(CurrentUser, doctor, _availabilityService, _appointmentService));
        }
    }

    private void ShowMyAppointments()
    {
        if (CurrentUser is not null)
        {
            CurrentViewModel = new MyAppointmentsViewModel(CurrentUser, _appointmentService, _availabilityService);
        }
    }

    private void ShowAdminDashboard() => CurrentViewModel = new AdminDashboardViewModel(_appointmentService);
    private void ShowManageDoctors() => CurrentViewModel = new ManageDoctorsViewModel(_doctorService);
    private void ShowManageAvailability() => CurrentViewModel = new ManageAvailabilityViewModel(_doctorService, _availabilityService);
    private void ShowManageAppointments() => CurrentViewModel = new ManageAppointmentsViewModel(_doctorService, _appointmentService);
    private void ShowManageSpecialties() => CurrentViewModel = new ManageSpecialtiesViewModel(_specialtyService);
}
