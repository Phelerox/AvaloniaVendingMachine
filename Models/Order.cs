using System;
namespace VendingMachine {
    public interface IOrder {

        ulong OrderNumber { get; }
        double TotalPrice(decimal vatRate = 0);

    }

    public struct OrderComponent : IEquatable<OrderComponent> {
        /// Intentionally skipped making an IOrderComponent interface,
        // because passing instances of a struct around cast to their "interface form"
        // would lead to a lot of boxing and unboxing.
        public readonly IProduct Product { get; }
        public readonly ushort Quantity { get; }
        private static ulong nextComponentId = 0;
        private readonly ushort UniquenessStamp { get; }

        public OrderComponent(IProduct orderedProduct, ushort quantity, ulong orderNumber) {
            Product = orderedProduct;
            Quantity = quantity;
            int hashCode = 17;
            hashCode = (hashCode * 397) ^ nextComponentId.GetHashCode();
            hashCode = (hashCode * 397) ^ orderNumber.GetHashCode();
            hashCode = (hashCode * 397) ^ System.DateTime.UtcNow.GetHashCode();
            UniquenessStamp = (ushort)hashCode;
            nextComponentId++;
        }
        public bool Equals(OrderComponent oc) {
            return this.GetHashCode() == oc.GetHashCode() && this.UniquenessStamp == oc.UniquenessStamp && this.Product.Equals(oc.Product) && this.Quantity == oc.Quantity;
        }
        public override int GetHashCode() {
            unchecked {
                int hashCode = 486187739;
                hashCode = (hashCode * 16777619) ^ UniquenessStamp.GetHashCode();
                hashCode = (hashCode * 16777619) ^ Product.GetHashCode();
                hashCode = (hashCode * 16777619) ^ Quantity.GetHashCode();

                return hashCode;
            }
        }

    }

    public class Order : IOrder {
        private static ulong lastOrderNumber = ulong.MaxValue;
        public ulong OrderNumber { get; } = ++lastOrderNumber;
        private readonly IObservable<OrderComponent> orderComponents = ;
        public Order() {

        }

        double IOrder.TotalPrice(decimal vatRate) {
            throw new NotImplementedException();
        }
    }
}
