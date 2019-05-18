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
                // Bind the 'ExampleCommand' to 'ExampleButton' defined above.
                //this.BindCommand(ViewModel, x => x.StockedProducts, x => x.ProductList)
                //    .DisposeWith(disposables);
                DataContext = VendingMachineViewModel;
            });
            AvaloniaXamlLoader.Load(this);
        }
    }
}
