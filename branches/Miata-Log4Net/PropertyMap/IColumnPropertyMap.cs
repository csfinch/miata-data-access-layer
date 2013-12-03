using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Miata.Library.PropertyMap
{
	/// <summary>
	/// Stores a mapping between an SQL column and a POCO object property
	/// </summary>
	public interface IColumnPropertyMap
	{
		/// <summary>
		/// Source DbDataReader column ordinal
		/// </summary>
		int ColumnOrdinal { get; set; }

		/// <summary>
		/// Source DbDataReader column name
		/// </summary>
		string ColumnName { get; set; }

		/// <summary>
		///  Column Property Type
		/// </summary>
		Type ColumnPropertyType { get; set; }

		/// <summary>
		///  POCO Property Type
		/// </summary>
		Type ObjectPropertyType { get; set; }

		/// <summary>
		/// Target POCO Property
		/// </summary>
		PropertyInfo Property { get; set; }

		/// <summary>
		/// Whether or not the property exists in the SQL Result
		/// </summary>
		bool SQLExists { get; set; }
	}
}
