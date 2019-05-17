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
            (new Drink("Coca-Cola", "The Cola-flavored classic!", 14, 1), 30),
            (new Drink("Sprite", "Kids think it's cool to drink.", 12, 1), 15),
            (new Drink("Fanta", "The war-time Cola alternative.", 13, 1), 20),
            (new Drink("Vanilla Coke", "The war-time Cola alternative.", 13, 1), 20),
        };
        public CocaColaVendingMachineManager() { }
        //private IDictionary < VendingMachine, IObservable < (IProduct product, uint quantity) >> machines = new Dictionary<VendingMachine, IObservable<int>>();

        public IObservable < (IProduct product, uint quantity) > SetupVendingMachine(IObservableCache<StockedProduct, int> stockStatus) {
            return Observable.Create < (IProduct product, uint quantity) >
                (observer => {
                    foreach (var productInInitialStock in this.products) {
                        observer.OnNext(productInInitialStock);
                        stockStatus.WatchValue(productInInitialStock.product.GetHashCode()).Where(p => p.Quantity <= 5).Throttle(new TimeSpan(0, 0, 14)).Subscribe(p => observer.OnNext((p.Product, 20 - p.Quantity)));
                    }
                    //observer.OnCompleted();
                    return Disposable.Create(() => Console.WriteLine("Observer has unsubscribed"));
                    //or can return an Action like
                    //return () => Console.WriteLine("Observer has unsubscribed");
                });
        }
    }
}
