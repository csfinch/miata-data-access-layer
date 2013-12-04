using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Miata.Library.Translator;
using System.Data.Common;
using System.Data;
using Miata.Library.PropertyMap;
using Common.Logging;

namespace MiataLibrary.Npgsql.Translator
{
    public class NgpgsqlTranslator<T> : AbstractTranslator<T>
    {
        // Logging instance
        private static readonly ILog log = LogManager.GetLogger(typeof(NgpgsqlTranslator<T>));

        /// <summary>
        /// A type reference of the generic type passed in
        /// </summary>
        private Type GenericType = typeof(T);

        public NgpgsqlTranslator()
        {
            base.ParseTypeProperties();
        }

        public NgpgsqlTranslator(DbDataReader dbReader)
        {
            base.ParseTypeProperties();
            base.SetColumnNumbers(dbReader);
        }

        public override void SetColumnNumbers(IDataReader dbReader)
        {
            var schemaTable = dbReader.GetSchemaTable();

            //For each field in the table...
            /*
             * Due to a ColumnOrdinal bug in the DataTable creation logic for NgpgsqlDataTable.GetSchemaTable(), we need to determine if there is an offset.
             * Currently Ngpgsql has an offset of + 1.
             */
            int ordinalOffset = 0;
            try
            {
                var rows = schemaTable.Rows.Cast<DataRow>();
                ordinalOffset = rows.Min(field => Int32.Parse(field["ColumnOrdinal"].ToString()));
            }
            catch (Exception ex)
            {
                log.Debug(m => m(ex.Message), ex);
            }
            foreach (DataRow myField in schemaTable.Rows)
            {
                var columnName = myField["ColumnName"].ToString().ToUpper();
                log.Debug(m => m("Found column: {0}", columnName));
                try
                {
                    var columnProperty = this.ColumnPropertyMapList.SingleOrDefault(cmp => cmp.ColumnName.Equals(columnName));
                    if (null != columnProperty)
                    {
                        //Console.WriteLine(columnName + " => " + myField["DataType"].ToString());
                        columnProperty.ColumnPropertyType = (Type)myField["DataType"];
                        // Thanks to a bug in Ngpgsql...
                        columnProperty.ColumnOrdinal = Int32.Parse(myField["ColumnOrdinal"].ToString()) - ordinalOffset;
                        columnProperty.SQLExists = true;

                        log.Debug(m => m("Column: {0} ColumnOrdinal: {1} DataType: {2}", columnName, columnProperty.ColumnOrdinal, columnProperty.ColumnPropertyType.Name));
                    }
                }
                catch (IndexOutOfRangeException)
                {
                    log.Warn(m => m("There is no column with name {0}", columnName));
                }
                catch (ArgumentNullException)
                {
                    log.Warn(m => m("There are no columns mapped in {0}", GenericType.Name));
                }
                catch (InvalidOperationException)
                {
                    log.Warn(m => m("The column '{0}' is mapped more than once in {1}", columnName, GenericType.Name));
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }
    }
}
