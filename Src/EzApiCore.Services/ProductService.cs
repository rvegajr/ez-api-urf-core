using EzApiCore.Data.Models;
using URF.Core.Abstractions.Trackable;
using URF.Core.Services;

namespace EzApiCore.Service
{
    public class ProductService : Service<Products>, IProductService
    {
        public ProductService(ITrackableRepository<Products> repository) : base(repository)
        {
        }
    }
}
