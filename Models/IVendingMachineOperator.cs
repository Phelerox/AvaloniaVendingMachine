using System;
using System.Collections.Generic;

namespace VendingMachine {
    public interface IVendingMachineOperator {

        IEnumerable < (IProduct, long) > SetupVendingMachine(IVendingMachine machine);
        IEnumerable < (IProduct, long) > RefillVendingMachine(IVendingMachine machine);
    }
}
