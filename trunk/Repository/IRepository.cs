using System;
using System.Data;
using System.Collections.Generic;
namespace Miata.Library.Repository
{
	public interface IRepository<T>
	{
		T Add(T item);
		void Delete(int id);
		IEnumerable<T> Get();
		T Get(int id);
		bool Update(T item);

		IDbConnection GetConnection();
	}
}
