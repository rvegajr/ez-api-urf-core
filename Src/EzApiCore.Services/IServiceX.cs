
using TrackableEntities.Common.Core;
using URF.Core.Abstractions.Services;

namespace EzApiCore.Service
{
  public interface IServiceX<TEntity> : IService<TEntity>, IRepositoryX<TEntity> where TEntity : class, ITrackable
  {

  }
}