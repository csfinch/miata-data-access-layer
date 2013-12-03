using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Miata.Library.PropertyMap;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using MiataLibrary.Tests.TestObjects;
namespace Miata.Library.PropertyMap.Tests
{
    [TestClass()]
    public class ColumnPropertyMapTests
    {
        private PropertyInfo[] _typeProperties;
        private PropertyInfo _property;

        [TestInitialize]
        public void Initialize()
        {
            _typeProperties = typeof(TestObject).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            _property = _typeProperties.FirstOrDefault(item => item.IsDefined(typeof(System.Data.Linq.Mapping.ColumnAttribute), true) || item.IsDefined(typeof(System.ComponentModel.DataAnnotations.Schema.ColumnAttribute), true));
        }

        [TestMethod()]
        public void ColumnPropertyMapConstructorTest()
        {
            var propertyMap = new ColumnPropertyMap(_property);
            // Make sure the object is not null
            Assert.IsNotNull(propertyMap);
        }

        [TestMethod()]
        public void ColumnPropertyMapColumnNameTest()
        {
            var columnName = string.Empty;
            var attributes = _property.GetCustomAttributes(typeof(System.Data.Linq.Mapping.ColumnAttribute));
            
            if (attributes.Any())
            {
                columnName = ((System.Data.Linq.Mapping.ColumnAttribute)attributes.First()).Name;
            } else {
                columnName = ((System.ComponentModel.DataAnnotations.Schema.ColumnAttribute)attributes.First()).Name;
            }

            var expected = columnName.ToUpper();
            var propertyMap = new ColumnPropertyMap(_property);
            var actual = propertyMap.ColumnName;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ColumnPropertyMapPropertyTest()
        {
            var expected = _property;
            var propertyMap = new ColumnPropertyMap(_property);
            var actual = propertyMap.Property;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ColumnPropertyMapObjectPropertyTest()
        {
            var expected = _property.PropertyType;
            var propertyMap = new ColumnPropertyMap(_property);
            var actual = propertyMap.ObjectPropertyType;
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ColumnPropertyMapGetMethodTest()
        {
            var propertyMap = new ColumnPropertyMap(_property);
            var actual = propertyMap.GetMethod;
            Assert.IsNotNull(actual);
        }

        [TestMethod()]
        public void ColumnPropertyMapGetMethodTest2()
        {
            var expected = new Random().Next(1000000);
            var testObject = new TestObject() { Id = expected };
            var propertyMap = new ColumnPropertyMap(_property);
            var getMethod = propertyMap.GetMethod;
            var actual = getMethod.Invoke(testObject);
            
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ColumnPropertyMapSetMethodTest()
        {
            var propertyMap = new ColumnPropertyMap(_property);
            var actual = propertyMap.SetMethod;
            Assert.IsNotNull(actual);
        }

        [TestMethod()]
        public void ColumnPropertyMapSetMethodTest2()
        {
            var expected = new Random().Next(1000000);
            var testObject = new TestObject();
            var propertyMap = new ColumnPropertyMap(_property);
            var setMethod = propertyMap.SetMethod;
            setMethod.Invoke(testObject, expected);

            var actual = testObject.Id;

            Assert.AreEqual(expected, actual);
        }
    }
}
