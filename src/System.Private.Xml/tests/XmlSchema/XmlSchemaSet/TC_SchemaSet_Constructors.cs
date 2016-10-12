// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_Constructors", Desc = "", Priority = 0)]
    public static class TC_SchemaSet_Constructors
    {
        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v1 - Default constructor", Priority = 1)]
        public static void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v2 - XmlSchemaSet(XmlNameTable = 0)")]
        public static void v2()
        {
            // GLOBALIZATION
            Assert.Throws<ArgumentNullException>("nameTable", () => new XmlSchemaSet(null));
        }

        [Fact]
        //[Variation(Desc = "v3 - XmlDataSourceResolver(XmlNameTable = valid) check back")]
        public static void v3()
        {
            NameTable NT = new NameTable();
            XmlSchemaSet sc = new XmlSchemaSet(NT);
            Assert.Same(sc.NameTable, NT);
        }
    }
}
