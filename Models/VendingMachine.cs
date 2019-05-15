using System;
using System.Collections.Generic;

namespace VendingMachine {
    public abstract class BaseVendingMachine : IVendingMachine {
        private readonly IVendingMachineOperator op;
        protected BaseVendingMachine(IVendingMachineOperator op) {
            this.op = op;
            //No need to worry about null because of C# 8.0.
            //   (It would be caught at compile-time)
            //To have IVendingMachineOperator be nullable,
            //I'd have to declare it as IVendingMachineOperator?
            this.op.SetupVendingMachine(this);
        }
    }

    public class VendingMachine : BaseVendingMachine {
        public VendingMachine(IVendingMachineOperator op) : base(op) {

        }
    }
}
