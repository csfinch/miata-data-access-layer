using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.Data;

namespace Miata.Library.Translator
{
	public interface ITranslator<T>
	{
		T ParseRow(IDataRecord record);
		void SetColumnNumbers(IDataReader dbReader);
		void ParseTypeProperties();
	}
}
