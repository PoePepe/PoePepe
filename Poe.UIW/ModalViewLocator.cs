using HanumanInstitute.MvvmDialogs.Wpf;

namespace Poe.UIW;

public class ModalViewLocator : ViewLocatorBase
{
    protected override string GetViewName(object viewModel) => viewModel.GetType().FullName!.Replace("ViewModel", "View");
}