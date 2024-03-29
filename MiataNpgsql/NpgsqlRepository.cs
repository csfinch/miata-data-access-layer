﻿using Common.Logging;
using Miata.Library.Repository;
using MiataLibrary.Npgsql.Translator;
using Npgsql;
using NpgsqlTypes;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Dynamic;

/*
 * Install-Package Npgsql
 */
namespace MiataLibrary.Npgsql
{
    public abstract class NpgsqlRepository<T> : AbstractRepository<T>
    {
        // Logging instance
        private static readonly ILog log = LogManager.GetLogger(typeof(NpgsqlRepository<T>));

        private string dataSourceName = ConfigurationManager.AppSettings["db.connection.name"].ToString();

        public override IDbConnection GetConnection()
        {
            if (null == this.DbConnection)
            {
                var connectionString = ConfigurationManager.ConnectionStrings[dataSourceName].ConnectionString;
                this.DbConnection = new NpgsqlConnection(connectionString);
            }
            return this.DbConnection;
        }

        protected IEnumerable<TRowType> ProcessReader<TRowType>(IDataReader reader)
        {
            IEnumerable<TRowType> results = null;
            try
            {
                var oTranslator = new NgpgsqlTranslator<TRowType>();
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

        protected dynamic ProcessOutputParameters(NpgsqlCommand cmd)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = expando as IDictionary<String, object>;

            var outParameters = new HashSet<NpgsqlParameter>();
            foreach (NpgsqlParameter p in cmd.Parameters)
            {
                if (!p.Direction.Equals(ParameterDirection.Input))
                {
                    outParameters.Add(p);
                }
            }

            foreach (var item in outParameters)
            {
                var cmdParameter = cmd.Parameters[item.ParameterName];
                var cmdParameterType = cmdParameter.NpgsqlDbType;
                var cmdParameterName = cmdParameter.ParameterName;
                if (NpgsqlDbType.Refcursor.Equals(cmdParameterType))
                {
                    using (NpgsqlDataReader cursor = (NpgsqlDataReader)cmdParameter.Value)
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

        protected dynamic ProcessOutputParameters<TRowType>(NpgsqlCommand cmd)
        {
            dynamic expando = new ExpandoObject();
            var expandoDict = expando as IDictionary<String, object>;

            var outParameters = new HashSet<NpgsqlParameter>();
            foreach (NpgsqlParameter p in cmd.Parameters)
            {
                if (!p.Direction.Equals(ParameterDirection.Input))
                {
                    outParameters.Add(p);
                }
            }

            foreach (var item in outParameters)
            {
                var cmdParameter = cmd.Parameters[item.ParameterName];
                var cmdParameterType = cmdParameter.NpgsqlDbType;
                var cmdParameterName = cmdParameter.ParameterName;
                if (NpgsqlDbType.Refcursor.Equals(cmdParameterType))
                {
                    using (NpgsqlDataReader cursor = (NpgsqlDataReader)cmdParameter.Value)
                    {
                        expandoDict[cmdParameterName] = this.ProcessReader<TRowType>(cursor);
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
