// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml.Schema;
using Xunit;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_IsCompiled", Desc = "")]
    public static class TC_SchemaSet_IsCompiled
    {
        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v1 - IsCompiled on empty collection", Priority = 0)]
        [Fact]
        public static void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            Assert.False(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v2 - IsCompiled, add one, Compile, IsCompiled, add one IsCompiled", Priority = 0)]
        [Fact]
        public static void v2()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("xsdauthor1", TestData._XsdNoNs);

            Assert.False(sc.IsCompiled);

            sc.Compile();

            Assert.True(sc.IsCompiled);

            sc.Add("xsdauthor", TestData._XsdAuthor);

            Assert.False(sc.IsCompiled);

            sc.Compile();

            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v3 - Add two, Compile, remove one, IsCompiled")]
        [Fact]
        public static void v3()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("xsdauthor1", TestData._XsdNoNs);
            XmlSchema Schema1 = sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Compile();
            sc.Remove(Schema1);

            Assert.False(sc.IsCompiled);
        }
    }
}
