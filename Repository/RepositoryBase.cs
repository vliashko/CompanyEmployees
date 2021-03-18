using Contracts;
using Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Linq.Expressions;

namespace Repository
{
    public abstract class RepositoryBase<T> : IRepositoryBase<T> where T : class
    {
        protected RepositoryDbContext context;
        public RepositoryBase(RepositoryDbContext context)
        {
            this.context = context;
        }

        public IQueryable<T> FindAll(bool trackChanges) =>
            !trackChanges ? context.Set<T>().AsNoTracking() : 
                context.Set<T>();

        public IQueryable<T> FindByCondition(Expression<Func<T, bool>> expression, bool trackChanges) =>
            !trackChanges ? context.Set<T>().Where(expression).AsNoTracking() : 
                context.Set<T>().Where(expression);

        public void Create(T entity) => context.Set<T>().Add(entity);

        public void Delete(T entity) => context.Set<T>().Remove(entity);

        public void Update(T entity) => context.Set<T>().Update(entity);
    }
}
