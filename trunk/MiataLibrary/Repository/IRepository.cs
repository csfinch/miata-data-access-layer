using System;
using System.Data;
using System.Collections.Generic;
namespace Miata.Library.Repository
{
	public interface IRepository<T>
	{
		IDbConnection GetConnection();
	}
}
