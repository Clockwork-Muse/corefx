// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections;
using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_CopyTo", Desc = "")]
    public static class TC_SchemaSet_CopyTo
    {
        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v1 - CopyTo with array = null")]
        public static void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("xsdauthor", TestData._XsdAuthor);
            // GLOBALIZATION
            Assert.Throws<ArgumentNullException>("schemas", () => sc.CopyTo(null, 0));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v2 - ICollection.CopyTo with array = null")]
        public static void v2()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("xsdauthor", TestData._XsdAuthor);
            ICollection Col = sc.Schemas();
            // GLOBALIZATION
            Assert.Throws<ArgumentNullException>("dest", () => Col.CopyTo(null, 0));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v3 - ICollection.CopyTo with array smaller than source", Priority = 0)]
        public static void v3()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(null, TestData._XsdNoNs);
            ICollection Col = sc.Schemas();
            XmlSchema[] array = new XmlSchema[1];
            // GLOBALIZATION
            Assert.Throws<ArgumentException>(string.Empty, () => Col.CopyTo(array, 0));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v4 - CopyTo with index < 0")]
        public static void v4()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(null, TestData._XsdNoNs);
            XmlSchema[] array = new XmlSchema[1];
            // GLOBALIZATION
            Assert.Throws<ArgumentOutOfRangeException>("index", () => sc.CopyTo(array, -1));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v5 - ICollection.CopyTo with index < 0")]
        public static void v5()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add("xsdauthor", TestData._XsdAuthor);
            sc.Add(null, TestData._XsdNoNs);
            ICollection Col = sc.Schemas();
            XmlSchema[] array = new XmlSchema[1];
            // GLOBALIZATION
            Assert.Throws<ArgumentOutOfRangeException>("dstIndex", () => Col.CopyTo(array, -1));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v6 - Filling last two positions of array", Priority = 0)]
        public static void v6()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchema Schema1 = sc.Add("xsdauthor", TestData._XsdAuthor);
            XmlSchema Schema2 = sc.Add(null, TestData._XsdNoNs);

            XmlSchema[] array = new XmlSchema[10];
            sc.CopyTo(array, 8);

            Assert.Same(Schema1, array[8]);
            Assert.Same(Schema2, array[9]);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v7 - Copy all to array of the same size", Priority = 0)]
        public static void v7()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchema Schema1 = sc.Add("xsdauthor", TestData._XsdAuthor);
            XmlSchema Schema2 = sc.Add(null, TestData._XsdNoNs);

            XmlSchema[] array = new XmlSchema[2];
            sc.CopyTo(array, 0);

            Assert.Same(Schema1, array[0]);
            Assert.Same(Schema2, array[1]);
        }

        [Fact]
        //[Variation(Desc = "v8 - 378346: CopyTo throws correct exception for index < 0 but incorrect exception for index > maxLength of array.", Priority = 0)]
        public static void v8()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            Assert.Throws<ArgumentOutOfRangeException>("index", () => ss.CopyTo(new XmlSchema[2], 3));
        }
    }
}
