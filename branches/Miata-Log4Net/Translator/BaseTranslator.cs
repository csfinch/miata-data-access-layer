using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;
using Miata.Library.PropertyMap;
using System.Reflection;
using System.Data.Linq.Mapping;
using System.Data.Common;
using System.Data;

namespace Miata.Library.Translator
{
	public class BaseTranslator<T> : AbstractTranslator<T>
	{
		// Logging instance
		private static readonly ILog log = LogManager.GetLogger(typeof(BaseTranslator<T>));

		public BaseTranslator()
		{
			base.ParseTypeProperties();
		}

		public BaseTranslator(DbDataReader dbReader)
		{
			base.ParseTypeProperties();
			base.SetColumnNumbers(dbReader);
		}
	}
}
