using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;
using System.Numerics;

namespace Miata.Library.Mappers.ValueMappers
{
	public class BigIntegerValueMapper : ValueMapperBase<BigInteger>
	{
		public override BigInteger MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			BigInteger value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = new BigInteger(((OracleDecimal)(record[propertyOrdinalValue])).Value);
			}
			else
			{
				value = new BigInteger(Convert.ToDecimal(record[propertyOrdinalValue].ToString()));
			}
			return value;
		}
	}

	public class NullableBigIntegerValueMapper : ValueMapperBase<Nullable<BigInteger>>
	{
		public override Nullable<BigInteger> MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Nullable<BigInteger> value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = new BigInteger(((OracleDecimal)(record[propertyOrdinalValue])).Value);
				}
				else
				{
					value = new BigInteger(Convert.ToDecimal(record[propertyOrdinalValue].ToString()));
				}
			}
			return value;
		}
	}
}
