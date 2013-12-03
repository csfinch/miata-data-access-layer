using Common.Logging;
using System.Data.Common;

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
