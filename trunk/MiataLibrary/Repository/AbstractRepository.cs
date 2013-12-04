using System;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using Miata.Library.Factory;
using Common.Logging;

namespace Miata.Library.Repository
{
	public abstract class AbstractRepository<T> : IRepository<T>, IDisposable
	{
		// Logging instance
		private static readonly ILog log = LogManager.GetLogger(typeof(AbstractRepository<T>));

		protected IDbConnection DbConnection { get; set; }

		protected virtual string DebugSqlQuery(StringBuilder command, IEnumerable<IDataParameter> parameters)
		{
			return this.DebugSqlQuery(command.ToString(), parameters);
		}

		protected virtual string DebugSqlQuery(String command, IEnumerable<IDataParameter> parameters)
		{
			var debugQueryString = command;
            foreach (var p in parameters.Where(p => p.Direction == ParameterDirection.Input || p.Direction == ParameterDirection.InputOutput))
			{
				var parameterNameMarker = String.Format(":{0}", p.ParameterName);
                var stringValue = string.Empty;
                if (p.DbType == DbType.AnsiString)
				{
                    stringValue = (null == p.Value || DBNull.Value == p.Value) ? "NULL" : String.Format("'{0}'", p.Value.ToString());
				}
				else
				{
                    stringValue = (null == p.Value || DBNull.Value == p.Value) ? "NULL" : p.Value.ToString();
				}
                debugQueryString = debugQueryString.Replace(parameterNameMarker, stringValue);
			}
			return debugQueryString;
		}

		public abstract IDbConnection GetConnection();

		protected virtual TCom CreateCommand<TCom>(String query, IEnumerable<IDataParameter> parameters) where TCom : IDbCommand
		{
			return this.CreateCommand<TCom>(this.DbConnection, query, parameters);
		}

		protected virtual TCom CreateCommand<TCom>(IDbConnection connection, String query, IEnumerable<IDataParameter> parameters) where TCom : IDbCommand
		{
			TCom command = ObjectFactory<TCom>.CreateObject();
			
			log.InfoFormat("Creating Command of Type: {0}", typeof(TCom));
			 
			command.Connection = connection;
			command.CommandText = query;
			command.CommandType = CommandType.Text;
			
			foreach (IDataParameter p in parameters)
			{
				command.Parameters.Add(p);
			}
			return command;
		}

		public void Dispose()
		{
			this.GetConnection().Close();
			this.GetConnection().Dispose();
		}
	}
}
