using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Reflection;
using log4net;
using Oracle.DataAccess.Client;
using Oracle.DataAccess.Types;
using System.Numerics;
using Miata.Library.Mappers.ValueMappers;
using System.Linq.Expressions;
using System.Reflection.Emit;

/**
* For the brave souls who get this far: You are the chosen ones,
* the valiant knights of programming who toil away, without rest,
* fixing our most awful code. To you, true saviors, kings of men,
* I say this: never gonna give you up, never gonna let you down,
* never gonna run around and desert you. Never gonna make you cry,
* never gonna say goodbye. Never gonna tell a lie and hurt you.
*/

namespace Miata.Library.Mappers
{
	public abstract class MapperBase<T> : IMapperBase
	{
		// Logging instance
		private static readonly ILog log = LogManager.GetLogger(typeof(MapperBase<T>));

		// Generic Setter function delegate
		private delegate void GenericSetter(object target, object value);

		// Generic Getter function delegate
		private delegate object GenericGetter(object target);

		// A list of properties specified by this mapper instance
		protected List<PropertyInfo> mapperProperties = new List<PropertyInfo>();

		// A static cache of "getter" function delegates for this mapper instance
		private static Dictionary<string, GenericGetter> GetDelegateCache = new Dictionary<string, GenericGetter>();

		// A static cache of "setter" function delegates for this mapper instance
		private static Dictionary<string, GenericSetter> SetDelegateCache = new Dictionary<string, GenericSetter>();

		// A static type reference of the generic type passed in
		private static Type GenericType = typeof(T);

		// A static cache of the generic types properties
		private static Dictionary<string, PropertyInfo> GenericTypePropertyCache = new Dictionary<string, PropertyInfo>();

		// A static cache of "setter" function delegates for the generic type of this mapper instance
		private static Dictionary<string, GenericSetter> GenericTypeSetDelegateCache = new Dictionary<string, GenericSetter>();

		// A static cache map of value mapper objects
		protected static Dictionary<Type, Type> ValueMappersDictionary = new Dictionary<Type, Type>();

		// A static cache map of value mapper objects that have already been created
		protected static Dictionary<Type, IValueMapper> ValueMapperInstances = new Dictionary<Type, IValueMapper>();

		protected MapperBase()
		{
			// Get the properties for the type
			PropertyInfo[] typeProperties = GenericType.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

			// Loop through each one and add it to the cache if required
			foreach (PropertyInfo item in typeProperties)
			{
				// Search the GenericTypePropertyCache for the property Name of the generic type parameter
				if (!GenericTypePropertyCache.ContainsKey(item.Name))
				{
					// If it does not exist, lock the cache and add the property
					lock (GenericTypePropertyCache)
					{
						// Add the property to the cache with the name as the key
						GenericTypePropertyCache.Add(item.Name, item);
					}
				}

				// Search the GenericTypeSetDelegateCache for the property Name of the generic type parameter
				if (!GenericTypeSetDelegateCache.ContainsKey(item.Name))
				{
					// If it does not exist, lock the cache and add the property
					lock (GenericTypeSetDelegateCache)
					{
						// Add the property to the cache with the name as the key
						// and create a set method delegate "setter"
						GenericTypeSetDelegateCache.Add(item.Name, CreateSetMethod(item));
					}
				}
			}

			// Get the properties of the mapper
			this.mapperProperties = new List<PropertyInfo>(this.GetType().GetProperties());

			// Loop through each property with a "ColumnAttribute" and set it's column position value to -1
			foreach (PropertyInfo prop in this.mapperProperties)
			{
				try
				{
					if (prop.IsDefined(typeof(ColumnAttribute), true))
					{
						// Get the accessor for the method info
						MethodInfo getMethodInfo = prop.GetAccessors().First();

						// Create a delegate for the accessor
						try
						{
							// Set up the null delegates
							GenericGetter getMethodDelegate = null;
							GenericSetter setMethodDelegate = null;

							// If the get delegate has not been mapped, create and store it
							if (!GetDelegateCache.TryGetValue(getMethodInfo.Name, out getMethodDelegate))
							{
								// If the get delegate has not been mapped, create and store it
								getMethodDelegate = CreateGetMethod(prop);
								lock (GetDelegateCache)
								{
									GetDelegateCache[prop.Name] = getMethodDelegate;
								}
							}

							// If the set delegate has not been mapped, create and store it
							if (!SetDelegateCache.TryGetValue(prop.Name, out setMethodDelegate))
							{
								setMethodDelegate = CreateSetMethod(prop);
								lock (SetDelegateCache)
								{
									SetDelegateCache[prop.Name] = setMethodDelegate;
								}
							}
							// Initialize the value to -1 using the delegate
							setMethodDelegate(this, -1);
						}
						catch (Exception e)
						{
							// It's an error, but only sort of
							log.Info(e.Message, e);
						}
					}
				}
				catch (InvalidCastException e)
				{
					log.ErrorFormat("Could not cast mapper property {0} from {1} to {2}", new object[] { prop.Name, "Int32", prop.PropertyType });
					log.Info(e);
				}
				catch (Exception e)
				{
					log.Error(e.Message, e);
					throw e;
				}
			}

			try
			{
				// Check to see how many values are stored in the value mappers cache
				if (ValueMappersDictionary.Count() == 0)
				{
					// Get the type of the IValueMapper interface
					Type valueMapperType = typeof(IValueMapper);

					// Search the application for classes that implement IValueMapper
					IEnumerable<Type> valueMappers = AppDomain.CurrentDomain.GetAssemblies().ToList().SelectMany(s => s.GetTypes()).Where(p => valueMapperType.IsAssignableFrom(p));

					// Loop through the list of IValueMappers where the mapper has a base type and has a generic type
					foreach (Type item in valueMappers.Where(mapper => null != mapper.BaseType && mapper.BaseType.GetGenericArguments().Length > 0))
					{
						try
						{
							// Get the first generic type parameter for the IValueMapper
							Type valueMapperGenericType = item.BaseType.GetGenericArguments().First();

							// If the IValueMapper has not been lodaed and cached yet, then do so now
							if (!ValueMappersDictionary.ContainsKey(valueMapperGenericType))
							{
								log.DebugFormat("Loading Mapper {0} for type {1}", item.Name, valueMapperGenericType.Name);
								ValueMappersDictionary.Add(valueMapperGenericType, item);
							}
						}
						catch (ArgumentException ex)
						{
							log.DebugFormat(ex.Message, ex);
						}
					}
				}
			}
			catch (ReflectionTypeLoadException e)
			{
				log.Error(e.Message);
				Exception[] loaderExceptions = e.LoaderExceptions;
				foreach (var item in loaderExceptions)
				{
					log.Info(item.Message);
				}
			}
			catch (Exception e)
			{
				log.Error(e.Message, e);
				throw e;
			}
		}

		public virtual Collection<T> MapAll(OracleDataReader reader)
		{
			Collection<T> collection = new Collection<T>();
			if (reader != null)
			{
				this.SetColumnOrdinals(reader);

				while (reader.Read())
				{
					try
					{
						collection.Add(Map(reader));
					}
					catch
					{
						throw;

						// NOTE:
						// consider handling exeption here instead of re-throwing
						// if graceful recovery can be accomplished
					}
				}
			}
			else
			{
				log.Error(string.Format("OracleDataReader is null, returning empty collection of {0}", typeof(T).FullName));
			}
			return collection;
		}

		/// <summary>
		/// Maps an individual database record to a Poco (Plain Old CLR Object) of the required type
		/// </summary>
		/// <param name="record">The result set row to process</param>
		/// <returns></returns>
		protected virtual T Map(IDataRecord record)
		{
			// Create a new instance
			T newObject = (T)Activator.CreateInstance(GenericType);

			try
			{
				// Loop through the mapper properties
				foreach (PropertyInfo prop in this.mapperProperties)
				{
					// Set the default property ordinal value (location in the result set) to 0
					int propertyOrdinalValue = 0;

					// Get the value of the mapper property using the delegate
					GenericGetter getterDelegate = null;
					if (!GetDelegateCache.TryGetValue(prop.Name, out getterDelegate))
					{
						// If the delegate is not found (shouldn't happen) fall back to the slower GetValue method
						propertyOrdinalValue = (int)prop.GetValue(this, null);
					}
					else
					{
						// Use the cached delegate to get the value
						propertyOrdinalValue = (int)getterDelegate(this);
					}

					// Only try to map those properties which were returned from the database
					if (propertyOrdinalValue >= 0)
					{
						// Get the PropertyInfo for the new object to return where the name equals the mapper object name
						if (!GenericTypePropertyCache.ContainsKey(prop.Name))
						{
							log.ErrorFormat("GenericTypePropertyCache Does Not Contain Key: {0}", prop.Name);
						}

						// Load the new object property from the cache which has the same (.Equals()) name as the mapper property
						PropertyInfo newObjectProperty = GenericTypePropertyCache[prop.Name];

						// Set up the new value mapper
						dynamic valueMapper = new StringValueMapper();
						IValueMapper tempValueMapper;

						// Load the appropriate value mapper object
						if (null == newObjectProperty)
						{
							// If the new object property could not be located, log an error
							log.WarnFormat("ValueMapper for property name {0} could not be located", prop.Name);
						}
						else if (ValueMapperInstances.TryGetValue(newObjectProperty.PropertyType, out tempValueMapper))
						{
							// Load the appropriate value mapper for the Type of the new object property
							valueMapper = ValueMapperInstances[newObjectProperty.PropertyType];
						}
						else if (ValueMapperInstances.TryGetValue(newObjectProperty.PropertyType.BaseType, out tempValueMapper))
						{
							// Load the appropriate value mapper for the Base Type of the new object property
							valueMapper = ValueMapperInstances[newObjectProperty.PropertyType.BaseType];
						}
						else if (newObjectProperty.PropertyType.IsEnum)
						{
							// If the property is an enum
							try
							{
								// Create an instance of the EnumValueMapper
								valueMapper = (IValueMapper)Activator.CreateInstance(ValueMappersDictionary[newObjectProperty.PropertyType.BaseType]);

								// Cache the EnumValueMapper instance
								ValueMapperInstances[newObjectProperty.PropertyType.BaseType] = valueMapper;
								if (log.IsDebugEnabled)
								{
									log.DebugFormat("Instantiated {0} to map {1}", valueMapper.GetType().Name, newObjectProperty.PropertyType.Name);
								}
							}
							catch (KeyNotFoundException)
							{
								log.DebugFormat("Could not locate ValueMapper for {0}. Defaulting to {1}", newObjectProperty.PropertyType.Name, valueMapper.GetType().Name);
								ValueMapperInstances[newObjectProperty.PropertyType.BaseType] = valueMapper;
							}
							catch (Exception e)
							{
								log.Warn(e.Message, e);
								continue;
							}
						}
						else if (!newObjectProperty.PropertyType.IsEnum)
						{
							// If the new object property type is not an enum
							try
							{
								valueMapper = (IValueMapper)Activator.CreateInstance(ValueMappersDictionary[newObjectProperty.PropertyType]);
								ValueMapperInstances[newObjectProperty.PropertyType] = valueMapper;
								if (log.IsDebugEnabled)
								{
									log.DebugFormat("Instantiated {0} to map {1}", valueMapper.GetType().Name, newObjectProperty.PropertyType.Name);
								}
							}
							catch (KeyNotFoundException)
							{
								log.DebugFormat("Could not locate ValueMapper for {0}. Defaulting to {1}", newObjectProperty.PropertyType.Name, valueMapper.GetType().Name);
								ValueMapperInstances[newObjectProperty.PropertyType] = valueMapper;
							}
							catch (Exception e)
							{
								log.Warn(e.Message, e);
								continue;
							}
						}
						else
						{
							log.WarnFormat("ValueMapper for property name {0} could not be located", prop.Name);
						}

						try
						{
							if (newObjectProperty == null)
							{
								log.DebugFormat("No property {0} was found in {1}", prop.Name, GenericType.Name);
							}
							else if (record.IsDBNull(propertyOrdinalValue) && newObjectProperty.PropertyType.Equals(typeof(Nullable)))
							{
								// If the record returned from the DB is NULL, and the new object property type is Nullable, then set the value to null
								newObjectProperty.SetValue(newObject, null, null);
							}
							else
							{
								GenericSetter setter = GenericTypeSetDelegateCache[prop.Name];
								if (newObjectProperty.PropertyType.IsEnum)
								{
									try
									{

										MethodInfo mapValueMethod = valueMapper.GetType().GetMethod("MapEnumValue");
										MethodInfo genericMethod = mapValueMethod.MakeGenericMethod(newObjectProperty.PropertyType);
										setter(newObject, genericMethod.Invoke(valueMapper, new Object[] { record, propertyOrdinalValue }));
									}
									catch (Exception e)
									{
										log.Error(e.Message, e);
									}
								}
								else
								{
									setter(newObject, valueMapper.MapValue(record, propertyOrdinalValue));
								}
							}
						}
						catch (Exception e)
						{
							throw e;
						}
					}
				}
			}
			catch (Exception e)
			{
				log.Error(e.Message, e);
				log.Error("Source: " + e.Source);
				log.Error("Stack: " + e.StackTrace);
				throw e;
			}
			return (T)newObject;
		}

		protected virtual void SetColumnOrdinals(OracleDataReader reader)
		{
			Type t = this.GetType();
			//PropertyInfo[] properties = t.GetProperties();
			Dictionary<string, string> columnNames = new Dictionary<string, string>(this.mapperProperties.Count());

			foreach (PropertyInfo prop in this.mapperProperties)
			{
				//log.DebugFormat("Currently mapping {0}", prop.Name);
				if (prop.IsDefined(typeof(ColumnAttribute), true))
				{
					ColumnAttribute column = (ColumnAttribute)prop.GetCustomAttributes(typeof(ColumnAttribute), true)[0];
					try
					{
						columnNames.Add(column.Name.ToUpper(), prop.Name);
						if (log.IsDebugEnabled)
						{
							log.DebugFormat("Mapping {0} to {1}", column.Name.ToUpper(), prop.Name);
						}
					}
					catch (Exception)
					{
						throw new ArgumentException(String.Format("The column {0} has already been mapped for {1}", column.Name.ToUpper(), t.Name));
					}
				}
				else if (log.IsDebugEnabled)
				{
					log.DebugFormat("{0} does not have a column attribute defined: skipping", prop.Name);
				}
			}

			// Loop through each field in the reader and attempt to map them to the hardcoded field names in the Mapper Class            
			for (int i = 0; i < reader.FieldCount; i++)
			{
				string columnName = reader.GetName(i);
				// If the column name exists try to set the property, else record an error
				if (columnNames.ContainsKey(columnName))
				{
					PropertyInfo prop = t.GetProperty(columnNames[columnName]);
					MethodInfo mi = prop.GetSetMethod();

					// Get the property using the delegate
					GenericSetter setter = null;
					if (!SetDelegateCache.TryGetValue(prop.Name, out setter))
					{
						// If the delegate is not found (shouldn't happen) fall back to the slower GetValue method
						prop.SetValue(this, reader.GetOrdinal(columnName), null);
					}
					else
					{
						// Use the cached delegate to get the value
						setter(this, reader.GetOrdinal(columnName));
					}
				}
				else if (log.IsInfoEnabled)
				{
					log.InfoFormat("A column attribute for {0} is not mapped in {1}", columnName, t.Name);
				}
			}
		}

		///
		/// Creates a dynamic setter for the property
		///
		private static GenericSetter CreateSetMethod(PropertyInfo propertyInfo)
		{
			/*
			* If there's no setter return null
			*/
			MethodInfo setMethod = propertyInfo.GetSetMethod();
			if (setMethod == null)
				return null;

			/*
			* Create the dynamic method
			*/
			Type[] arguments = new Type[2];
			arguments[0] = arguments[1] = typeof(object);

			DynamicMethod setter = new DynamicMethod(String.Concat("_Set", propertyInfo.Name, "_"), typeof(void), arguments, propertyInfo.DeclaringType);
			ILGenerator generator = setter.GetILGenerator();
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
			generator.Emit(OpCodes.Ldarg_1);

			if (propertyInfo.PropertyType.IsClass)
			{
				generator.Emit(OpCodes.Castclass, propertyInfo.PropertyType);
			}
			else
			{
				generator.Emit(OpCodes.Unbox_Any, propertyInfo.PropertyType);
			}

			generator.EmitCall(OpCodes.Callvirt, setMethod, null);
			generator.Emit(OpCodes.Ret);

			/*
			* Create the delegate and return it
			*/
			return (GenericSetter)setter.CreateDelegate(typeof(GenericSetter));
		}

		///
		/// Creates a dynamic getter for the property
		///
		private static GenericGetter CreateGetMethod(PropertyInfo propertyInfo)
		{
			/*
			* If there's no getter return null
			*/
			MethodInfo getMethod = propertyInfo.GetGetMethod();
			if (getMethod == null)
				return null;

			/*
			* Create the dynamic method
			*/
			Type[] arguments = new Type[1];
			arguments[0] = typeof(object);

			DynamicMethod getter = new DynamicMethod(String.Concat("_Get", propertyInfo.Name, "_"), typeof(object), arguments, propertyInfo.DeclaringType);
			ILGenerator generator = getter.GetILGenerator();
			generator.DeclareLocal(typeof(object));
			generator.Emit(OpCodes.Ldarg_0);
			generator.Emit(OpCodes.Castclass, propertyInfo.DeclaringType);
			generator.EmitCall(OpCodes.Callvirt, getMethod, null);

			if (!propertyInfo.PropertyType.IsClass)
				generator.Emit(OpCodes.Box, propertyInfo.PropertyType);

			generator.Emit(OpCodes.Ret);

			/*
			* Create the delegate and return it
			*/
			return (GenericGetter)getter.CreateDelegate(typeof(GenericGetter));
		}
	}
}
