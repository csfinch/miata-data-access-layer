using Common.Logging;
using Miata.Library.Factory;
using Miata.Library.Repository;
using Miata.Library.Translator;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;

namespace MiataLibrary.Oracle
{
    public abstract class OracleRepository<T> : AbstractRepository<T>
    {
        // Logging instance
        private static readonly ILog log = LogManager.GetLogger(typeof(OracleRepository<T>));

        private string dataSourceName = ConfigurationManager.AppSettings["db.connection.name"].ToString();

        public override IDbConnection GetConnection()
        {
            if (null == this.DbConnection)
            {
                var connectionString = ConfigurationManager.ConnectionStrings[dataSourceName].ConnectionString;
                this.DbConnection = new OracleConnection(connectionString);
            }
            return this.DbConnection;
        }

        protected override OracleCommand CreateCommand<OracleCommand>(IDbConnection connection, String query, IEnumerable<IDataParameter> parameters)
        {
            var commandType = typeof(OracleCommand);
            var command = ObjectFactory<OracleCommand>.CreateObject();

            if (commandType is OracleCommand)
            {
                log.Info("Created an OracleCommand, attempting to set BindByName to True.");
                try
                {
                    var bindByNameProperty = commandType.GetProperty("BindByName");
                    if (null != bindByNameProperty)
					{
						bindByNameProperty.SetValue(command, true, null);
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
                log.InfoFormat("Found: {0}", commandType);
            }

            command.Connection = connection;
            command.CommandText = query;
            command.CommandType = CommandType.Text;

            foreach (var p in parameters)
            {
                command.Parameters.Add(p);
            }
            return command;
        }

        protected IEnumerable<TRowType> ProcessRefCursor<TRowType>(OracleRefCursor cursor)
        {
            IEnumerable<TRowType> results = null;
            try
            {
                var reader = (OracleDataReader)cursor.GetDataReader();
                try
                {
                    var oTranslator = new BaseTranslator<TRowType>();
                    results = oTranslator.ParseReader(reader);
                }
                catch (Exception e)
                {
                    log.Error(e.Message, e);
                    throw e;
                }
                finally
                {
                    if (reader != null)
                    {
                        reader.Close();
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return results;
        }

        protected IEnumerable<TRowType> ProcessReader<TRowType>(IDataReader reader)
        {
            IEnumerable<TRowType> results = null;
            try
            {
                var oTranslator = new BaseTranslator<TRowType>();
                results = oTranslator.ParseReader(reader);
            }
            catch (Exception e)
            {
                log.Error(m => m(e.Message), e);
                throw e;
            }
            finally
            {
                if (reader != null)
                {
                    reader.Close();
                }
            }
            return results;
        }

        protected dynamic ProcessOutputParameters(OracleCommand cmd)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = expando as IDictionary<String, object>;

            var outParameters = new HashSet<OracleParameter>();
            foreach (OracleParameter p in cmd.Parameters)
            {
                if (!p.Direction.Equals(ParameterDirection.Input))
                {
                    outParameters.Add(p);
                }
            }

            foreach (var item in outParameters)
            {
                var cmdParameter = cmd.Parameters[item.ParameterName];
                var cmdParameterType = cmdParameter.OracleDbType;
                var cmdParameterName = cmdParameter.ParameterName;
                if (OracleDbType.RefCursor.Equals(cmdParameterType))
                {
                    using (OracleRefCursor cursor = (OracleRefCursor)cmdParameter.Value)
                    {
                        log.WarnFormat("REF CURSORS are not supported by this method.  Use ProcessOutputParameters<TRowType>(OracleCommand cmd) instead to retrieve the value of {0}", cmdParameterName);
                    }
                }
                else
                {
                    expandoDict[cmdParameterName] = cmdParameter.Value;
                }
            }

            this.DbConnection.Close();

            return expando;
        }

        protected dynamic ProcessOutputParameters<TRowType>(OracleCommand cmd)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = expando as IDictionary<String, object>;

            var outParameters = new HashSet<OracleParameter>();
            foreach (OracleParameter p in cmd.Parameters)
            {
                if (!p.Direction.Equals(ParameterDirection.Input))
                {
                    outParameters.Add(p);
                }
            }

            foreach (var item in outParameters)
            {
                var cmdParameter = cmd.Parameters[item.ParameterName];
                var cmdParameterType = cmdParameter.OracleDbType;
                var cmdParameterName = cmdParameter.ParameterName;
                if (OracleDbType.RefCursor.Equals(cmdParameterType))
                {
                    using (OracleRefCursor cursor = (OracleRefCursor)cmdParameter.Value)
                    {
                        expandoDict[cmdParameterName] = this.ProcessRefCursor<TRowType>(cursor);
                    }
                }
                else
                {
                    expandoDict[cmdParameterName] = cmdParameter.Value;
                }
            }

            return expando;
        }
    }
}
