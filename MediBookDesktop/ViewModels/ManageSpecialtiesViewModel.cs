using System.Collections.ObjectModel;
using System.Windows.Input;
using MediBookDesktop.Commands;
using MediBookDesktop.Models;
using MediBookDesktop.Services;

namespace MediBookDesktop.ViewModels;

public class ManageSpecialtiesViewModel : ViewModelBase
{
    private readonly SpecialtyService _specialtyService;
    private Specialty? _selectedSpecialty;
    private string _name = string.Empty;
    private string _message = string.Empty;

    public ManageSpecialtiesViewModel(SpecialtyService specialtyService)
    {
        _specialtyService = specialtyService;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        NewCommand = new RelayCommand(() =>
        {
            SelectedSpecialty = null;
            Name = string.Empty;
        });
        SaveCommand = new AsyncRelayCommand(SaveAsync);
        DeleteCommand = new AsyncRelayCommand(DeleteAsync);
        _ = LoadAsync();
    }

    public ObservableCollection<Specialty> Specialties { get; } = new();

    public Specialty? SelectedSpecialty
    {
        get => _selectedSpecialty;
        set
        {
            if (SetProperty(ref _selectedSpecialty, value))
            {
                Name = value?.Name ?? string.Empty;
            }
        }
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand RefreshCommand { get; }
    public ICommand NewCommand { get; }
    public ICommand SaveCommand { get; }
    public ICommand DeleteCommand { get; }

    private async Task LoadAsync()
    {
        Specialties.Clear();
        foreach (var specialty in await _specialtyService.GetSpecialtiesAsync())
        {
            Specialties.Add(specialty);
        }
    }

    private async Task SaveAsync()
    {
        var result = await _specialtyService.SaveAsync(new Specialty { Id = SelectedSpecialty?.Id ?? 0, Name = Name });
        Message = result.Message;
        if (result.Success)
        {
            await LoadAsync();
            Name = string.Empty;
            SelectedSpecialty = null;
        }
    }

    private async Task DeleteAsync()
    {
        if (SelectedSpecialty is null)
        {
            Message = "Select a specialty.";
            return;
        }

        var result = await _specialtyService.DeleteAsync(SelectedSpecialty.Id);
        Message = result.Message;
        if (result.Success)
        {
            await LoadAsync();
            Name = string.Empty;
            SelectedSpecialty = null;
        }
    }
}
