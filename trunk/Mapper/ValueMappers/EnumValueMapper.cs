using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using Oracle.DataAccess.Types;
using System.Numerics;
using System.Reflection;
using log4net;
using UHCL.Library.Helpers;

namespace Miata.Library.Mappers.ValueMappers
{
	public class EnumValueMapper : ValueMapperBase<Enum>
	{
		private static readonly ILog log = LogManager.GetLogger(typeof(EnumValueMapper));

    public override T MapEnumValue<T>(IDataRecord record, int propertyOrdinalValue)
		{
			Type type = typeof(T);
			if (!type.IsEnum)
			{
				throw new ArgumentException("EnumValueMapper<T> must be of type System.Enum");
			}

			T returnEnum = default(T);
			String dbValue = new StringValueMapper().MapValue(record, propertyOrdinalValue);
			log.DebugFormat("Searching for Enum Value: {0}", dbValue);
			if (Enum.IsDefined(type, dbValue))
			{
				returnEnum = (T)Enum.Parse(type, dbValue);
				log.DebugFormat("Searched Enum Values and found ", returnEnum);
			}
			else
			{
				Boolean foundValue = false;
				if (!foundValue)
				{
					try
					{
						returnEnum = EnumHelper.GetValueFromDefaultValue<T>(dbValue);
						log.DebugFormat("Searched Enum Default Value Attribute and found ", returnEnum);
						foundValue = true;
					}
					catch (ArgumentException e)
					{
						// Do nothing because it is not really an exception
						log.Debug("Searched Enum Default Value Attribute and found nothing");
					}
					catch (Exception)
					{
						throw;
					}
				}
				
				if (!foundValue)
				{
					try
					{
						returnEnum = EnumHelper.GetValueFromDatabase<T>(dbValue);
						log.DebugFormat("Searched Enum Database Value Attribute and found ", returnEnum);
						foundValue = true;
					}
					catch (ArgumentException e)
					{
						// Do nothing because it is not really an exception
						log.Debug("Searched Enum Database Value Attribute and found nothing");
					}
					catch (Exception)
					{
						throw;
					}
				}
				
				if (!foundValue)
				{
					try
					{
						returnEnum = EnumHelper.GetValueFromDescription<T>(dbValue);
						log.DebugFormat("Searched Enum Description Attribute and found ", returnEnum);
						foundValue = true;
					}
					catch (ArgumentException e)
					{
						// Do nothing because it is not really an exception
						log.Debug("Searched Enum Description Attribute and found nothing");
					}
					catch (Exception)
					{
						throw;
					}
				}
			}
			log.DebugFormat("Enum Value: {0}", returnEnum);
			return returnEnum;
		}
	}
}
