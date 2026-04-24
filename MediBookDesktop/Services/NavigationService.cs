using MediBookDesktop.ViewModels;

namespace MediBookDesktop.Services;

public class NavigationService
{
    public event Action<ViewModelBase>? Navigated;

    public void NavigateTo(ViewModelBase viewModel)
    {
        Navigated?.Invoke(viewModel);
    }
}
