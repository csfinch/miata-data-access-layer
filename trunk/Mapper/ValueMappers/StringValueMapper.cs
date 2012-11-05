using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;
using System.Numerics;

namespace Miata.Library.Mappers.ValueMappers
{
	public class StringValueMapper : ValueMapperBase<String>
	{
		public override String MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			string value = string.Empty;
			Object temp;
			if (DBNull.Value != (temp = record[propertyOrdinalValue]))
			{
				if (temp.GetType() == typeof(OracleString))
				{
					value = ((OracleString)temp).Value;
				}
				else if (temp.GetType() == typeof(OracleClob))
				{
					value = ((OracleClob)temp).Value;
				}
				else
				{
					value = temp.ToString();
				}
			}
			return value;
		}
	}
}
