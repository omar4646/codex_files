using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class AdminDashboardViewModel : ViewModelBase
{
    private readonly AppointmentService _appointmentService;
    private int _totalDoctors;
    private int _upcomingAppointments;
    private int _appointmentsToday;
    private int _cancelledAppointments;

    public AdminDashboardViewModel(AppointmentService appointmentService)
    {
        _appointmentService = appointmentService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        _ = LoadAsync();
    }

    public int TotalDoctors
    {
        get => _totalDoctors;
        set => SetProperty(ref _totalDoctors, value);
    }

    public int UpcomingAppointments
    {
        get => _upcomingAppointments;
        set => SetProperty(ref _upcomingAppointments, value);
    }

    public int AppointmentsToday
    {
        get => _appointmentsToday;
        set => SetProperty(ref _appointmentsToday, value);
    }

    public int CancelledAppointments
    {
        get => _cancelledAppointments;
        set => SetProperty(ref _cancelledAppointments, value);
    }

    public ICommand RefreshCommand { get; }

    private async Task LoadAsync()
    {
        var stats = await _appointmentService.GetAdminStatsAsync();
        TotalDoctors = stats.TotalDoctors;
        UpcomingAppointments = stats.UpcomingAppointments;
        AppointmentsToday = stats.TodayAppointments;
        CancelledAppointments = stats.CancelledAppointments;
    }
}
