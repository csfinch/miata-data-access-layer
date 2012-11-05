using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;
using System.Numerics;

namespace Miata.Library.Mappers.ValueMappers
{
	public class DecimalValueMapper : ValueMapperBase<Decimal>
	{
		public override Decimal MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Decimal value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = ((OracleDecimal)(record[propertyOrdinalValue])).Value;
			}
			else
			{
				value = Convert.ToDecimal(record[propertyOrdinalValue].ToString());
			}
			return value;
		}
	}

	public class NullableDecimalValueMapper : ValueMapperBase<Nullable<Decimal>>
	{
		public override Nullable<Decimal> MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Nullable<Decimal> value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = ((OracleDecimal)(record[propertyOrdinalValue])).Value;
				}
				else
				{
					value = Convert.ToDecimal(record[propertyOrdinalValue].ToString());
				}
			}
			return value;
		}
	}
}
