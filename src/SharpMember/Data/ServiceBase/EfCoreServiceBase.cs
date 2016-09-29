﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Web;

namespace SharpMember.Data.ServiceBase
{
    /// <summary>
    /// Make up the missing methods for EntityFramework Core DbSet object.
    /// </summary>
    static public class EfCoreExt
    {
        /// <summary>
        /// from: http://stackoverflow.com/questions/29030472/dbset-doesnt-have-a-find-method-in-ef7
        /// This method has not been tested yet, so it's not used in production, just leave here as reference.
        /// </summary>
        public static TEntity Find<TEntity>(this DbSet<TEntity> set, params object[] keyValues) where TEntity : class
        {
            var context = ((IInfrastructure<IServiceProvider>)set).GetService<DbContext>();

            var entityType = context.Model.FindEntityType(typeof(TEntity));
            var key = entityType.FindPrimaryKey();

            var entries = context.ChangeTracker.Entries<TEntity>();

            var i = 0;
            foreach (var property in key.Properties)
            {
                entries = entries.Where(e => e.Property(property.Name).CurrentValue == keyValues[i]);
                i++;
            }

            var entry = entries.FirstOrDefault();
            if (entry != null)
            {
                // Return the local object if it exists.
                return entry.Entity;
            }

            // TODO: Build the real LINQ Expression
            // set.Where(x => x.Id == keyValues[0]);
            var parameter = Expression.Parameter(typeof(TEntity), "x");
            var query = set.Where((Expression<Func<TEntity, bool>>)
                Expression.Lambda(
                    Expression.Equal(
                        Expression.Property(parameter, "Id"),
                        Expression.Constant(keyValues[0])),
                    parameter));

            // Look in the database
            return query.FirstOrDefault();
        }

        /// <summary>
        /// from: http://stackoverflow.com/questions/33819159/is-there-a-dbsettentity-local-equivalent-in-entity-framework-7
        /// </summary>
        public static ObservableCollection<TEntity> GetLocal<TEntity>(this DbSet<TEntity> set) where TEntity : class
        {
            var context = set.GetService<DbContext>();
            var data = context.ChangeTracker.Entries<TEntity>().Select(e => e.Entity);
            var collection = new ObservableCollection<TEntity>(data);

            collection.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    context.AddRange(e.NewItems.Cast<TEntity>());
                }

                if (e.OldItems != null)
                {
                    context.RemoveRange(e.OldItems.Cast<TEntity>());
                }
            };

            return collection;
        }
    }

    /// <summary>
    /// from: http://blog.cincura.net/233451-using-entity-frameworks-find-method-with-predicate/
    /// </summary>
    static public class EfExt
    {
        public static IQueryable<T> FindPredicateFromLocalAndDb<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate) where T : class
        {
            var local = dbSet.GetLocal().Where(predicate.Compile()); // query 'Local' to see if data has been loaded
            if (local.Any())
                return local.AsQueryable();
            else
                return dbSet.Where(predicate); // load data from the database
        }

        /// <returns>Not sure how to asynchronously return a IQueryable, so use IEnumerable instead</returns>
        public static async Task<IEnumerable<T>> FindPredicateAsync<T>(this DbSet<T> dbSet, Expression<Func<T, bool>> predicate) where T : class
        {
            var local = dbSet.GetLocal().Where(predicate.Compile());
            if (local.Any())
                return local;
            else
                return await dbSet.Where(predicate).ToListAsync().ConfigureAwait(false);
        }
    }

    public interface IServiceBase<TEntity> where TEntity : class
    {
        TEntity GetById(int id);
        TEntity Add(TEntity entity);
        void Update(TEntity entity);
        void Delete(TEntity entity);
        void Delete(Expression<Func<TEntity, bool>> where);
        IQueryable<TEntity> GetAll();
        IQueryable<TEntity> GetMany(Expression<Func<TEntity, bool>> where);
        IQueryable<TEntity> GetManyLocalFirst(Expression<Func<TEntity, bool>> where);
    }

    /// <summary>
    /// from:
    /// http://www.asp.net/mvc/overview/older-versions/getting-started-with-ef-5-using-mvc-4/implementing-the-repository-and-unit-of-work-patterns-in-an-asp-net-mvc-application
    /// https://github.com/MarlabsInc/webapi-angularjs-spa/blob/28bea19b3267aeed1768920b0d77be329b0278a5/source/ResourceMetadata/ResourceMetadata.Data/Infrastructure/RepositoryBase.cs
    /// </summary>
    abstract public class EfCoreServiceBase<TEntity> : IServiceBase<TEntity> where TEntity : class 
    {
        protected IUnitOfWork<ApplicationDbContext> UnitOfWork { get; set; }
        private readonly DbSet<TEntity> DbSet;

        public EfCoreServiceBase(IUnitOfWork<ApplicationDbContext> uow)
        {
            try
            {
                if(null == uow) throw new Exception("IUnitOfWork reference is null");
                UnitOfWork = uow;
                DbSet = UnitOfWork.Context.Set<TEntity>();
            }
            catch (Exception ex)
            {
                //Logger.WriteWarning(ex.Message);
                Trace.WriteLine(ex.StackTrace);
            }
        }

        /// <summary>
        /// Not sure if the Find() extension method on the top is right or not, so declare this method as an abstract 
        /// and override it in the subclasses then implement it using Single() method
        /// 
        /// The original method in EF 6.x:
        ///     public virtual TEntity GetById(object id)
        ///     {
        ///         return DbSet.Find(id);
        ///     }
        /// </summary>
        public abstract TEntity GetById(int id);

        public virtual TEntity Add(TEntity entity)
        {
            DbSet.Add(entity);
            return entity;
        }

        public virtual void Update(TEntity entity)
        {
            DbSet.Attach(entity);
            UnitOfWork.Context.Entry(entity).State = EntityState.Modified;
        }

        public virtual void Delete(TEntity entity)
        {
            DbSet.Remove(entity);
        }

        public virtual void Delete(Expression<Func<TEntity, bool>> where)
        {
            IEnumerable<TEntity> objects = DbSet.Where<TEntity>(where).AsEnumerable();
            foreach (TEntity obj in objects)
            {
                this.Delete(obj);
            }
        }

        /// <returns>Return IQueryable to use QueryableExtensions methods like Load(), Include() etc. </returns>
        public virtual IQueryable<TEntity> GetAll()
        {
            return DbSet;
        }

        /// <returns>See the return comment of <see cref="GetAll()"/></returns>
        public virtual IQueryable<TEntity> GetMany(Expression<Func<TEntity, bool>> where)
        {
            // Don't use "where.Compile(), otherwise when do "ToList()", such an exception will throw out: 
            // "There is already an open DataReader associated with this Command which must be closed first"
            return DbSet.Where(where);
        }

        /// <summary>
        /// WARNING: be careful to use this method, if not VERY sure, always use "GetMany" to load from database
        ///          rather than using this "GetManyLocalFirst" to return from what's already in memory.
        /// 
        /// The original idea of this method is: get data immediately after adding data, the data should be better
        /// get directly from memory (aka. local)
        /// 
        /// However, if call this method twice with a writing between the 2 calls, then the second call
        /// will only return the dataset from the loaded first call, i.e. the just written new data will not
        /// be included in the return collection.
        /// </summary>
        /// <returns>See the return comment of <see cref="GetAll()"/></returns>
        public virtual IQueryable<TEntity> GetManyLocalFirst(Expression<Func<TEntity, bool>> where)
        {
            return DbSet.FindPredicateFromLocalAndDb(where);
        }
    }
}