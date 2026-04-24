using System.Windows;
using MediBookDesktop.Data;
using MediBookDesktop.Services;
using MediBookDesktop.ViewModels;

namespace MediBookDesktop;

public partial class App : Application
{
    protected override async void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var contextFactory = new AppDbContextFactory();
        await DatabaseInitializer.InitializeAsync(contextFactory);

        var authService = new AuthService(contextFactory);
        var doctorService = new DoctorService(contextFactory);
        var availabilityService = new AvailabilityService(contextFactory);
        var appointmentService = new AppointmentService(contextFactory);
        var specialtyService = new SpecialtyService(contextFactory);
        var navigationService = new NavigationService();

        var mainViewModel = new MainViewModel(authService, doctorService, availabilityService, appointmentService, specialtyService, navigationService);
        var window = new MainWindow { DataContext = mainViewModel };
        window.Show();
    }
}
