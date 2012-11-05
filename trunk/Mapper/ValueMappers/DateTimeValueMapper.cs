using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;

namespace Miata.Library.Mappers.ValueMappers
{
	public class DateTimeValueMapper : ValueMapperBase<DateTime>
	{
		public override DateTime MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			DateTime value;

			if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDate)))
			{
				value = ((OracleDate)(record[propertyOrdinalValue])).Value;
			}
			else if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleTimeStamp)))
			{
				value = ((OracleTimeStamp)(record[propertyOrdinalValue])).Value;
			}
			else if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleTimeStampTZ)))
			{
				value = ((OracleTimeStampTZ)(record[propertyOrdinalValue])).Value;
			}
			else if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleTimeStampLTZ)))
			{
				value = ((OracleTimeStampLTZ)(record[propertyOrdinalValue])).Value;
			}
			else
			{
				value = record.GetDateTime(propertyOrdinalValue);
			}

			return record.GetDateTime(propertyOrdinalValue);
		}
	}

	public class NullableDateTimeValueMapper : ValueMapperBase<Nullable<DateTime>>
	{
		public override Nullable<DateTime> MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			DateTime? value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleDate)))
				{
					value = ((OracleDate)(record[propertyOrdinalValue])).Value;
				}
				else if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleTimeStamp)))
				{
					value = ((OracleTimeStamp)(record[propertyOrdinalValue])).Value;
				}
				else if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleTimeStampTZ)))
				{
					value = ((OracleTimeStampTZ)(record[propertyOrdinalValue])).Value;
				}
				else if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleTimeStampLTZ)))
				{
					value = ((OracleTimeStampLTZ)(record[propertyOrdinalValue])).Value;
				}
				else
				{
					value = record.GetDateTime(propertyOrdinalValue);
				}
			}
			return value;
		}
	}
}
