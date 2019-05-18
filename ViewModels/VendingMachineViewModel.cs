using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;

using DynamicData;

using ReactiveUI;

namespace VendingMachine.ViewModels {
    public class VendingMachineViewModel : ViewModelBase, ISupportsActivation {
        private VendingMachine VendingMachine { get; set; }
        private ObservableAsPropertyHelper<IReadOnlyCollection<StockedProduct>> ? stockedProducts;
        public IReadOnlyCollection<StockedProduct> ? StockedProducts => stockedProducts?.Value;
        public string VendingMachineGreeting => "Welcome to the Vending Machine!";

        public ViewModelActivator Activator { get; }

        public VendingMachineViewModel() {
            IVendingMachineManager manager = new CocaColaVendingMachineManager();
            VendingMachine = new VendingMachine(manager, Currency.SEK, 25);
            var ObservingStockDisposable = VendingMachine.StockStatus.ToCollection().ToProperty(source: this, property: p => p.StockedProducts, out stockedProducts!);
            Activator = new ViewModelActivator();
            this.WhenActivated(disposables => {
                // Handle ViewModel activation and deactivation.
                Disposable.Create(() => this.HandleDeactivation()).DisposeWith(disposables);
                ObservingStockDisposable.DisposeWith(disposables);

            });

        }

        private void HandleDeactivation() {
            return;
        }

    }
}
