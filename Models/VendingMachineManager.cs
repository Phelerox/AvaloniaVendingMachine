using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;

using DynamicData;

using ReactiveUI;

namespace VendingMachine {

    public interface IVendingMachineManager {

        public IObservable < (IProduct product, uint quantity) > SetupVendingMachine(IObservableCache<StockedProduct, int> stockStatus);
    }

    public class CocaColaVendingMachineManager : IVendingMachineManager {
        private readonly List < (IProduct product, uint quantity) > products = new List < (IProduct product, uint quantity) > () {
            (new Drink("Coca-Cola", "The Cola-flavored classic!", 12, 1, VendingMachine.VAT, extraStoreMarkupFactor : 1.1m), 30),
            (new Drink("Sprite", "Kids think it's cool to drink.", 9, 1, VendingMachine.VAT, extraStoreMarkupFactor : 1.1m), 15),
            (new Drink("Fanta", "The war-time Cola alternative.", 10, 1, VendingMachine.VAT, extraStoreMarkupFactor : 1.1m), 20),
            (new Drink("Vanilla Coke", "The war-time Cola alternative.", 15, 1, VendingMachine.VAT, extraStoreMarkupFactor : 1.1m), 20),
            (new Drink("Pepsi", "The One True Cola-flavored classic!", 11, 1, VendingMachine.VAT, extraStoreMarkupFactor : 1.1m), 30),
            (new Drink("Mountain Dew", "It's green!", 13, 1, VendingMachine.VAT, extraStoreMarkupFactor : 1.1m), 15),
            (new Drink("Fanta Lemon", "Lemon!", 12, 1, VendingMachine.VAT, extraStoreMarkupFactor : 1.1m), 20),
            (new Drink("Cherry Coke", "The war-time Cola alternative.", 14, 1, VendingMachine.VAT, extraStoreMarkupFactor : 1.1m), 20),
        };
        public CocaColaVendingMachineManager() { }
        //private IDictionary < VendingMachine, IObservable < (IProduct product, uint quantity) >> machines = new Dictionary<VendingMachine, IObservable<int>>();

        public IObservable < (IProduct product, uint quantity) > SetupVendingMachine(IObservableCache<StockedProduct, int> stockStatus) {
            return Observable.Create < (IProduct product, uint quantity) >
                (observer => {
                    foreach (var productInInitialStock in this.products) {
                        observer.OnNext(productInInitialStock);
                        stockStatus.WatchValue(productInInitialStock.product.GetHashCode()).Where(p => p.Quantity <= 5).Throttle(new TimeSpan(0, 0, 8))
                            .Subscribe(p =>
                                observer.OnNext((p.Product, productInInitialStock.quantity - p.Quantity)));
                    }
                    //observer.OnCompleted();
                    return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
                });
        }
    }
}
