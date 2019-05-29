using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;

using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Reactive;

using DynamicData;
using DynamicData.Cache;

using Microsoft.CSharp.RuntimeBinder;

using ReactiveUI;
using ReactiveUI.Fody.Helpers;

using VendingMachine;

namespace VendingMachine.ViewModels {
    public class VendingMachineViewModel : ViewModelBase, ISupportsActivation {
        public VendingMachine BackingMachine { get; private set; }
        private readonly ObservableAsPropertyHelper<IReadOnlyCollection<StockedProduct>> ? stockedProducts;
        public IReadOnlyCollection<StockedProduct> ? StockedProducts => stockedProducts?.Value;
        public Dictionary<IProduct, int> ShoppingCart = new Dictionary<IProduct, int>();
        private IObserver < (Customer customer, IReadOnlyCollection < (IProduct product, uint quantity) > cart) > ? orderReceiver;
        private Customer Customer { get; } = new Customer(1100m);

        private decimal moneyBalance = 0;
        public decimal MoneyBalance {
            get => moneyBalance;
            set => this.RaiseAndSetIfChanged(ref moneyBalance, value);
        }

        private decimal costOfCart = 0;
        public decimal CostOfCart {
            get => costOfCart;
            set => this.RaiseAndSetIfChanged(ref costOfCart, value);
        }

        private int productsInCart = 0;
        public int ProductsInCart {
            get => productsInCart;
            set => this.RaiseAndSetIfChanged(ref productsInCart, value);
        }

        public decimal TotalSpent { get; private set; } = 0;

        public decimal ProfitFromRounding { get; private set; } = 0;

        public string VendingMachineGreeting => "Welcome to the Vending Machine!";

        public ViewModelActivator Activator { get; }

        public VendingMachineViewModel() {
            IVendingMachineManager manager = new CocaColaVendingMachineManager();
            //TODO: Have a customer associated with each ViewModel, so that I could have two VendingMachineViews in MainWindow that each have different backing ViewModels and Customers.
            //TODO: Would be so cool to then showcase the power of the reactive paradigm by allowing instant reservations (time-limited?) of the stock in a user's cart, preventing other users from buying it.
            //TODO: For bonus points, it wouldn't be very hard to use different threads.
            BackingMachine = new VendingMachine(manager, Currency.SEK, 25);
            BackingMachine.NewIncomingStreamOfOrders(Observable.Create < (Customer customer, IReadOnlyCollection < (IProduct product, uint quantity) > cart) > (
                (IObserver < (Customer customer, IReadOnlyCollection < (IProduct product, uint quantity) > cart) > observer) => {
                    if (orderReceiver == null) {
                        orderReceiver = observer;
                    } else {
                        throw new Exception();
                    }
                    return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
                }));

            var ObservingStockDisposable = BackingMachine.StockStatus.ToCollection().ToProperty(source: this, property: p => p.StockedProducts, out stockedProducts!);

            Activator = new ViewModelActivator();
            var canPlaceOrder = this.WhenAnyValue(
                x => x.ProductsInCart, x => x.MoneyBalance, x => x.CostOfCart,
                (inCart, money, cost) =>
                inCart > 0 && (cost <= money));
            DoPlaceOrder = ReactiveCommand.Create(RunPlaceOrder, canPlaceOrder);

            Func<int, IObservable<bool>> CanInsertCoin = ((denomination) => this.WhenAnyValue(
                x => x.Customer.Money,
                (money) =>
                money >= denomination));

            DoInsertCoin1 = ReactiveCommand.Create<int>(RunInsertCoin, CanInsertCoin(1));
            DoInsertCoin5 = ReactiveCommand.Create<int>(RunInsertCoin, CanInsertCoin(5));
            DoInsertCoin10 = ReactiveCommand.Create<int>(RunInsertCoin, CanInsertCoin(10));
            DoInsertCoin20 = ReactiveCommand.Create<int>(RunInsertCoin, CanInsertCoin(20));
            DoInsertCoin50 = ReactiveCommand.Create<int>(RunInsertCoin, CanInsertCoin(50));
            DoInsertCoin100 = ReactiveCommand.Create<int>(RunInsertCoin, CanInsertCoin(100));
            DoInsertCoin500 = ReactiveCommand.Create<int>(RunInsertCoin, CanInsertCoin(500));
            DoInsertCoin1000 = ReactiveCommand.Create<int>(RunInsertCoin, CanInsertCoin(1000));
            this.WhenActivated(disposables => {
                // Handle ViewModel activation and deactivation.
                Disposable.Create(() => this.HandleDeactivation()).DisposeWith(disposables);
                ObservingStockDisposable.DisposeWith(disposables);
            });
        }

        public ReactiveCommand<int, Unit> DoInsertCoin1 { get; }
        public ReactiveCommand<int, Unit> DoInsertCoin5 { get; }
        public ReactiveCommand<int, Unit> DoInsertCoin10 { get; }
        public ReactiveCommand<int, Unit> DoInsertCoin20 { get; }
        public ReactiveCommand<int, Unit> DoInsertCoin50 { get; }
        public ReactiveCommand<int, Unit> DoInsertCoin100 { get; }
        public ReactiveCommand<int, Unit> DoInsertCoin500 { get; }
        public ReactiveCommand<int, Unit> DoInsertCoin1000 { get; }
        public ReactiveCommand<Unit, Unit> DoPlaceOrder { get; }

        void RunInsertCoin(int denomination) {
            CashOfDenomination den = (CashOfDenomination)denomination;
            bool found = false;
            for (int i = 0; i < VendingMachine.MoneyDenominations.Length; i++) {
                found = den == VendingMachine.MoneyDenominations[i] || found;
            }

            if (!found) {
                throw new Exception("Invalid Money Denomination!");
            }
            if (Customer.Pay(denomination)) {
                MoneyBalance += denomination;
            }
        }

        public void CartItemChanged(AvaloniaPropertyChangedEventArgs e) {
            NumericUpDown spinner = (NumericUpDown)e.Sender;
            IProduct product = (IProduct)spinner.Tag;
            Console.WriteLine(product);
            int quantity = (int)spinner.Value;
            int oldQuantity = ShoppingCart.ContainsKey(product) ? ShoppingCart[product] : 0;
            if (quantity <= 0) {
                ProductsInCart -= oldQuantity;
                ShoppingCart.Remove(product);
            } else {
                ProductsInCart += (quantity - oldQuantity);
                ShoppingCart[product] = quantity;
            }

            CostOfCart += (quantity - oldQuantity) * product.RetailPriceWithVAT;
            //Debugging section:
            if (ShoppingCart.Keys.Count == 0 && ProductsInCart != 0) {
                throw new Exception("Critical invariant does not hold!");
            }
            Console.WriteLine($"Updated item in Shopping Cart: {product} {quantity}");
            foreach (var cartItem in ShoppingCart) {
                if (cartItem.Key != product)
                    Console.WriteLine($"Rest of Shopping Cart: {cartItem.Key} {cartItem.Value}");
            }
        }

        void RunPlaceOrder() {
            var order = new List < (IProduct product, uint quantity) > (ShoppingCart.Keys.Count);
            foreach (var item in ShoppingCart) {
                if (item.Value > 0) {
                    order.Add((item.Key, (uint)item.Value));
                }
            }
            orderReceiver?.OnNext((Customer, order.AsReadOnly())); //TODO: Pass in an Action<bool> that will set cart state and so on depending on if the order succeeds
            //Check this out: https://stackoverflow.com/questions/23483910/best-way-to-get-an-iobservablet-from-actiont

            foreach (var orderedItem in order) {
                ShoppingCart[orderedItem.product] = 0;
            }
            ProductsInCart = 0;

            MoneyBalance -= CostOfCart;
            TotalSpent += CostOfCart;
            CostOfCart = 0;
            List<CashOfDenomination> change = new List<CashOfDenomination>();
            Console.WriteLine($"Total Change: {MoneyBalance}");
            foreach (int denomination in Enum.GetValues(typeof(CashOfDenomination)).Cast<int>().OrderByDescending(x => x)) {
                Console.Write($"Coins of denomination: {denomination} : ");
                int coins = 0;
                while (MoneyBalance >= denomination) {
                    MoneyBalance -= denomination;
                    change.Add((CashOfDenomination)denomination);
                    coins++;
                }
                Console.WriteLine($"{coins}");
            }
            Customer.ReceiveChange(change);
            ProfitFromRounding += MoneyBalance;
            MoneyBalance = 0;
        }

        private void HandleDeactivation() {
            return;
        }

    }
}
