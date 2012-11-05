using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;

namespace Miata.Library.Mappers.ValueMappers
{
	public class Int32ValueMapper : ValueMapperBase<Int32>
	{
		public override Int32 MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Int32 value = (record.IsDBNull(propertyOrdinalValue) ? -1 : Int32.Parse(record[propertyOrdinalValue].ToString()));
			return value;
		}
	}

	public class NullableInt32ValueMapper : ValueMapperBase<Nullable<Int32>>
	{
		public override Int32? MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			int? value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = ((OracleDecimal)(record[propertyOrdinalValue])).ToInt32();
				}
				else
				{
					value = Convert.ToInt32(record[propertyOrdinalValue].ToString());
				}
			}
			return value;
		}
	}

	public class UInt32ValueMapper : ValueMapperBase<UInt32>
	{
		public override UInt32 MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			UInt32 value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = Convert.ToUInt32(((OracleDecimal)(record[propertyOrdinalValue])).Value);
			}
			else
			{
				value = Convert.ToUInt32(record[propertyOrdinalValue].ToString());
			}
			return value;
		}
	}

	public class NullableUInt32ValueMapper : ValueMapperBase<Nullable<UInt32>>
	{
		public override UInt32? MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			UInt32? value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = Convert.ToUInt32(((OracleDecimal)(record[propertyOrdinalValue])).Value);
				}
				else
				{
					value = Convert.ToUInt32(record[propertyOrdinalValue].ToString());
				}
			}
			return value;
		}
	}
}
