using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;
using System.Numerics;
using log4net;

namespace Miata.Library.Mappers.ValueMappers
{
	public class ByteArrayValueMapper : ValueMapperBase<byte[]>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(ByteArrayValueMapper));
		public override byte[] MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			byte[] value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleBlob)))
				{
					value = ((OracleBlob)(record[propertyOrdinalValue])).Value;
				}
				else if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleBinary)))
				{
					value = ((OracleBinary)(record[propertyOrdinalValue])).Value;
				}
				else if (record[propertyOrdinalValue].GetType().Equals(typeof(OracleBFile)))
				{
					value = ((OracleBFile)(record[propertyOrdinalValue])).Value;
				}
				else if (record[propertyOrdinalValue].GetType().Equals(typeof(Byte[])))
				{
					value = (Byte[])(record[propertyOrdinalValue]);
				}
				else
				{
					log.Error(string.Format("Unknown byte array source: {0}", record[propertyOrdinalValue].GetType()));
				}
			}
			return value;
		}
	}
}
