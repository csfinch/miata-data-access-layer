using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.ObjectModel;
using Miata.Library.Factory;
using log4net;
using Oracle.DataAccess.Client;
using System.Reflection;

namespace Miata.Library.Repository
{
	public abstract class AbstractRepository<T> : IRepository<T>, IDisposable
	{
		// Logging instance
		private static readonly ILog log = LogManager.GetLogger(typeof(AbstractRepository<T>));

		private static Type OracleCommandType;

		static AbstractRepository() {
			OracleCommandType = Type.GetType("Oracle.DataAccess.Client.OracleCommand", false);
		}

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

		public abstract IEnumerable<T> Get();

		public abstract T Get(int id);

		public abstract T Add(T item);

		public abstract bool Update(T item);

		public abstract void Delete(int id);

		public abstract IDbConnection GetConnection();

		protected virtual TCom CreateCommand<TCom>(String query, IEnumerable<IDataParameter> parameters) where TCom : IDbCommand
		{
			return this.CreateCommand<TCom>(this.DbConnection, query, parameters);
		}

		protected virtual TCom CreateCommand<TCom>(IDbConnection connection, String query, IEnumerable<IDataParameter> parameters) where TCom : IDbCommand
		{
			TCom command = ObjectFactory<TCom>.CreateObject();
			command.Connection = connection;
			command.CommandText = query;
			command.CommandType = CommandType.Text;

			if (null != OracleCommandType && command.GetType().Equals(OracleCommandType))
			{
				PropertyInfo bindByNameProperty = command.GetType().GetProperty("BindByName");
				MethodInfo bindByNameMethod = bindByNameProperty.GetSetMethod();
				bindByNameMethod.Invoke(command, new Object[] { true });
			}
			
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
