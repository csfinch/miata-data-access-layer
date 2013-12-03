using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Miata.Library.Factory;
using System.Reflection;

namespace Miata.Library.Utils
{
    public static class ObjectHelper
    {

        private static List<Type> UpCastApprovedTypes
        {
            get
            {
                List<Type> _approvedTypes = new List<Type>();
                _approvedTypes = new List<Type>();

                _approvedTypes.Add(typeof(short));
                _approvedTypes.Add(typeof(int));
                _approvedTypes.Add(typeof(long));
                _approvedTypes.Add(typeof(string));
                _approvedTypes.Add(typeof(DateTime));
                _approvedTypes.Add(typeof(double));
                _approvedTypes.Add(typeof(decimal));
                _approvedTypes.Add(typeof(float));
                _approvedTypes.Add(typeof(List<>));
                _approvedTypes.Add(typeof(bool));
                _approvedTypes.Add(typeof(Boolean));
                _approvedTypes.Add(typeof(uint));
                _approvedTypes.Add(typeof(ushort));
                _approvedTypes.Add(typeof(ulong));

                _approvedTypes.Add(typeof(short?));
                _approvedTypes.Add(typeof(int?));
                _approvedTypes.Add(typeof(long?));
                _approvedTypes.Add(typeof(DateTime?));
                _approvedTypes.Add(typeof(double?));
                _approvedTypes.Add(typeof(decimal?));
                _approvedTypes.Add(typeof(float?));
                _approvedTypes.Add(typeof(bool?));
                _approvedTypes.Add(typeof(List<>));
                _approvedTypes.Add(typeof(Boolean?));
                _approvedTypes.Add(typeof(uint?));
                _approvedTypes.Add(typeof(ushort?));
                _approvedTypes.Add(typeof(ulong?));

                // Experimental:
                _approvedTypes.Add(typeof(StringBuilder));

                _approvedTypes.Add(typeof(byte));
                _approvedTypes.Add(typeof(byte[]));
                _approvedTypes.Add(typeof(byte?));
                _approvedTypes.Add(typeof(byte?[]));

                _approvedTypes.Add(typeof(char));
                _approvedTypes.Add(typeof(char[]));
                _approvedTypes.Add(typeof(char?));
                _approvedTypes.Add(typeof(char?[]));

                _approvedTypes.Add(typeof(sbyte));
                _approvedTypes.Add(typeof(sbyte[]));
                _approvedTypes.Add(typeof(sbyte?));
                _approvedTypes.Add(typeof(sbyte?[]));

                _approvedTypes.Add(typeof(Enum));

                return _approvedTypes;
            }
        }

        public static T UpCast<T>(this object baseObject, params string[] excludeProps)
        {
            var sourceType = baseObject.GetType();
            Type derivedType = typeof(T);
            var result = ObjectFactory<T>.CreateObject();

            foreach (PropertyInfo sourceProperty in sourceType.GetProperties())
            {
                //Skip if in the exclude list
                if (excludeProps.Contains(sourceProperty.Name) != true && UpCastApprovedTypes.Contains(sourceProperty.PropertyType))
                {
                    var sourcePropertyValue = sourceProperty.GetValue(baseObject, null);
                    if (sourcePropertyValue != null)
                    {
                        var destinationProperty = derivedType.GetProperty(sourceProperty.Name);
                        if (destinationProperty != null)
                        {
                            try { destinationProperty.SetValue(result, sourcePropertyValue, null); }
                            catch { }
                        }
                    }
                }
            }

            return result;
        }

    }
}
