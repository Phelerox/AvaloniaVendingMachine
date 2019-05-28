using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;

using ReactiveUI;

using VendingMachine.ViewModels;

namespace VendingMachine.Views {
    public class VendingMachineView : ReactiveUserControl<VendingMachineViewModel> {
        public VendingMachineViewModel VendingMachineViewModel => new VendingMachineViewModel();

        public VendingMachineView() {
            InitializeComponent();
        }

        private void InitializeComponent() {
            this.WhenActivated(disposables => {
                DataContext = VendingMachineViewModel;
                //It took me a long time to figure out how to achieve what the line below does
                //it will notify the CartItemChanged method in the ViewModel whenever *any* NumericUpDown instance changes.
                NumericUpDown.ValueProperty.Changed.AddClassHandler<NumericUpDown>(x => ((VendingMachineViewModel)this.ViewModel).CartItemChanged).DisposeWith(disposables);

            });
            AvaloniaXamlLoader.Load(this);
        }

    }
}
