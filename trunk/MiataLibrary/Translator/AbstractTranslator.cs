using Common.Logging;
using Miata.Library.Factory;
using Miata.Library.PropertyMap;
using Miata.Library.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Miata.Library.Translator
{
    public abstract class AbstractTranslator<T> : ITranslator<T>
    {
        // Logging instance
        private readonly ILog log = LogManager.GetLogger(typeof(AbstractTranslator<T>));

        /// <summary>
        /// A type reference of the generic type passed in
        /// </summary>
        private Type GenericType = typeof(T);

        /// <summary>
        /// List of IColumnPropertyMaps representing each property of the generic type with a ColumnAttribute
        /// </summary>
        public HashSet<IColumnPropertyMap> ColumnPropertyMapList { get; set; }

        /// <summary>
        /// Loop through an IDataReader and returns an IEnumerable of results
        /// </summary>
        /// <param name="reader">DataReader to process</param>
        /// <returns>IEnumerable of Objects representing each row</returns>
        public virtual IEnumerable<T> ParseReader(IDataReader reader)
        {
            this.SetColumnNumbers(reader);
            while (reader.Read())
            {
                yield return this.ParseRow(reader);
            }
        }

        /// <summary>
        /// Processes one IDataRecord in to a POCO or the specified type
        /// </summary>
        /// <param name="record">IDataRecord to process</param>
        /// <returns>A POCO of the proper type</returns>
        public virtual T ParseRow(IDataRecord record)
        {
            var newObject = ObjectFactory<T>.CreateObject();
            foreach (ColumnPropertyMap item in this.ColumnPropertyMapList.Where(cmp => cmp.SQLExists))
            {
                var columnOrdinal = item.ColumnOrdinal;
                var objectPropertyType = item.ObjectPropertyType;
                var setMethod = item.SetMethod;

                var isDBNull = record.IsDBNull(columnOrdinal);
                var isEnum = objectPropertyType.IsEnum;
                var isNullable = (objectPropertyType.IsGenericType && objectPropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)));
                var isString = objectPropertyType.Equals(typeof(string));

                if (!isDBNull && !isEnum && !isNullable)
                {
                    var propertyString = String.Format("{0}({1}) to {2}({3})", columnOrdinal, record.GetValue(columnOrdinal), item.ColumnName, objectPropertyType.Name);
                    try
                    {
                        log.Debug(m => m("Converting column {0}", propertyString));
                        var columnValue = Convert.ChangeType(record.GetValue(columnOrdinal), objectPropertyType);
                        setMethod(newObject, columnValue);
                    }
                    catch (InvalidCastException e)
                    {
                        log.Warn(m => m("Error converting {0}", propertyString));
                        log.Warn(m => m(e.Message), e);
                    }
                    catch (Exception e)
                    {
                        log.Warn(m => m("Error converting {0}", propertyString));
                        log.Warn(m => m(e.Message), e);
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
                        var mapValueMethod = this.GetType().GetMethod("ParseEnum");
                        var genericMethod = mapValueMethod.MakeGenericMethod(objectPropertyType);
                        setMethod(newObject, genericMethod.Invoke(this, new Object[] { record.GetValue(columnOrdinal).ToString() }));
                    }
                    catch (Exception e)
                    {
                        log.Error(m => m(e.Message), e);
                    }
                }
                else if ((isDBNull && isNullable) || (isDBNull && isString))
                {
                    setMethod(newObject, (isString ? "" : null));
                }
            }
            return newObject;
        }

        public virtual TEnum ParseEnum<TEnum>(String dbEnumValue) where TEnum : struct, IConvertible
        {
            var type = typeof(TEnum);
            if (!type.IsEnum)
            {
                throw new ArgumentException("ParseEnum<TEnum> must be of type System.Enum");
            }

            var returnEnum = default(TEnum);
            if (Enum.IsDefined(type, dbEnumValue))
            {
                returnEnum = (TEnum)Enum.Parse(type, dbEnumValue);
            }
            else
            {
                var foundValue = false;
                if (!foundValue)
                {
                    try
                    {
                        returnEnum = EnumUtils.GetValueFromDefaultValue<TEnum>(dbEnumValue);
                        log.Debug(m => m("Searched Enum Default Value Attribute and found {0}", returnEnum));
                        foundValue = true;
                    }
                    catch (ArgumentException)
                    {
                        // Do nothing because it is not really an exception
                        log.Debug(m => m("Searched Enum Default Value Attribute and found nothing"));
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
                        log.Debug(m => m("Searched Enum Description Attribute and found ", returnEnum));
                        foundValue = true;
                    }
                    catch (ArgumentException)
                    {
                        // Do nothing because it is not really an exception
                        log.Debug(m => m("Searched Enum Description Attribute and found nothing"));
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
            var schemaTable = dbReader.GetSchemaTable();

            //For each field in the table...
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
                        columnProperty.ColumnOrdinal = Int32.Parse(myField["ColumnOrdinal"].ToString());
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

        public virtual void ParseTypeProperties()
        {
            var linqColumnAttributeType = typeof(System.Data.Linq.Mapping.ColumnAttribute);
            var schemaColumnAttributeType = typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute);
            var typeProperties = GenericType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            this.ColumnPropertyMapList = new HashSet<IColumnPropertyMap>();

            Func<PropertyInfo, bool> linqFunc = item => Attribute.IsDefined(item, linqColumnAttributeType, true) || Attribute.IsDefined(item, schemaColumnAttributeType, true);
            foreach (var item in typeProperties.Where(linqFunc))
            {
                ColumnPropertyMapList.Add(new ColumnPropertyMap(item));
            }
        }
    }
}
