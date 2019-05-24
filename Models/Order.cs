using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace VendingMachine {
    public interface IOrder {

        ulong OrderNumber { get; }
        decimal TotalPrice { get; }
        decimal TotalPriceWithVAT { get; }
        decimal TotalPriceWithCustomVAT(decimal vatRate);

    }

    public class Order : IOrder {
        private static ulong lastOrderNumber = ulong.MaxValue;
        public ulong OrderNumber { get; } = ++lastOrderNumber;
        private readonly IReadOnlyCollection<OrderComponent> OrderComponents;
        public IEnumerable < (IProduct product, uint quantity) > Items {
            get { return OrderComponents.Select(c => ((c.Product, (uint)c.Quantity))); }
        }
        private Customer Customer { get; }
        public decimal TotalPrice { get; }
        public decimal TotalPriceWithVAT { get; }

        public Order(IReadOnlyCollection < (IProduct product, uint quantity) > orderComponents, Customer customer, decimal vatRate = 0.25M) {
            var components = new List<OrderComponent>(orderComponents.Count);
            foreach (var item in orderComponents) {
                components.Add(new OrderComponent(item.product, (ushort)item.quantity, OrderNumber));
            }
            this.OrderComponents = components.AsReadOnly();
            Customer = customer;
            decimal totalPrice = 0;

            foreach (var component in this.OrderComponents) {
                totalPrice += component.Product.RecommendedRetailPrice * component.Quantity;
            }
            TotalPrice = totalPrice;
            TotalPriceWithVAT = TotalPriceWithCustomVAT(vatRate);
        }

        public decimal TotalPriceWithCustomVAT(decimal vatRate) {
            return TotalPrice * (1 + vatRate);
        }

        private struct OrderComponent : IEquatable<OrderComponent> {
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
    }
}
