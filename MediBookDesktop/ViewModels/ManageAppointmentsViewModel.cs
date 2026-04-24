using System.Collections.ObjectModel;
using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class ManageAppointmentsViewModel : ViewModelBase
{
    private readonly DoctorService _doctorService;
    private readonly AppointmentService _appointmentService;
    private Doctor? _selectedDoctorFilter;
    private string _patientFilter = string.Empty;
    private DateTime? _selectedDate;
    private string _statusFilter = "All";
    private Appointment? _selectedAppointment;
    private AppointmentStatus _newStatus = AppointmentStatus.Booked;
    private string _message = string.Empty;

    public ManageAppointmentsViewModel(DoctorService doctorService, AppointmentService appointmentService)
    {
        _doctorService = doctorService;
        _appointmentService = appointmentService;
        StatusOptions = new ObservableCollection<string>(new[] { "All", "Booked", "Completed", "Cancelled", "NoShow" });
        StatusUpdateOptions = new ObservableCollection<AppointmentStatus>(Enum.GetValues<AppointmentStatus>());
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        UpdateStatusCommand = new AsyncRelayCommand(UpdateStatusAsync);
        _ = LoadFiltersAndAppointmentsAsync();
    }

    public ObservableCollection<Doctor> Doctors { get; } = new();
    public ObservableCollection<Appointment> Appointments { get; } = new();
    public ObservableCollection<string> StatusOptions { get; }
    public ObservableCollection<AppointmentStatus> StatusUpdateOptions { get; }

    public Doctor? SelectedDoctorFilter
    {
        get => _selectedDoctorFilter;
        set => SetProperty(ref _selectedDoctorFilter, value);
    }

    public string PatientFilter
    {
        get => _patientFilter;
        set => SetProperty(ref _patientFilter, value);
    }

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set => SetProperty(ref _selectedDate, value);
    }

    public string StatusFilter
    {
        get => _statusFilter;
        set => SetProperty(ref _statusFilter, value);
    }

    public Appointment? SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            if (SetProperty(ref _selectedAppointment, value) && value is not null)
            {
                NewStatus = value.Status;
            }
        }
    }

    public AppointmentStatus NewStatus
    {
        get => _newStatus;
        set => SetProperty(ref _newStatus, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand UpdateStatusCommand { get; }

    private async Task LoadFiltersAndAppointmentsAsync()
    {
        Doctors.Clear();
        foreach (var doctor in await _doctorService.GetAllDoctorsAsync())
        {
            Doctors.Add(doctor);
        }

        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        Appointments.Clear();
        AppointmentStatus? status = null;
        if (StatusFilter != "All" && Enum.TryParse<AppointmentStatus>(StatusFilter, out var parsed))
        {
            status = parsed;
        }

        var appointments = await _appointmentService.GetAppointmentsAsync(SelectedDoctorFilter?.Id, PatientFilter, SelectedDate, status);
        foreach (var appointment in appointments)
        {
            Appointments.Add(appointment);
        }

        Message = Appointments.Count == 0 ? "No appointments match the current filters." : string.Empty;
    }

    private async Task UpdateStatusAsync()
    {
        if (SelectedAppointment is null)
        {
            Message = "Select an appointment.";
            return;
        }

        var result = await _appointmentService.UpdateStatusAsync(SelectedAppointment.Id, NewStatus);
        Message = result.Message;
        await LoadAsync();
    }
}
