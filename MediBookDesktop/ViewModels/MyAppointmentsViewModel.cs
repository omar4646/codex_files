using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class MyAppointmentsViewModel : ViewModelBase
{
    private readonly User _patient;
    private readonly AppointmentService _appointmentService;
    private readonly AvailabilityService _availabilityService;
    private Appointment? _selectedAppointment;
    private AvailabilitySlot? _selectedNewSlot;
    private string _message = string.Empty;

    public MyAppointmentsViewModel(User patient, AppointmentService appointmentService, AvailabilityService availabilityService)
    {
        _patient = patient;
        _appointmentService = appointmentService;
        _availabilityService = availabilityService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        CancelCommand = new AsyncRelayCommand(CancelAsync);
        RescheduleCommand = new AsyncRelayCommand(RescheduleAsync);
        _ = LoadAsync();
    }

    public ObservableCollection<Appointment> Appointments { get; } = new();
    public ObservableCollection<AvailabilitySlot> RescheduleSlots { get; } = new();

    public Appointment? SelectedAppointment
    {
        get => _selectedAppointment;
        set
        {
            if (SetProperty(ref _selectedAppointment, value))
            {
                _ = LoadRescheduleSlotsAsync();
            }
        }
    }

    public AvailabilitySlot? SelectedNewSlot
    {
        get => _selectedNewSlot;
        set => SetProperty(ref _selectedNewSlot, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand CancelCommand { get; }
    public ICommand RescheduleCommand { get; }

    private async Task LoadAsync()
    {
        Appointments.Clear();
        foreach (var appointment in await _appointmentService.GetPatientAppointmentsAsync(_patient.Id))
        {
            Appointments.Add(appointment);
        }

        Message = Appointments.Count == 0 ? "You do not have any appointments yet." : string.Empty;
    }

    private async Task LoadRescheduleSlotsAsync()
    {
        RescheduleSlots.Clear();
        SelectedNewSlot = null;
        if (SelectedAppointment is null || !SelectedAppointment.IsUpcoming)
        {
            return;
        }

        var slots = await _availabilityService.GetAvailableSlotsAsync(SelectedAppointment.DoctorId);
        foreach (var slot in slots.Where(slot => slot.Id != SelectedAppointment.AvailabilitySlotId).Take(30))
        {
            RescheduleSlots.Add(slot);
        }
    }

    private async Task CancelAsync()
    {
        if (SelectedAppointment is null)
        {
            Message = "Select an appointment to cancel.";
            return;
        }

        if (MessageBox.Show("Cancel this appointment?", "Confirm cancellation", MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
        {
            return;
        }

        var result = await _appointmentService.CancelAppointmentAsync(SelectedAppointment.Id);
        Message = result.Message;
        await LoadAsync();
    }

    private async Task RescheduleAsync()
    {
        if (SelectedAppointment is null || SelectedNewSlot is null)
        {
            Message = "Select an appointment and a new slot.";
            return;
        }

        var result = await _appointmentService.RescheduleAppointmentAsync(SelectedAppointment.Id, SelectedNewSlot.Id);
        Message = result.Message;
        await LoadAsync();
        await LoadRescheduleSlotsAsync();
    }
}
