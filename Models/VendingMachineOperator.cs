using System;
using System.Collections.Generic;

namespace VendingMachine {
    public class CocaColaVendingMachineOperator : IVendingMachineOperator {

        public IEnumerable < (IProduct, long) > SetupVendingMachine(IVendingMachine machine) {
            return new List < (IProduct, long) > () {
                (new Drink("Coca-Cola", "The Cola-flavored classic!", 14, 1), 30),
                (new Drink("Sprite", "Kids think it's cool to drink.", 12, 1), 15),
                (new Drink("Fanta", "The war-time Cola alternative.", 13, 1), 20),
                (new Drink("Vanilla Coke", "The war-time Cola alternative.", 13, 1), 20),

            };
        }
        public IEnumerable < (IProduct, long) > RefillVendingMachine(IVendingMachine machine) {
            return new List < (IProduct, long) > () {
                (new Drink("Coca-Cola", "The Cola-flavored classic!", 14, 1), 10),
            };
        }
    }
}
