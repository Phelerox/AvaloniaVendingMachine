using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

using DynamicData;

namespace VendingMachine.ViewModels {
    public class VendingMachineViewModel : ViewModelBase {
        public string VendingMachineGreeting => "Welcome to the Vending Machine!";

        public VendingMachineViewModel(VendingMachine vendingMachine) {
            Items = vendingMachine.StockStatus;
        }

        public IObservable<IChangeSet<StockedProduct, int>> Items { get; }
    }
}
