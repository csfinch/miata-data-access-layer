using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;

namespace Miata.Library.Mappers.ValueMappers
{
	public class Int16ValueMapper : ValueMapperBase<Int16>
	{
		public override Int16 MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Int16 value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = ((OracleDecimal)(record[propertyOrdinalValue])).ToInt16();
			}
			else
			{
				value = Convert.ToInt16(record[propertyOrdinalValue].ToString());
			}
			return value;
		}
	}

	public class NullableInt16ValueMapper : ValueMapperBase<Nullable<Int16>>
	{
		public override Int16? MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Int16? value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = ((OracleDecimal)(record[propertyOrdinalValue])).ToInt16();
				}
				else
				{
					value = Convert.ToInt16(record[propertyOrdinalValue].ToString());
				}
			}
			return value;
		}
	}

	public class UInt16ValueMapper : ValueMapperBase<UInt16>
	{
		public override UInt16 MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			UInt16 value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = Convert.ToUInt16(((OracleDecimal)(record[propertyOrdinalValue])).Value);
			}
			else
			{
				value = Convert.ToUInt16(record[propertyOrdinalValue].ToString());
			}
			return value;
		}
	}

	public class NullableUInt16ValueMapper : ValueMapperBase<Nullable<UInt16>>
	{
		public override UInt16? MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			UInt16? value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = Convert.ToUInt16(((OracleDecimal)(record[propertyOrdinalValue])).Value);
				}
				else
				{
					value = Convert.ToUInt16(record[propertyOrdinalValue].ToString());
				}
			}
			return value;
		}
	}
}
