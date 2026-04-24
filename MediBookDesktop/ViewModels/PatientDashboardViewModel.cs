using System.Collections.ObjectModel;
using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class PatientDashboardViewModel : ViewModelBase
{
    private readonly User _patient;
    private readonly AppointmentService _appointmentService;
    private string _message = string.Empty;

    public PatientDashboardViewModel(User patient, AppointmentService appointmentService, Action bookNew, Action myAppointments)
    {
        _patient = patient;
        _appointmentService = appointmentService;
        BookNewCommand = new RelayCommand(bookNew);
        MyAppointmentsCommand = new RelayCommand(myAppointments);
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        _ = LoadAsync();
    }

    public ObservableCollection<Appointment> UpcomingAppointments { get; } = new();
    public ObservableCollection<Appointment> PastAppointments { get; } = new();

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand BookNewCommand { get; }
    public ICommand MyAppointmentsCommand { get; }
    public ICommand RefreshCommand { get; }

    public async Task LoadAsync()
    {
        UpcomingAppointments.Clear();
        PastAppointments.Clear();
        var appointments = await _appointmentService.GetPatientAppointmentsAsync(_patient.Id);
        foreach (var appointment in appointments)
        {
            if (appointment.AppointmentDateTime > DateTime.Now && appointment.Status == AppointmentStatus.Booked)
            {
                UpcomingAppointments.Add(appointment);
            }
            else
            {
                PastAppointments.Add(appointment);
            }
        }

        Message = UpcomingAppointments.Count == 0 ? "No upcoming appointments yet." : string.Empty;
    }
}
