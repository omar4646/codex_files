using System.Collections.ObjectModel;
using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class DoctorSearchViewModel : ViewModelBase
{
    private readonly DoctorService _doctorService;
    private readonly Action<Doctor> _openDoctor;
    private string _searchText = string.Empty;
    private string _selectedSpecialty = "All";
    private string _selectedLocation = "All";
    private DateTime? _availableDate;
    private Doctor? _selectedDoctor;
    private string _message = string.Empty;

    public DoctorSearchViewModel(User patient, DoctorService doctorService, AvailabilityService availabilityService, AppointmentService appointmentService, Action<Doctor> openDoctor)
    {
        _doctorService = doctorService;
        _openDoctor = openDoctor;
        SearchCommand = new AsyncRelayCommand(SearchAsync);
        OpenDoctorCommand = new RelayCommand(parameter =>
        {
            if (parameter is Doctor doctor)
            {
                SelectedDoctor = doctor;
                _openDoctor(doctor);
            }
            else if (SelectedDoctor is not null)
            {
                _openDoctor(SelectedDoctor);
            }
        });
        _ = LoadFiltersAndSearchAsync();
    }

    public ObservableCollection<Doctor> Doctors { get; } = new();
    public ObservableCollection<string> SpecialtyOptions { get; } = new();
    public ObservableCollection<string> LocationOptions { get; } = new();

    public string SearchText
    {
        get => _searchText;
        set => SetProperty(ref _searchText, value);
    }

    public string SelectedSpecialty
    {
        get => _selectedSpecialty;
        set => SetProperty(ref _selectedSpecialty, value);
    }

    public string SelectedLocation
    {
        get => _selectedLocation;
        set => SetProperty(ref _selectedLocation, value);
    }

    public DateTime? AvailableDate
    {
        get => _availableDate;
        set => SetProperty(ref _availableDate, value);
    }

    public Doctor? SelectedDoctor
    {
        get => _selectedDoctor;
        set => SetProperty(ref _selectedDoctor, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand SearchCommand { get; }
    public ICommand OpenDoctorCommand { get; }

    private async Task LoadFiltersAndSearchAsync()
    {
        SpecialtyOptions.Clear();
        SpecialtyOptions.Add("All");
        foreach (var specialty in await _doctorService.GetSpecialtiesAsync())
        {
            SpecialtyOptions.Add(specialty);
        }

        LocationOptions.Clear();
        LocationOptions.Add("All");
        foreach (var location in await _doctorService.GetLocationsAsync())
        {
            LocationOptions.Add(location);
        }

        await SearchAsync();
    }

    private async Task SearchAsync()
    {
        Doctors.Clear();
        var doctors = await _doctorService.SearchDoctorsAsync(SearchText, SelectedSpecialty, SelectedLocation, AvailableDate);
        foreach (var doctor in doctors)
        {
            Doctors.Add(doctor);
        }

        Message = Doctors.Count == 0 ? "No doctors match the current search." : string.Empty;
    }
}
