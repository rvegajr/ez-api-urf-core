using EzApiCore.Data.Models;
using Microsoft.AspNet.OData.Builder;
using Microsoft.OData.Edm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EzApiCore.Api.Infrastructure
{
    internal static class EdmModel
    {

        public static IEdmModel GetEdmModel()
        {
            var odataBuilder = new ODataConventionModelBuilder();
            var categoriesEntitySetConfiguration = odataBuilder.EntitySet<Categories>(nameof(Categories));
            categoriesEntitySetConfiguration.EntityType.HasKey(x => x.CategoryId);

            var customersEntitySetConfiguration = odataBuilder.EntitySet<Customers>(nameof(Customers));
            customersEntitySetConfiguration.EntityType.HasKey(x => x.CustomerId);
            customersEntitySetConfiguration.EntityType.Ignore(x => x.CustomerCustomerDemo);
            customersEntitySetConfiguration.EntityType.Ignore(x => x.Orders);

            var productsEntitySetConfiguration = odataBuilder.EntitySet<Products>(nameof(Products));
            productsEntitySetConfiguration.EntityType.HasKey(x => x.ProductId);
            productsEntitySetConfiguration.EntityType.Ignore(x => x.Category);
            productsEntitySetConfiguration.EntityType.Ignore(x => x.Supplier);

            var orderDetailsEntitySetConfiguration = odataBuilder.EntitySet<OrderDetails>(nameof(OrderDetails));
            orderDetailsEntitySetConfiguration.EntityType.HasKey(x => x.OrderId);
            orderDetailsEntitySetConfiguration.EntityType.HasKey(x => x.ProductId);
            orderDetailsEntitySetConfiguration.EntityType.Ignore(x => x.Order);

            return odataBuilder.GetEdmModel();
        }
    }
}
