﻿using ApplicationCore.Interfaces;
using Microsoft.eShopWeb.ApplicationCore.Entities;
using Microsoft.OData.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infrastructure.OData
{
    public class ODataClientRepository<TEntity> : IRepository<TEntity>, IAsyncRepository<TEntity>
        where TEntity : BaseEntity
    {
        protected string entitySetName;
        protected Container container;
        
        public ODataClientRepository(Uri serviceRoot)
        {
            this.entitySetName = GetEntitySetName();
            this.container = new Container(serviceRoot);
        }

        public TEntity Add(TEntity entity)
        {
            return AddAsync(entity).Result;
        }

        public virtual async Task<TEntity> AddAsync(TEntity entity)
        {
            container.AddObject(entitySetName, entity);
            var response = await container.SaveChangesAsync();
            return entity;
        }

        public void Delete(TEntity entity)
        {
            DeleteAsync(entity).Wait();
        }

        public Task DeleteAsync(TEntity entity)
        {
            container.DeleteObject(entity);
            return container.SaveChangesAsync();
        }

        public TEntity GetById(int id)
        {
            return GetByIdAsync(id).Result;
        }

        public Task<TEntity> GetByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public TEntity GetSingleBySpec(ISpecification<TEntity> spec)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<TEntity> List(ISpecification<TEntity> spec)
        {
            return ListAsync(spec).Result;
        }

        public IEnumerable<TEntity> ListAll()
        {
            return ListAllAsync().Result;
        }

        public async Task<List<TEntity>> ListAllAsync()
        {
            var result = await QueryAllAsync();
            return result.ToList();
        }

        public async Task<List<TEntity>> ListAsync(ISpecification<TEntity> spec)
        {
            var query = CreateQuery(spec);
            var result = await query.ExecuteAsync();
            return result.ToList();
        }

        public void Update(TEntity entity)
        {
            UpdateAsync(entity).Wait();
        }

        public virtual Task UpdateAsync(TEntity entity)
        {
            container.UpdateObject(entity);
            return container.SaveChangesAsync();
        }


        protected DataServiceQuery<TEntity> CreateQuery(ISpecification<TEntity> spec = null)
        {
            var query = container.CreateQuery<TEntity>(entitySetName);
            if (spec != null)
            {
                query = spec.Includes.Aggregate(
                    query,
                    (current, include) => current.Expand(include));
                query = spec.IncludeStrings.Aggregate(
                    query,
                    (current, include) => current.Expand(include));
                query = query.Where2(spec.Criteria);
            }
            return query;
        }

        private Task<IEnumerable<TEntity>> QueryAllAsync()
        {
            var query = container.CreateQuery<TEntity>(entitySetName);
            return query.ExecuteAsync();
        }
        
        virtual protected string GetEntitySetName()
        {
            return typeof(TEntity).Name + "s";
        }
    }
}
