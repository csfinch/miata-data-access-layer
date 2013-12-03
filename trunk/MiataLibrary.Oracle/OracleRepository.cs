using Common.Logging;
using Miata.Library.Repository;
using Miata.Library.Translator;
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
                string connectionString = ConfigurationManager.ConnectionStrings[dataSourceName].ConnectionString;
                this.DbConnection = new OracleConnection(connectionString);
            }
            return this.DbConnection;
        }

        protected IEnumerable<TRowType> ProcessRefCursor<TRowType>(OracleRefCursor cursor)
        {
            IEnumerable<TRowType> results = null;
            try
            {
                OracleDataReader reader = (OracleDataReader)cursor.GetDataReader();
                try
                {
                    BaseTranslator<TRowType> oTranslator = new BaseTranslator<TRowType>();
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
                BaseTranslator<TRowType> oTranslator = new BaseTranslator<TRowType>();
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
            return results;
        }

        protected dynamic ProcessOutputParameters(OracleCommand cmd)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = expando as IDictionary<String, object>;

            List<OracleParameter> outParameters = new List<OracleParameter>();
            foreach (OracleParameter p in cmd.Parameters)
            {
                if (!p.Direction.Equals(ParameterDirection.Input))
                {
                    outParameters.Add(p);
                }
            }

            foreach (var item in outParameters)
            {
                OracleParameter cmdParameter = cmd.Parameters[item.ParameterName];
                OracleDbType cmdParameterType = cmdParameter.OracleDbType;
                String cmdParameterName = cmdParameter.ParameterName;
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

            List<OracleParameter> outParameters = new List<OracleParameter>();
            foreach (OracleParameter p in cmd.Parameters)
            {
                if (!p.Direction.Equals(ParameterDirection.Input))
                {
                    outParameters.Add(p);
                }
            }

            foreach (var item in outParameters)
            {
                OracleParameter cmdParameter = cmd.Parameters[item.ParameterName];
                OracleDbType cmdParameterType = cmdParameter.OracleDbType;
                String cmdParameterName = cmdParameter.ParameterName;
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
