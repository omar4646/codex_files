using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class ManageDoctorsViewModel : ViewModelBase
{
    private readonly DoctorService _doctorService;
    private Doctor? _selectedDoctor;
    private int _editingDoctorId;
    private string _fullName = string.Empty;
    private string _specialty = string.Empty;
    private string _location = string.Empty;
    private decimal _consultationFee = 100;
    private string _bio = string.Empty;
    private bool _isActive = true;
    private string _message = string.Empty;

    public ManageDoctorsViewModel(DoctorService doctorService)
    {
        _doctorService = doctorService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        NewDoctorCommand = new RelayCommand(ClearForm);
        SaveDoctorCommand = new AsyncRelayCommand(SaveAsync);
        DeleteDoctorCommand = new AsyncRelayCommand(DeleteAsync);
        _ = LoadAsync();
    }

    public ObservableCollection<Doctor> Doctors { get; } = new();
    public ObservableCollection<string> Specialties { get; } = new();

    public Doctor? SelectedDoctor
    {
        get => _selectedDoctor;
        set
        {
            if (SetProperty(ref _selectedDoctor, value) && value is not null)
            {
                _editingDoctorId = value.Id;
                FullName = value.FullName;
                Specialty = value.Specialty;
                Location = value.Location;
                ConsultationFee = value.ConsultationFee;
                Bio = value.Bio;
                IsActive = value.IsActive;
            }
        }
    }

    public string FullName
    {
        get => _fullName;
        set => SetProperty(ref _fullName, value);
    }

    public string Specialty
    {
        get => _specialty;
        set => SetProperty(ref _specialty, value);
    }

    public string Location
    {
        get => _location;
        set => SetProperty(ref _location, value);
    }

    public decimal ConsultationFee
    {
        get => _consultationFee;
        set => SetProperty(ref _consultationFee, value);
    }

    public string Bio
    {
        get => _bio;
        set => SetProperty(ref _bio, value);
    }

    public bool IsActive
    {
        get => _isActive;
        set => SetProperty(ref _isActive, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand NewDoctorCommand { get; }
    public ICommand SaveDoctorCommand { get; }
    public ICommand DeleteDoctorCommand { get; }

    private async Task LoadAsync()
    {
        Doctors.Clear();
        foreach (var doctor in await _doctorService.GetAllDoctorsAsync())
        {
            Doctors.Add(doctor);
        }

        Specialties.Clear();
        foreach (var specialty in await _doctorService.GetSpecialtiesAsync())
        {
            Specialties.Add(specialty);
        }
    }

    private void ClearForm()
    {
        _editingDoctorId = 0;
        SelectedDoctor = null;
        FullName = string.Empty;
        Specialty = Specialties.FirstOrDefault() ?? string.Empty;
        Location = string.Empty;
        ConsultationFee = 100;
        Bio = string.Empty;
        IsActive = true;
        Message = string.Empty;
    }

    private async Task SaveAsync()
    {
        var doctor = new Doctor
        {
            Id = _editingDoctorId,
            FullName = FullName,
            Specialty = Specialty,
            Location = Location,
            ConsultationFee = ConsultationFee,
            Bio = Bio,
            IsActive = IsActive
        };

        var result = await _doctorService.SaveDoctorAsync(doctor);
        Message = result.Message;
        if (result.Success)
        {
            await LoadAsync();
            ClearForm();
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedDoctor is null)
        {
            Message = "Select a doctor to delete.";
            return;
        }

        if (MessageBox.Show($"Delete {SelectedDoctor.FullName}?", "Confirm delete", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
        {
            return;
        }

        var result = await _doctorService.DeleteDoctorAsync(SelectedDoctor.Id);
        Message = result.Message;
        if (result.Success)
        {
            await LoadAsync();
            ClearForm();
        }
    }
}
