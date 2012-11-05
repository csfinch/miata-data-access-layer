using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;
using System.Numerics;

namespace Miata.Library.Mappers.ValueMappers
{
	public class SingleValueMapper : ValueMapperBase<Single>
	{
		public override Single MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Single value;
			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
			{
				value = ((OracleDecimal)(record[propertyOrdinalValue])).ToSingle();
			}
			else
			{
				value = Convert.ToSingle(record[propertyOrdinalValue].ToString());
			}
			return value;
		}
	}

	public class NullableSingleValueMapper : ValueMapperBase<Nullable<Single>>
	{
		public override Nullable<Single> MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Nullable<Single> value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDecimal)))
				{
					value = ((OracleDecimal)(record[propertyOrdinalValue])).ToSingle();
				}
				else
				{
					value = Convert.ToSingle(record[propertyOrdinalValue].ToString());
				}
			}
			return value;
		}
	}
}
