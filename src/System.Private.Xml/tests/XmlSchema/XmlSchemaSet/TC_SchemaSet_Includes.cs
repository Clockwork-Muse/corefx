// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Linq;
using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_Includes", Desc = "")]
    public static class TC_SchemaSet_Includes
    {
        //-----------------------------------------------------------------------------------
        [Theory]
        //[Variation(Desc = "v1.6 - Include: A(ns-a) include B(ns-a) which includes C(ns-a) ", Priority = 2, Params = new object[] { "include_v7_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v7_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v1.5 - Include: A with NS includes B and C with no NS", Priority = 2, Params = new object[] { "include_v6_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v6_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v1.4 - Include: A with NS includes B and C with no NS, B also includes C", Priority = 2, Params = new object[] { "include_v5_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v5_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v1.3 - Include: A with NS includes B with no NS, which includes C with no NS", Priority = 2, Params = new object[] { "include_v4_a.xsd", 1, "ns-a:c-e2" })]
        [InlineData("include_v4_a.xsd", 1, "ns-a:c-e2")]
        //[Variation(Desc = "v1.2 - Include: A with no NS includes B with no NS", Priority = 0, Params = new object[] { "include_v3_a.xsd", 1, "e2" })]
        [InlineData("include_v3_a.xsd", 1, "e2")]
        //[Variation(Desc = "v1.1 - Include: A with NS includes B with no NS", Priority = 0, Params = new object[] { "include_v1_a.xsd", 1, "ns-a:e2" })]
        [InlineData("include_v1_a.xsd", 1, "ns-a:e2")]
        public static void v1(string filename1, int count, string filename2)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, filename1)); // param as filename
            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(count, sc.Count);

            // Check that B's data is present in the NS for A
            Assert.Contains(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals(filename2));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v2 - Include: A with NS includes B with a diff NS (INVALID)", Priority = 1)]
        [Fact]
        public static void v2()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema schema = new XmlSchema();
            sc.Add(null, TestData._XsdNoNs);
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();

            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            Assert.Throws<XmlSchemaException>(() => sc.Add(null, Path.Combine(TestData._Root, "include_v2.xsd")));

            // no schema should be addded to the set.
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v3 - Include: A(ns-a) which includes B(ns-a) twice", Priority = 2)]
        public static void v8()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v8_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v4 - Include: A(ns-a) which includes B(No NS) twice", Priority = 2)]
        public static void v9()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v9_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v5 - Include: A,B,C all include each other, all with no ns and refer each others' types", Priority = 2)]
        public static void v10()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v10_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("c-e2"));
            // Check that B's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("b-e1"));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v6 - Include: A,B,C all include each other, all with same ns and refer each others' types", Priority = 2)]
        [Fact]
        public static void v11()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v11_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            // Check that A's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e1"));

            // Check B's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));

            // Check C's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e3"));
        }

        //[Variation(Desc = "v12 - 20008213 SOM: SourceUri property on a chameleon include is not set", Priority = 1)]
        [Fact]
        public static void v12()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();

            XmlSchema a = ss.Add(null, Path.Combine(TestData._Root, "include_v12_a.xsd"));
            Assert.Equal(1, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(1, ss.Count);
            Assert.True(ss.IsCompiled);

            Assert.All(a.Includes.Cast<XmlSchemaExternal>(), schema => Assert.False(string.IsNullOrWhiteSpace(schema.Schema.SourceUri)));
            Assert.Contains(a.Includes.Cast<XmlSchemaExternal>(), schema => schema.Schema.SourceUri.EndsWith("include_v12_b.xsd"));
        }

        /******** reprocess compile include **********/

        [Theory]
        //[Variation(Desc = "v101.6 - Include: A(ns-a) include B(ns-a) which includes C(ns-a) ", Priority = 2, Params = new object[] { "include_v7_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v7_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v101.5 - Include: A with NS includes B and C with no NS", Priority = 2, Params = new object[] { "include_v6_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v6_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v101.4 - Include: A with NS includes B and C with no NS, B also includes C", Priority = 2, Params = new object[] { "include_v5_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v5_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v101.3 - Include: A with NS includes B with no NS, which includes C with no NS", Priority = 2, Params = new object[] { "include_v4_a.xsd", 1, "ns-a:c-e2" })]
        [InlineData("include_v4_a.xsd", 1, "ns-a:c-e2")]
        //[Variation(Desc = "v101.2 - Include: A with no NS includes B with no NS", Priority = 0, Params = new object[] { "include_v3_a.xsd", 1, "e2" })]
        [InlineData("include_v3_a.xsd", 1, "e2")]
        //[Variation(Desc = "v101.1 - Include: A with NS includes B with no NS", Priority = 0, Params = new object[] { "include_v1_a.xsd", 1, "ns-a:e2" })]
        [InlineData("include_v1_a.xsd", 1, "ns-a:e2")]
        public static void v101(string filename1, int count, string expected)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, filename1)); // param as filename
            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(count, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(count, sc.Count);

            // Check that B's data is present in the NS for A
            Assert.Contains(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals(expected));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v102 - Include: A with NS includes B with a diff NS (INVALID)", Priority = 1)]
        [Fact]
        public static void v102()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema schema = new XmlSchema();
            sc.Add(null, TestData._XsdNoNs);
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            Assert.Throws<ArgumentException>("schema", () => sc.Reprocess(schema));
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Compile();

            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            Assert.Throws<XmlSchemaException>(() => sc.Add(null, Path.Combine(TestData._Root, "include_v2.xsd")));

            // no schema should be addded to the set.
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            Assert.Throws<ArgumentException>("schema", () => sc.Reprocess(schema));
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v103 - Include: A(ns-a) which includes B(ns-a) twice", Priority = 2)]
        [Fact]
        public static void v103()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v8_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v104 - Include: A(ns-a) which includes B(No NS) twice", Priority = 2)]
        [Fact]
        public static void v104()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();


            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v9_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v105 - Include: A,B,C all include each other, all with no ns and refer each others' types", Priority = 2)]
        [Fact]
        public static void v105()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v10_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("c-e2"));
            // Check that B's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("b-e1"));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v106 - Include: A,B,C all include each other, all with same ns and refer each others' types", Priority = 2)]
        [Fact]
        public static void v106()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v11_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            // Check that A's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e1"));

            // Check B's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));

            // Check C's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e3"));
        }

        //[Variation(Desc = "v112 - 20008213 SOM: SourceUri property on a chameleon include is not set", Priority = 1)]
        [Fact]
        public static void v107()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();

            XmlSchema a = ss.Add(null, Path.Combine(TestData._Root, "include_v12_a.xsd"));
            Assert.Equal(1, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Reprocess(a);
            Assert.False(ss.IsCompiled);
            Assert.Equal(1, ss.Count);

            ss.Compile();
            Assert.Equal(1, ss.Count);
            Assert.True(ss.IsCompiled);

            Assert.All(a.Includes.Cast<XmlSchemaExternal>(), schema => Assert.False(string.IsNullOrWhiteSpace(schema.Schema.SourceUri)));
            Assert.Contains(a.Includes.Cast<XmlSchemaExternal>(), schema => schema.Schema.SourceUri.EndsWith("include_v12_b.xsd"));
        }

        /********  compile reprocess include **********/

        [Theory]
        //[Variation(Desc = "v201.6 - Include: A(ns-a) include B(ns-a) which includes C(ns-a) ", Priority = 2, Params = new object[] { "include_v7_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v7_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v201.5 - Include: A with NS includes B and C with no NS", Priority = 2, Params = new object[] { "include_v6_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v6_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v201.4 - Include: A with NS includes B and C with no NS, B also includes C", Priority = 2, Params = new object[] { "include_v5_a.xsd", 1, "ns-a:e3" })]
        [InlineData("include_v5_a.xsd", 1, "ns-a:e3")]
        //[Variation(Desc = "v201.3 - Include: A with NS includes B with no NS, which includes C with no NS", Priority = 2, Params = new object[] { "include_v4_a.xsd", 1, "ns-a:c-e2" })]
        [InlineData("include_v4_a.xsd", 1, "ns-a:c-e2")]
        //[Variation(Desc = "v201.2 - Include: A with no NS includes B with no NS", Priority = 0, Params = new object[] { "include_v3_a.xsd", 1, "e2" })]
        [InlineData("include_v3_a.xsd", 1, "e2")]
        //[Variation(Desc = "v201.1 - Include: A with NS includes B with no NS", Priority = 0, Params = new object[] { "include_v1_a.xsd", 1, "ns-a:e2" })]
        [InlineData("include_v1_a.xsd", 1, "ns-a:e2")]
        public static void v201(object filename1, object count, object expected)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, filename1.ToString())); // param as filename
            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(count, sc.Count);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(count, sc.Count);

            // Check that B's data is present in the NS for A
            Assert.Contains(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals(expected));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v202 - Include: A with NS includes B with a diff NS (INVALID)", Priority = 1)]
        [Fact]
        public static void v202()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema schema = new XmlSchema();
            sc.Add(null, TestData._XsdNoNs);
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();

            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            Assert.Throws<ArgumentException>("schema", () => sc.Reprocess(schema));
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            Assert.Throws<XmlSchemaException>(() => sc.Add(null, Path.Combine(TestData._Root, "include_v2.xsd")));
            // no schema should be addded to the set.
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            Assert.Throws<ArgumentException>("schema", () => sc.Reprocess(schema));
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v203 - Include: A(ns-a) which includes B(ns-a) twice", Priority = 2)]
        [Fact]
        public static void v203()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v8_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v204 - Include: A(ns-a) which includes B(No NS) twice", Priority = 2)]
        [Fact]
        public static void v204()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v9_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v205 - Include: A,B,C all include each other, all with no ns and refer each others' types", Priority = 2)]
        [Fact]
        public static void v205()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v10_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            // Check that C's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("c-e2"));
            // Check that B's data is present in A's NS
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("b-e1"));
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v206 - Include: A,B,C all include each other, all with same ns and refer each others' types", Priority = 2)]
        [Fact]
        public static void v206()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema schema = sc.Add(null, Path.Combine(TestData._Root, "include_v11_a.xsd"));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(schema);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            // Check that A's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e1"));

            // Check B's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e2"));

            // Check C's data
            Assert.Single(schema.Elements.Names.Cast<XmlQualifiedName>(), name => name.ToString().Equals("ns-a:e3"));
        }

        //[Variation(Desc = "v212 - 20008213 SOM: SourceUri property on a chameleon include is not set", Priority = 1)]
        [Fact]
        public static void v207()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();

            XmlSchema a = ss.Add(null, Path.Combine(TestData._Root, "include_v12_a.xsd"));
            Assert.Equal(1, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(1, ss.Count);
            Assert.True(ss.IsCompiled);

            ss.Reprocess(a);
            Assert.False(ss.IsCompiled);
            Assert.Equal(1, ss.Count);

            Assert.All(a.Includes.Cast<XmlSchemaExternal>(), schema => Assert.False(string.IsNullOrWhiteSpace(schema.Schema.SourceUri)));
            Assert.Contains(a.Includes.Cast<XmlSchemaExternal>(), schema => schema.Schema.SourceUri.EndsWith("include_v12_b.xsd"));
        }
    }
}
