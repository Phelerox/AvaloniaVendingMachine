using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Disposables;
// using System.Reactive.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;

using DynamicData;

using Microsoft.CSharp.RuntimeBinder;

using ReactiveUI;

namespace VendingMachine.ViewModels {
    public class VendingMachineViewModel : ViewModelBase, ISupportsActivation {
        private VendingMachine VendingMachine { get; set; }
        private ObservableAsPropertyHelper<IReadOnlyCollection<StockedProduct>> ? stockedProducts;
        public IReadOnlyCollection<StockedProduct> ? StockedProducts => stockedProducts?.Value;
        private IDictionary<IProduct, int> shoppingCart = new Dictionary<IProduct, int>();
        public IReadOnlyDictionary<IProduct, int> ShoppingCart = shoppingCart.AsReadOnlyCollection();

        public string VendingMachineGreeting => "Welcome to the Vending Machine!";

        public ViewModelActivator Activator { get; }

        public VendingMachineViewModel() {
            IVendingMachineManager manager = new CocaColaVendingMachineManager();
            //TODO: Have a customer associated with each ViewModel, so that I could have two VendingMachineViews in MainWindow that each have different backing ViewModels and Customers.
            //TODO: Would be so cool to then showcase the power of the reactive paradigm by allowing instant reservations (time-limited?) of the stock in a user's cart, preventing other users from buying it. 
            VendingMachine = new VendingMachine(manager, Currency.SEK, 25);
            var ObservingStockDisposable = VendingMachine.StockStatus.ToCollection().ToProperty(source: this, property: p => p.StockedProducts, out stockedProducts!);

            Activator = new ViewModelActivator();
            DoInsertCoin = ReactiveCommand.Create<int>(RunInsertCoin);
            DoUpdateCart = ReactiveCommand.Create<object>(RunUpdateCart);
            DoPlaceOrder = ReactiveCommand.Create<int>(RunPlaceOrder);
            this.WhenActivated(disposables => {
                // Handle ViewModel activation and deactivation.
                Disposable.Create(() => this.HandleDeactivation()).DisposeWith(disposables);
                ObservingStockDisposable.DisposeWith(disposables);
            });
        }

        public ReactiveCommand<int, Unit> DoInsertCoin { get; }
        public ReactiveCommand<object, Unit> DoUpdateCart { get; }
        public ReactiveCommand<int, Unit> DoPlaceOrder { get; }

        void RunInsertCoin(int denomination) {
            throw new NotImplementedException();
        }

        public void CartItemChanged(AvaloniaPropertyChangedEventArgs e) {
            NumericUpDown spinner = (NumericUpDown)e.Sender;
            // ... Get Value.
            // ... Set Window Title.
            IProduct product = (IProduct)spinner.Tag;
            int quantity = (int)spinner.Value;
            if (quantity <= 0) {
                shoppingCart.Remove(product);
            } else {
                shoppingCart[product] = quantity;
            }
            Console.WriteLine($"Updated item in Shopping Cart: {product} {quantity}");
            foreach (var cartItem in shoppingCart) {
                if (cartItem.Key != product)
                    Console.WriteLine($"Rest of Shopping Cart: {cartItem.Key} {cartItem.Value}");
            }

        }

        void RunUpdateCart(object parameters) {
            var parameterTuple = (Tuple<object, object>)parameters;
            (IProduct product, int quantity) = ((IProduct)parameterTuple.Item1, (int)parameterTuple.Item2);
            if (quantity <= 0) {
                shoppingCart.Remove(product);
            } else {
                shoppingCart[product] = quantity;
            }
        }

        void RunPlaceOrder(int something) {

        }

        private void HandleDeactivation() {
            return;
        }

    }
}
