using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Miata.Library.Mappers.ValueMappers
{
	public interface IValueMapper
	{
	}

	public abstract class AbstractValueMapper<T> : IValueMapper
	{
		public abstract T MapValue(IDataRecord record, int propertyOrdinalValue);

		public abstract T MapEnumValue<T>(IDataRecord record, int propertyOrdinalValue);
	}

	public class ValueMapperBase<T> : AbstractValueMapper<T>
	{
		public override T MapValue(IDataRecord record, int propertyOrdinalValue)
		{
			throw new NotImplementedException();
		}

    public override T MapEnumValue<T>(IDataRecord record, int propertyOrdinalValue)
		{
			throw new NotImplementedException();
		}
	}
}
