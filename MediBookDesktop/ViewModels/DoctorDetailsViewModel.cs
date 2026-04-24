using System.Collections.ObjectModel;
using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class DoctorDetailsViewModel : ViewModelBase
{
    private readonly User _patient;
    private readonly AvailabilityService _availabilityService;
    private readonly AppointmentService _appointmentService;
    private DateTime? _selectedDate = DateTime.Today;
    private AvailabilitySlot? _selectedSlot;
    private string _reasonForVisit = string.Empty;
    private string _message = string.Empty;
    private Appointment? _confirmedAppointment;

    public DoctorDetailsViewModel(User patient, Doctor doctor, AvailabilityService availabilityService, AppointmentService appointmentService)
    {
        _patient = patient;
        Doctor = doctor;
        _availabilityService = availabilityService;
        _appointmentService = appointmentService;
        LoadSlotsCommand = new AsyncRelayCommand(LoadSlotsAsync);
        BookCommand = new AsyncRelayCommand(BookAsync);
        _ = LoadSlotsAsync();
    }

    public Doctor Doctor { get; }
    public ObservableCollection<AvailabilitySlot> AvailableSlots { get; } = new();

    public DateTime? SelectedDate
    {
        get => _selectedDate;
        set
        {
            if (SetProperty(ref _selectedDate, value))
            {
                _ = LoadSlotsAsync();
            }
        }
    }

    public AvailabilitySlot? SelectedSlot
    {
        get => _selectedSlot;
        set => SetProperty(ref _selectedSlot, value);
    }

    public string ReasonForVisit
    {
        get => _reasonForVisit;
        set => SetProperty(ref _reasonForVisit, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public Appointment? ConfirmedAppointment
    {
        get => _confirmedAppointment;
        set
        {
            if (SetProperty(ref _confirmedAppointment, value))
            {
                OnPropertyChanged(nameof(ConfirmationText));
            }
        }
    }

    public string ConfirmationText => ConfirmedAppointment is null
        ? string.Empty
        : $"{ConfirmedAppointment.AppointmentCode} confirmed with {Doctor.FullName} on {ConfirmedAppointment.AppointmentDateTime:MMM d, yyyy h:mm tt} at {Doctor.Location}.";

    public ICommand LoadSlotsCommand { get; }
    public ICommand BookCommand { get; }

    private async Task LoadSlotsAsync()
    {
        AvailableSlots.Clear();
        var slots = await _availabilityService.GetAvailableSlotsAsync(Doctor.Id, SelectedDate);
        foreach (var slot in slots)
        {
            AvailableSlots.Add(slot);
        }

        Message = AvailableSlots.Count == 0 ? "No open slots for this date." : string.Empty;
    }

    private async Task BookAsync()
    {
        if (SelectedSlot is null)
        {
            Message = "Select an available time slot.";
            return;
        }

        var result = await _appointmentService.BookAppointmentAsync(_patient.Id, Doctor.Id, SelectedSlot.Id, ReasonForVisit);
        Message = result.Message;
        ConfirmedAppointment = result.Appointment;
        if (result.Success)
        {
            ReasonForVisit = string.Empty;
            await LoadSlotsAsync();
        }
    }
}
