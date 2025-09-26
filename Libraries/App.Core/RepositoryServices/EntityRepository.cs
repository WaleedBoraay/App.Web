
using App.Core.Configuration;
using App.Core.PagedListServices;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace App.Core.RepositoryServices
{
    /// <summary>
    /// Represents the entity repository implementation
    /// </summary>
    /// <typeparam name="TEntity">Entity type</typeparam>
    public partial class EntityRepository<TEntity> : IRepository<TEntity> where TEntity : BaseEntity
    {
        #region Fields

        protected readonly IAppDataProvider _dataProvider;
        protected readonly bool _usingDistributedCache;

        #endregion

        #region Ctor

        public EntityRepository(IAppDataProvider dataProvider)
        {
            _dataProvider = dataProvider;
        }

        #endregion

        #region Utilities

        /// <summary>
        /// Get all entity entries
        /// </summary>
        /// <param name="getAllAsync">Function to select entries</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entries
        /// </returns>
        protected virtual async Task<IList<TEntity>> GetEntitiesAsync(Func<Task<IList<TEntity>>> getAllAsync)
        {
            return await getAllAsync();
        }

        /// <summary>
        /// Get all entity entries
        /// </summary>
        /// <param name="getAll">Function to select entries</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <returns>Entity entries</returns>
        protected virtual IList<TEntity> GetEntities(Func<IList<TEntity>> getAll)
        {
            return getAll();
        }

        /// <summary>
        /// Adds "deleted" filter to query which contains <see cref="ISoftDeletedEntity"/> entries, if its need
        /// </summary>
        /// <param name="query">Entity entries</param>
        /// <param name="includeDeleted">Whether to include deleted items</param>
        /// <returns>Entity entries</returns>
        protected virtual IQueryable<TEntity> AddDeletedFilter(IQueryable<TEntity> query)
        {
            //return query without soft delete filter

            return query;

        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the entity entry
        /// </summary>
        /// <param name="id">Entity entry identifier</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="App.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <param name="useShortTermCache">Whether to use short term cache instead of static cache</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entry
        /// </returns>
        public virtual async Task<TEntity> GetByIdAsync(int? id, bool includeDeleted = true, bool useShortTermCache = false)
        {
            if (!id.HasValue || id == 0)
                return null;

            async Task<TEntity> getEntityAsync()
            {
                return await AddDeletedFilter(Table).FirstOrDefaultAsync(entity => entity.Id == Convert.ToInt32(id));
            }
            return await getEntityAsync();
        }

        /// <summary>
        /// Get the entity entry
        /// </summary>
        /// <param name="id">Entity entry identifier</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// The entity entry
        /// </returns>
        public virtual TEntity GetById(int? id, bool includeDeleted = true)
        {
            if (!id.HasValue || id == 0)
                return null;

            TEntity getEntity()
            {
                return AddDeletedFilter(Table).FirstOrDefault(entity => entity.Id == Convert.ToInt32(id));
            }
            return getEntity();
        }

        /// <summary>
        /// Get entity entries by identifiers
        /// </summary>
        /// <param name="ids">Entity entry identifiers</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entries
        /// </returns>
        public virtual async Task<IList<TEntity>> GetByIdsAsync(IList<int> ids, bool includeDeleted = true)
        {
            if (ids?.Any() != true)
                return new List<TEntity>();

            static IList<TEntity> sortByIdList(IList<int> listOfId, IDictionary<int, TEntity> entitiesById)
            {
                var sortedEntities = new List<TEntity>(listOfId.Count);

                foreach (var id in listOfId)
                    if (entitiesById.TryGetValue(id, out var entry))
                        sortedEntities.Add(entry);

                return sortedEntities;
            }

            async Task<IList<TEntity>> getByIdsAsync(IList<int> listOfId, bool sort = true)
            {
                var query = AddDeletedFilter(Table)
                    .Where(entry => listOfId.Contains(entry.Id));

                return sort
                    ? sortByIdList(listOfId, await query.ToDictionaryAsync(entry => entry.Id))
                    : await query.ToListAsync();
            }
            return await getByIdsAsync(ids);

        }

        /// <summary>
        /// Get all entity entries
        /// </summary>
        /// <param name="func">Function to select entries</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="App.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entries
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAllAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null,
            bool includeDeleted = true)
        {
            async Task<IList<TEntity>> getAllAsync()
            {
                var query = AddDeletedFilter(Table);
                query = func != null ? func(query) : query;

                return await query.ToListAsync();
            }
            return await getAllAsync();
        }

        /// <summary>
        /// Get all entity entries
        /// </summary>
        /// <param name="func">Function to select entries</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="App.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>Entity entries</returns>
        public virtual IList<TEntity> GetAll(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null,
             bool includeDeleted = true)
        {
            IList<TEntity> getAll()
            {
                var query = AddDeletedFilter(Table);
                query = func != null ? func(query) : query;

                return query.ToList();
            }

            return GetEntities(getAll);
        }

        /// <summary>
        /// Get all entity entries
        /// </summary>
        /// <param name="func">Function to select entries</param>
        /// <param name="getCacheKey">Function to get a cache key; pass null to don't cache; return null from this function to use the default key</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="App.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the entity entries
        /// </returns>
        public virtual async Task<IList<TEntity>> GetAllAsync(
            Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> func = null,
             bool includeDeleted = true)
        {
            async Task<IList<TEntity>> getAllAsync()
            {
                var query = AddDeletedFilter(Table);
                query = func != null ? await func(query) : query;

                return await query.ToListAsync();
            }

            return await GetEntitiesAsync(getAllAsync);
        }

        /// <summary>
        /// Get paged list of all entity entries
        /// </summary>
        /// <param name="func">Function to select entries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">Whether to get only the total number of entries without actually loading data</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="App.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the paged list of entity entries
        /// </returns>
        public virtual async Task<IPagedList<TEntity>> GetAllPagedAsync(Func<IQueryable<TEntity>, IQueryable<TEntity>> func = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = true)
        {
            var query = AddDeletedFilter(Table);

            query = func != null ? func(query) : query;

            return (IPagedList<TEntity>)await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Get paged list of all entity entries
        /// </summary>
        /// <param name="func">Function to select entries</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <param name="getOnlyTotalCount">Whether to get only the total number of entries without actually loading data</param>
        /// <param name="includeDeleted">Whether to include deleted items (applies only to <see cref="App.Core.Domain.Common.ISoftDeletedEntity"/> entities)</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the paged list of entity entries
        /// </returns>
        public virtual async Task<IPagedList<TEntity>> GetAllPagedAsync(Func<IQueryable<TEntity>, Task<IQueryable<TEntity>>> func = null,
            int pageIndex = 0, int pageSize = int.MaxValue, bool getOnlyTotalCount = false, bool includeDeleted = true)
        {
            var query = AddDeletedFilter(Table);

            query = func != null ? await func(query) : query;

            return (IPagedList<TEntity>)await query.ToPagedListAsync(pageIndex, pageSize, getOnlyTotalCount);
        }

        /// <summary>
        /// Insert the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await _dataProvider.InsertEntityAsync(entity);


        }

        /// <summary>
        /// Insert the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        public virtual void Insert(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _dataProvider.InsertEntity(entity);
        }

        /// <summary>
        /// Insert entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task InsertAsync(IList<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            await _dataProvider.BulkInsertEntitiesAsync(entities);
            transaction.Complete();

        }

        /// <summary>
        /// Insert entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        public virtual void Insert(IList<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            _dataProvider.BulkInsertEntities(entities);
            transaction.Complete();
        }

        /// <summary>
        /// Loads the original copy of the entity
        /// </summary>
        /// <param name="entity">Entity</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the copy of the passed entity
        /// </returns>
        public virtual async Task<TEntity> LoadOriginalCopyAsync(TEntity entity)
        {
            return await _dataProvider.GetTable<TEntity>()
                .FirstOrDefaultAsync(e => e.Id == Convert.ToInt32(entity.Id));
        }

        /// <summary>
        /// Update the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            await _dataProvider.UpdateEntityAsync(entity);
        }

        /// <summary>
        /// Update the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        public virtual void Update(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            _dataProvider.UpdateEntity(entity);
        }

        /// <summary>
        /// Update entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task UpdateAsync(IList<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            if (!entities.Any())
                return;

            await _dataProvider.UpdateEntitiesAsync(entities);

        }

        /// <summary>
        /// Update entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        public virtual void Update(IList<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            if (!entities.Any())
                return;

            _dataProvider.UpdateEntities(entities);

        }

        /// <summary>
        /// Delete the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteAsync(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            switch (entity)
            {

                default:
                    await _dataProvider.DeleteEntityAsync(entity);
                    break;
            }

        }

        /// <summary>
        /// Delete the entity entry
        /// </summary>
        /// <param name="entity">Entity entry</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        public virtual void Delete(TEntity entity)
        {
            ArgumentNullException.ThrowIfNull(entity);

            switch (entity)
            {

                default:
                    _dataProvider.DeleteEntity(entity);
                    break;
            }
        }

        /// <summary>
        /// Delete entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task DeleteAsync(IList<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            if (!entities.Any())
                return;

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            await _dataProvider.BulkDeleteEntitiesAsync(entities);
            await _dataProvider.UpdateEntitiesAsync(entities);


            transaction.Complete();

        }

        /// <summary>
        /// Delete entity entries
        /// </summary>
        /// <param name="entities">Entity entries</param>
        /// <param name="publishEvent">Whether to publish event notification</param>
        public virtual void Delete(IList<TEntity> entities)
        {
            ArgumentNullException.ThrowIfNull(entities);

            if (!entities.Any())
                return;

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);

            _dataProvider.BulkDeleteEntities(entities);


            transaction.Complete();

        }

        /// <summary>
        /// Delete entity entries by the passed predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the number of deleted records
        /// </returns>
        public virtual async Task<int> DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var countDeletedRecords = await _dataProvider.BulkDeleteEntitiesAsync(predicate);
            transaction.Complete();

            return countDeletedRecords;
        }

        /// <summary>
        /// Delete entity entries by the passed predicate
        /// </summary>
        /// <param name="predicate">A function to test each element for a condition</param>
        /// <returns>
        /// The number of deleted records
        /// </returns>
        public virtual int Delete(Expression<Func<TEntity, bool>> predicate)
        {
            ArgumentNullException.ThrowIfNull(predicate);

            using var transaction = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
            var countDeletedRecords = _dataProvider.BulkDeleteEntities(predicate);
            transaction.Complete();

            return countDeletedRecords;
        }

        /// <summary>
        /// Truncates database table
        /// </summary>
        /// <param name="resetIdentity">Performs reset identity column</param>
        /// <returns>A task that represents the asynchronous operation</returns>
        public virtual async Task TruncateAsync(bool resetIdentity = false)
        {
            await _dataProvider.TruncateAsync<TEntity>(resetIdentity);
        }

        public Task<TEntity> GetByIdAsync(int? id)
        {
            return GetByIdAsync(id, true);
        }

        #endregion

        #region Properties

        /// <summary>
        /// Gets a table
        /// </summary>
        public virtual IQueryable<TEntity> Table => _dataProvider.GetTable<TEntity>();

        #endregion
    }
}
