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
	public class BooleanValueMapper : ValueMapperBase<Boolean>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(BooleanValueMapper));
		public override Boolean MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			bool value = false;
			Object temp;

			if (DBNull.Value != (temp = record[propertyOrdinalValue]))
			{
				if (temp.GetType() == typeof(String))
				{
					String recordValue = new StringValueMapper().MapValue(record, propertyOrdinalValue);
					Boolean isParseable = false;
					if (Boolean.TryParse(recordValue, out isParseable))
					{
						value = (String.IsNullOrWhiteSpace(recordValue) ? false : bool.Parse(recordValue));
					}
					else
					{
						if (recordValue.StartsWith("T", StringComparison.CurrentCultureIgnoreCase))
						{
							value = true;
						}
						else if (recordValue.StartsWith("F", StringComparison.CurrentCultureIgnoreCase))
						{
							value = false;
						}
						else if (recordValue.StartsWith("Y", StringComparison.CurrentCultureIgnoreCase))
						{
							value = true;
						}
						else if (recordValue.StartsWith("N", StringComparison.CurrentCultureIgnoreCase))
						{
							value = false;
						}
						else if (recordValue.StartsWith("0"))
						{
							value = false;
						}
						else if (recordValue.StartsWith("1"))
						{
							value = true;
						}
					}
				}
				else if (temp.GetType() == typeof(Int32))
				{
					Int32 recordValue = new Int32ValueMapper().MapValue(record, propertyOrdinalValue);
					value = recordValue != 0;
				}
				else if (temp.GetType() == typeof(Int64))
				{
					Int64 recordValue = new Int64ValueMapper().MapValue(record, propertyOrdinalValue);
					value = recordValue != 0;
				}
				else if (temp.GetType() == typeof(Int16))
				{
					Int16 recordValue = new Int16ValueMapper().MapValue(record, propertyOrdinalValue);
					value = recordValue != 0;
				}
				else if (temp.GetType() == typeof(Decimal) || temp.GetType() == typeof(OracleDecimal))
				{
					Decimal recordValue = new DecimalValueMapper().MapValue(record, propertyOrdinalValue);
					value = recordValue != 0;
				}
				else
				{
					log.Warn(string.Format("Found object type: {0}", record.GetValue(propertyOrdinalValue).GetType()));
					value = (record.IsDBNull(propertyOrdinalValue) ? false : record.GetBoolean(propertyOrdinalValue));
				}
			}
			return value;
		}
	}

	public class NullableBooleanValueMapper : ValueMapperBase<Nullable<Boolean>>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(NullableBooleanValueMapper));
		public override Nullable<Boolean> MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			Boolean? value = null;
			if (!record.IsDBNull(propertyOrdinalValue))
			{
				Type valueType = record.GetValue(propertyOrdinalValue).GetType();

				if (valueType.Equals(typeof(String)))
				{
					String recordValue = record.GetString(propertyOrdinalValue);
					try
					{
						value = Boolean.Parse(recordValue);
					}
					catch (FormatException)
					{
						if (recordValue.StartsWith("T", StringComparison.CurrentCultureIgnoreCase))
						{
							value = true;
						}
						else if (recordValue.StartsWith("F", StringComparison.CurrentCultureIgnoreCase))
						{
							value = false;
						}
						else if (recordValue.StartsWith("Y", StringComparison.CurrentCultureIgnoreCase))
						{
							value = true;
						}
						else if (recordValue.StartsWith("N", StringComparison.CurrentCultureIgnoreCase))
						{
							value = false;
						}
						else if (recordValue.StartsWith("0", StringComparison.CurrentCultureIgnoreCase))
						{
							value = false;
						}
						else if (recordValue.StartsWith("1", StringComparison.CurrentCultureIgnoreCase))
						{
							value = true;
						}
					}
				}
				else if (valueType.Equals(typeof(Int32)))
				{
					Int32 recordValue = record.GetInt32(propertyOrdinalValue);
					value = recordValue != 0;
				}
				else if (valueType.Equals(typeof(Int64)))
				{
					Int64 recordValue = record.GetInt64(propertyOrdinalValue);
					value = recordValue != 0;
				}
				else if (valueType.Equals(typeof(Int16)))
				{
					Int16 recordValue = record.GetInt16(propertyOrdinalValue);
					value = recordValue != 0;
				}
				else if (valueType.Equals(typeof(Decimal)))
				{
					Decimal recordValue = record.GetDecimal(propertyOrdinalValue);
					value = recordValue != 0;
				}
				else if (valueType.Equals(typeof(OracleDecimal)))
				{
					OracleDecimal recordValue = (OracleDecimal)record.GetValue(propertyOrdinalValue);
					value = recordValue.ToInt64() != 0;
				}
				else
				{
					log.Warn(string.Format("Found object type: {0}", valueType));
					value = record.GetBoolean(propertyOrdinalValue);
				}
			}
			return value;
		}
	}
}
