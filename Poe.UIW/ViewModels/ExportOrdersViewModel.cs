using System;
using HanumanInstitute.MvvmDialogs;

namespace Poe.UIW.ViewModels;

public class ExportOrdersViewModel : ViewModelValidatableBase, IModalDialogViewModel, ICloseable
{
    public ExportOrdersViewModel()
    {
    }
    
    public bool? DialogResult { get; private set; }
    public event EventHandler RequestClose;
}