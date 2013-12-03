using System;
using System.Collections.Generic;
using System.Data.Linq.Mapping;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiataLibrary.Tests.TestObjects
{
    internal class TestObject
    {
        [Column(Name="Id")]
        public int Id { get; set; }

        [Column(Name = "Description")]
        public string Description { get; set; }

        [Column(Name = "CreatedDate")]
        public DateTime CreatedDate { get; set; }

        [Column(Name = "Status")]
        public string Status { get; set; }
    }
}
