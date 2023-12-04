using System;
using HanumanInstitute.MvvmDialogs;

namespace Poe.UIW.ViewModels;

public class ImportOrdersViewModel : ViewModelValidatableBase, IModalDialogViewModel, ICloseable
{
    public ImportOrdersViewModel()
    {
    }
    
    public bool? DialogResult { get; private set; }
    public event EventHandler RequestClose;
}