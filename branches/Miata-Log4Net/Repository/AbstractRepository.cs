using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using log4net;
using Miata.Library.Factory;

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
			String debugQueryString = command;
			foreach (IDataParameter p in parameters)
			{
				bool isInput = (p.Direction == ParameterDirection.Input);
				bool isInputOutput = (p.Direction == ParameterDirection.InputOutput);
				bool isOutput = (p.Direction == ParameterDirection.Output);

				if ((isInput || isInputOutput) && p.DbType == DbType.AnsiString)
				{
					debugQueryString = debugQueryString.Replace(":" + p.ParameterName, ((null == p.Value || DBNull.Value == p.Value) ? "NULL" : "'" + p.Value.ToString() + "'"));
				}
				else if (isOutput)
				{
					// don't do anything, messy
				}
				else
				{
					debugQueryString = debugQueryString.Replace(":" + p.ParameterName, ((null == p.Value || DBNull.Value == p.Value) ? "NULL" : p.Value.ToString()));
				}

			}
			return debugQueryString;
		}

		//public abstract IEnumerable<T> Get();

		//public abstract T Get(int id);

		//public abstract T Add(T item);

		//public abstract bool Update(T item);

		//public abstract void Delete(int id);

		public abstract IDbConnection GetConnection();

		protected virtual TCom CreateCommand<TCom>(String query, IEnumerable<IDataParameter> parameters) where TCom : IDbCommand
		{
			return this.CreateCommand<TCom>(this.DbConnection, query, parameters);
		}

		protected virtual TCom CreateCommand<TCom>(IDbConnection connection, String query, IEnumerable<IDataParameter> parameters) where TCom : IDbCommand
		{
			TCom command = ObjectFactory<TCom>.CreateObject();
			
			if ("Oracle.DataAccess.Client.OracleCommand".Equals(typeof(TCom).FullName))
			{
				log.Info("Created an OracleCommand, attempting to set BindByName to True.");
				try
				{
					if (null != command.GetType().GetProperty("BindByName"))
					{
						command.GetType().GetProperty("BindByName").SetValue(command, true, null);
					}
				}
				catch (Exception ex)
				{
					log.Warn("Failed to locate BindByName for Oracle.DataAccess.Client.OracleCommand");
					log.Debug(ex.Message, ex);
				}
			}
			else
			{
				log.InfoFormat("Found: {0}", typeof(TCom));
			}
			 
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
