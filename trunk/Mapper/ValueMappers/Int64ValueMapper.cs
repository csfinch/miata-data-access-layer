using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;

namespace Miata.Library.Mappers.ValueMappers
{
	public class Int64ValueMapper : ValueMapperBase<Int64>
	{
		public override Int64 MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Int64 value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = ((OracleDecimal)(record[propertyOrdinalValue])).ToInt64();
			}
			else
			{
				value = Convert.ToInt64(record[propertyOrdinalValue].ToString());
			}
			return value;
		}
	}

	public class NullableInt64ValueMapper : ValueMapperBase<Nullable<Int64>>
	{
		public override Int64? MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Int64? value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = ((OracleDecimal)(record[propertyOrdinalValue])).ToInt64();
				}
				else
				{
					value = Convert.ToInt64(record[propertyOrdinalValue].ToString());

				}
			}
			return value;
		}
	}

	public class UInt64ValueMapper : ValueMapperBase<UInt64>
	{
		public override UInt64 MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			UInt64 value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = Convert.ToUInt64(((OracleDecimal)(record[propertyOrdinalValue])).Value);
			}
			else
			{
				value = Convert.ToUInt64(record[propertyOrdinalValue].ToString());
			}
			return value;
		}
	}

	public class NullableUInt64ValueMapper : ValueMapperBase<Nullable<UInt64>>
	{
		public override UInt64? MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			UInt64? value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = Convert.ToUInt64(((OracleDecimal)(record[propertyOrdinalValue])).Value);
				}
				else
				{
					value = Convert.ToUInt64(record[propertyOrdinalValue].ToString());

				}
			}
			return value;
		}
	}
}
