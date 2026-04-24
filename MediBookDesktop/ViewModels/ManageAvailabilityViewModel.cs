using System.Collections.ObjectModel;
using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class ManageAvailabilityViewModel : ViewModelBase
{
    private readonly DoctorService _doctorService;
    private readonly AvailabilityService _availabilityService;
    private Doctor? _selectedDoctor;
    private AvailabilitySlot? _selectedSlot;
    private DateTime? _selectedDate = DateTime.Today;
    private string _startTimeText = "09:00";
    private string _endTimeText = "09:45";
    private string _message = string.Empty;

    public ManageAvailabilityViewModel(DoctorService doctorService, AvailabilityService availabilityService)
    {
        _doctorService = doctorService;
        _availabilityService = availabilityService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        AddSlotCommand = new AsyncRelayCommand(AddSlotAsync);
        MarkUnavailableCommand = new AsyncRelayCommand(MarkUnavailableAsync);
        _ = LoadAsync();
    }

    public ObservableCollection<Doctor> Doctors { get; } = new();
    public ObservableCollection<AvailabilitySlot> Slots { get; } = new();

    public Doctor? SelectedDoctor
    {
        get => _selectedDoctor;
        set
        {
            if (SetProperty(ref _selectedDoctor, value))
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

    public string StartTimeText
    {
        get => _startTimeText;
        set => SetProperty(ref _startTimeText, value);
    }

    public string EndTimeText
    {
        get => _endTimeText;
        set => SetProperty(ref _endTimeText, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand AddSlotCommand { get; }
    public ICommand MarkUnavailableCommand { get; }

    private async Task LoadAsync()
    {
        Doctors.Clear();
        foreach (var doctor in await _doctorService.GetAllDoctorsAsync(false))
        {
            Doctors.Add(doctor);
        }

        SelectedDoctor ??= Doctors.FirstOrDefault();
        await LoadSlotsAsync();
    }

    private async Task LoadSlotsAsync()
    {
        Slots.Clear();
        if (SelectedDoctor is null)
        {
            return;
        }

        foreach (var slot in await _availabilityService.GetSlotsForDoctorAsync(SelectedDoctor.Id, SelectedDate))
        {
            Slots.Add(slot);
        }
    }

    private async Task AddSlotAsync()
    {
        if (SelectedDoctor is null || SelectedDate is null)
        {
            Message = "Select a doctor and date.";
            return;
        }

        if (!TimeSpan.TryParse(StartTimeText, out var startTime) || !TimeSpan.TryParse(EndTimeText, out var endTime))
        {
            Message = "Use 24-hour time like 09:00 and 09:45.";
            return;
        }

        var result = await _availabilityService.AddSlotAsync(SelectedDoctor.Id, SelectedDate.Value.Date.Add(startTime), SelectedDate.Value.Date.Add(endTime));
        Message = result.Message;
        await LoadSlotsAsync();
    }

    private async Task MarkUnavailableAsync()
    {
        if (SelectedSlot is null)
        {
            Message = "Select a slot.";
            return;
        }

        var result = await _availabilityService.MarkUnavailableAsync(SelectedSlot.Id);
        Message = result.Message;
        await LoadSlotsAsync();
    }
}
