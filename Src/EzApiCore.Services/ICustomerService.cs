using System;
using System.Linq.Expressions;
using EzApiCore.Data.Models;
using URF.Core.Abstractions.Services;

namespace EzApiCore.Service
{
  // Example: extending IService<TEntity> and/or ITrackableRepository<TEntity>, scope: ICustomerService
  public interface ICustomerService : IService<Customers>
  {
    // Example: adding synchronous Single method, scope: ICustomerService
    Customers Single(Expression<Func<Customers, bool>> predicate);
  }
}