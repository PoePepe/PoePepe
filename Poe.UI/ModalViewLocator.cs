using HanumanInstitute.MvvmDialogs.Avalonia;

namespace Poe.UI;

public class ModalViewLocator : ViewLocatorBase
{
    protected override string GetViewName(object viewModel) => viewModel.GetType().FullName!.Replace("ViewModel", "View");
}