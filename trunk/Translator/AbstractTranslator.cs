using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Miata.Library.PropertyMap;
using System.Reflection;
using System.Data.Linq.Mapping;
using System.Data.Common;
using System.Data;
using Miata.Library.Factory;
using Miata.Library.Utils;

namespace Miata.Library.Translator
{
	public abstract class AbstractTranslator<T> : ITranslator<T>
	{
		// Logging instance
		private static readonly ILog log = LogManager.GetLogger(typeof(AbstractTranslator<T>));

		/// <summary>
		/// A type reference of the generic type passed in
		/// </summary>
		private Type GenericType = typeof(T);

		/// <summary>
		/// List of IColumnPropertyMaps representing each property of the generic type with a ColumnAttribute
		/// </summary>
		public List<IColumnPropertyMap> ColumnPropertyMapList { get; set; }

		/// <summary>
		/// Loop through an IDataReader and returns an IEnumerable of results
		/// </summary>
		/// <param name="reader">DataReader to process</param>
		/// <returns>IEnumerable of Objects representing each row</returns>
		public virtual IEnumerable<T> ParseReader(IDataReader reader)
		{
			IList<T> results = new List<T>();
			this.SetColumnNumbers(reader);
			while (reader.Read())
			{
				try
				{
					results.Add(this.ParseRow(reader));
				}
				catch (Exception e)
				{
					log.Warn(e.Message, e);
					throw e;
				}
			}
			return results;
		}

		/// <summary>
		/// Processes one IDataRecord in to a POCO or the specified type
		/// </summary>
		/// <param name="record">IDataRecord to process</param>
		/// <returns>A POCO of the proper type</returns>
		public virtual T ParseRow(IDataRecord record)
		{
			T newObject = ObjectFactory<T>.CreateObject();
			foreach (ColumnPropertyMap item in this.ColumnPropertyMapList.Where(cmp => cmp.SQLExists))
			{
				int columnOrdinal = item.ColumnOrdinal;
				Type objectPropertyType = item.ObjectPropertyType;
				GenericSetter setMethod = item.SetMethod;

				bool isDBNull = record.IsDBNull(columnOrdinal);
				bool isEnum = objectPropertyType.IsEnum;
				bool isNullable = (objectPropertyType.IsGenericType && objectPropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
				
				if (!isDBNull && !isEnum && !isNullable)
				{
					try
					{
						if (log.IsDebugEnabled)
						{
							log.DebugFormat("Converting column {0}({1}) to {2}({3})", columnOrdinal, record.GetValue(columnOrdinal), item.ColumnName, objectPropertyType.Name);
						}
						var columnValue = Convert.ChangeType(record.GetValue(columnOrdinal), objectPropertyType);
						setMethod(newObject, columnValue);
					}
					catch (InvalidCastException e)
					{
						log.WarnFormat("Error converting column {0}({1}) to {2}({3})", columnOrdinal, record.GetValue(columnOrdinal), item.ColumnName, objectPropertyType.Name);
						log.Warn(e.Message, e);
					}
					catch (Exception e)
					{
						log.WarnFormat("Error converting column {0}({1}) to {2}({3})", columnOrdinal, record.GetValue(columnOrdinal), item.ColumnName, objectPropertyType.Name);
						log.Warn(e.Message, e);
						//throw;
					}
				}
				else if (!isDBNull && !isEnum && isNullable)
				{
					var columnValue = Convert.ChangeType(record.GetValue(columnOrdinal), Nullable.GetUnderlyingType(objectPropertyType));
					setMethod(newObject, columnValue);
				}
				else if (!isDBNull && isEnum)
				{
					try
					{
						MethodInfo mapValueMethod = this.GetType().GetMethod("ParseEnum");
						MethodInfo genericMethod = mapValueMethod.MakeGenericMethod(objectPropertyType);
						setMethod(newObject, genericMethod.Invoke(this, new Object[] { record.GetValue(columnOrdinal).ToString() }));
					}
					catch (Exception e)
					{
						log.Error(e.Message, e);
					}
				}
				else if (isDBNull && isNullable)
				{
					setMethod(newObject, null);
				}
			}
			return newObject;
		}

		public virtual TEnum ParseEnum<TEnum>(String dbEnumValue) where TEnum : struct, IConvertible
		{
			Type type = typeof(TEnum);
			if (!type.IsEnum)
			{
				throw new ArgumentException("ParseEnum<TEnum> must be of type System.Enum");
			}
			TEnum returnEnum = default(TEnum);
			if (Enum.IsDefined(type, dbEnumValue))
			{
				returnEnum = (TEnum)Enum.Parse(type, dbEnumValue);
			}
			else
			{
				Boolean foundValue = false;
				if (!foundValue)
				{
					try
					{
						returnEnum = EnumUtils.GetValueFromDefaultValue<TEnum>(dbEnumValue);
						log.DebugFormat("Searched Enum Default Value Attribute and found ", returnEnum);
						foundValue = true;
					}
					catch (ArgumentException)
					{
						// Do nothing because it is not really an exception
						log.Debug("Searched Enum Default Value Attribute and found nothing");
					}
					catch (Exception)
					{
						throw;
					}
				}

				if (!foundValue)
				{
					try
					{
						returnEnum = EnumUtils.GetValueFromDescription<TEnum>(dbEnumValue);
						log.DebugFormat("Searched Enum Description Attribute and found ", returnEnum);
						foundValue = true;
					}
					catch (ArgumentException)
					{
						// Do nothing because it is not really an exception
						log.Debug("Searched Enum Description Attribute and found nothing");
					}
					catch (Exception)
					{
						throw;
					}
				}
			}

			return returnEnum;
		}

		public virtual void SetColumnNumbers(IDataReader dbReader)
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
						columnProperty.ColumnOrdinal = Int32.Parse(myField["ColumnOrdinal"].ToString());
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

		public virtual void ParseTypeProperties()
		{
			PropertyInfo[] typeProperties = GenericType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			this.ColumnPropertyMapList = new List<IColumnPropertyMap>(typeProperties.Count());

			foreach (PropertyInfo item in typeProperties)
			{
				if (item.IsDefined(typeof(ColumnAttribute), true))
				{
					ColumnPropertyMapList.Add(new ColumnPropertyMap(item));
				}
			}
		}
	}
}
