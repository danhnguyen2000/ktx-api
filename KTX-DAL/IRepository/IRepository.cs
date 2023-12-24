using KTX_DAL.Models.Entity;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL.IRepository
{
    public interface IRepository<T> where T : BaseEntity
    {
        List<T> GetAll(QueryFlags flags = QueryFlags.None, int page = 0, int row = 0);
        /// <summary>
        /// Get Item by id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        T GetById(int id, QueryFlags flags = QueryFlags.None);
        /// <summary>
        /// Get Item by list id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="flags"></param>
        /// <param name="page">page</param>
        /// <param name="row">row in page</param>
        /// <returns></returns>
        List<T> GetByListId(List<int> id, QueryFlags flags = QueryFlags.None, int page = 0, int row = 0);
        /// <summary>
        /// Get first item by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        T GetByKey(string key, object value, QueryFlags flags = QueryFlags.None);

        /// <summary>
        /// Get first item by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> GetListByKey(string key, object value, QueryFlags flags = QueryFlags.None);

        /// <summary>
        /// Get first item by key
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        List<T> GetListByListKey(string key, List<object> values, QueryFlags flags = QueryFlags.None);

        /// <summary>
        /// Insert new item
        /// </summary>
        /// <typeparam name="T2"></typeparam>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<int> Upsert<T2>(List<T2> entity) where T2 : class;
        Task<int> Insert(T entity);
        Task<int> Insert<T2>(T2 data, IDbTransaction transaction = null) where T2 : class;
        Task<int> Insert(List<T> entity);
        Task<int> Insert<T2>(List<T2> data, IDbTransaction transaction = null) where T2 : class;

        /// <summary>
        /// Update item
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="T2"></typeparam>
        /// <param name="id"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        Task<int> Update<T2>(int id, T2 data) where T2 : class;
        Task<int> Update<T2>(List<int> listIds, T2 data) where T2 : class;
        /// <summary>
        /// disable item
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<int> Disable(int id);
        Task<int> Disable(List<int> id);
        Task<int> Disable(int id, int disabledBy);
    }
}
