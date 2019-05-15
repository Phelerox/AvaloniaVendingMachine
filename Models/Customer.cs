using System;
using System.Reactive;

namespace VendingMachine {
    public class Customer {
        public IObservable<OrderComponent> StartConsuming() {
            throw new NotImplementedException();
            //return Observable.Create();
        }
    }
}
