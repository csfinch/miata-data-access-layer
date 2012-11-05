using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;
using System.Numerics;

namespace Miata.Library.Mappers.ValueMappers
{
	public class DoubleValueMapper : ValueMapperBase<Double>
	{
		public override Double MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Double value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = ((OracleDecimal)(record[propertyOrdinalValue])).ToDouble();
			}
			else
			{
				value = Convert.ToDouble(record[propertyOrdinalValue].ToString());
			}
			return value;
		}
	}

	public class NullableDoubleValueMapper : ValueMapperBase<Nullable<Double>>
	{
		public override Nullable<Double> MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Nullable<Double> value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = ((OracleDecimal)(record[propertyOrdinalValue])).ToDouble();
				}
				else
				{
					value = Convert.ToDouble(record[propertyOrdinalValue].ToString());
				}
			}
			return value;
		}
	}
}
