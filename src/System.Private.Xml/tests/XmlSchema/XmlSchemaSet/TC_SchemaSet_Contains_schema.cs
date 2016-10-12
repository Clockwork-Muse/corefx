// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_Contains_Schema", Desc = "")]
    public static class TC_SchemaSet_Contains_Schema
    {
        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v1 - Contains with null")]
        public static void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            // GLOBALIZATION
            Assert.Throws<ArgumentNullException>("schema", () => sc.Contains((XmlSchema)null));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v2 - Contains with not added schema")]
        public static void v2()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
#pragma warning disable 0618
            XmlSchemaCollection scl = new XmlSchemaCollection();
#pragma warning restore 0618

            XmlSchema Schema = scl.Add(null, TestData._XsdAuthor);

            Assert.False(sc.Contains(Schema));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v3 - Contains with existing schema, Remove it, Contains again", Priority = 0)]
        public static void v3()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
#pragma warning disable 0618
            XmlSchemaCollection scl = new XmlSchemaCollection();
#pragma warning restore 0618

            XmlSchema Schema = scl.Add(null, TestData._XsdAuthor);
            sc.Add(Schema);

            Assert.True(sc.Contains(Schema));

            sc.Remove(Schema);

            Assert.False(sc.Contains(Schema));

            return;
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v4 - Contains for added with URL", Priority = 0)]
        public static void v4()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchema Schema = sc.Add(null, TestData._XsdAuthor);

            Assert.True(sc.Contains(Schema));
        }
    }
}
