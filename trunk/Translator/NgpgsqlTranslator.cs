﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Miata.Library.Translator;
using log4net;
using System.Data.Common;
using System.Data;
using Miata.Library.PropertyMap;

namespace MiataDataAccessLayer.Translator
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
			DataTable schemaTable = dbReader.GetSchemaTable();

			//For each field in the table...
			foreach (DataRow myField in schemaTable.Rows)
			{
				String columnName = myField["ColumnName"].ToString().ToUpper();
				log.DebugFormat("Found column: {0}", columnName);
				try
				{
					IColumnPropertyMap columnProperty = this.ColumnPropertyMapList.Where(cmp => cmp.ColumnName.Equals(columnName)).SingleOrDefault();
					if (null != columnProperty)
					{
						//Console.WriteLine(columnName + " => " + myField["DataType"].ToString());
						columnProperty.ColumnPropertyType = (Type)myField["DataType"];
						// Thanks to a bug in Ngpgsql...
						columnProperty.ColumnOrdinal = Int32.Parse(myField["ColumnOrdinal"].ToString()) -1;
						columnProperty.SQLExists = true;

						log.DebugFormat("Column: {0} ColumnOrdinal: {1} DataType: {2}", columnName, columnProperty.ColumnOrdinal, columnProperty.ColumnPropertyType.Name);
					}
				}
				catch (IndexOutOfRangeException)
				{
					log.WarnFormat("There is no column with name {0}", columnName);
				}
				catch (ArgumentNullException)
				{
					log.WarnFormat("There are no columns mapped in {0}", GenericType.Name);
				}
				catch (InvalidOperationException)
				{
					log.WarnFormat("The column '{0}' is mapped more than once in {1}", columnName, GenericType.Name);
				}
				catch (Exception)
				{
					throw;
				}
			}
		}


	}
}
