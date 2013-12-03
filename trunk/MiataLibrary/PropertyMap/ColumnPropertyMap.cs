using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Reflection.Emit;
using System.Data.Linq.Mapping;
using System.ComponentModel.DataAnnotations.Schema;

namespace Miata.Library.PropertyMap
{
	// Generic Setter function delegate
	public delegate void GenericSetter(object target, object value);

	// Generic Getter function delegate
	public delegate object GenericGetter(object target);

	public class ColumnPropertyMap : IColumnPropertyMap
	{
		/// <summary>
        /// Ordinal position (column number) of this property within the Result Set
        /// </summary>
        public int ColumnOrdinal { get; set; }

        /// <summary>
        /// Name of the column name of the property within the Result Set
        /// </summary>
		public string ColumnName { get; set; }

        /// <summary>
        /// Result Set column property type
        /// </summary>
		public Type ColumnPropertyType { get; set; }

        /// <summary>
        /// Object property type
        /// </summary>
		public Type ObjectPropertyType { get; set; }

        /// <summary>
        /// The PropertyInfo for the mapped property
        /// </summary>
		public PropertyInfo Property { get; set; }

		/// <summary>
		/// Compiled MSIL getter for the mapped property
		/// </summary>
        public GenericGetter GetMethod { get; set; }

		/// <summary>
		/// Compiled MSIL setter for the mapped property
		/// </summary>
        public GenericSetter SetMethod { get; set; }

		/// <summary>
		/// Whether or not the property/column was found in the ResultSet
		/// </summary>
        public bool SQLExists { get; set; }

		/// <summary>
		/// Create a new map based on the PropertyInfo provided
		/// </summary>
		/// <param name="property">The object property to create a map for</param>
        /// <exception cref="ArgumentNullException">Thrown when the supplied property is null</exception>
        public ColumnPropertyMap(PropertyInfo property)
		{
			if (property == null)
				throw new ArgumentNullException("The property provided was null");

			if (property.IsDefined(typeof(System.Data.Linq.Mapping.ColumnAttribute), true))
			{
				System.Data.Linq.Mapping.ColumnAttribute column = (System.Data.Linq.Mapping.ColumnAttribute)property.GetCustomAttributes(typeof(System.Data.Linq.Mapping.ColumnAttribute), true)[0];
				this.ColumnName = column.Name.ToUpper();
			}
			else if (property.IsDefined(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute), true))
			{
				System.ComponentModel.DataAnnotations.Schema.ColumnAttribute column = (System.ComponentModel.DataAnnotations.Schema.ColumnAttribute)property.GetCustomAttributes(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute), true)[0];
				this.ColumnName = column.Name.ToUpper();
			}

			this.Property = property;
			this.ObjectPropertyType = property.PropertyType;
			this.GetMethod = this.CreateGetMethod();
			this.SetMethod = this.CreateSetMethod();
			this.SQLExists = false;
		}

		/// <summary>
        /// Creates a dynamic setter for the property
		/// </summary>
        /// <returns>Compiled MSIL setter for the mapped property or null is no Set method is found</returns>
        private GenericSetter CreateSetMethod()
		{
			PropertyInfo propertyInfo = this.Property;
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

		/// <summary>
        /// Creates a dynamic getter for the property
        /// </summary>
        /// <returns>Compiled MSIL getter for the mapped property or null is no Get method is found</returns>
		private GenericGetter CreateGetMethod()
		{
			PropertyInfo propertyInfo = this.Property;
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
