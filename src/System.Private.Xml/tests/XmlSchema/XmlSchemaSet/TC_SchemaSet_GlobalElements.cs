// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_GlobalElements", Desc = "")]
    public static class TC_SchemaSet_GlobalElements
    {
        public static XmlSchema GetSchema(string ns, string e1, string e2)
        {
            string xsd = String.Empty;
            if (ns.Equals(String.Empty))
                xsd = "<schema xmlns='http://www.w3.org/2001/XMLSchema'><element name='" + e1 + "'/><element name='" + e2 + "'/></schema>";
            else
                xsd = "<schema xmlns='http://www.w3.org/2001/XMLSchema' targetNamespace='" + ns + "'><element name='" + e1 + "'/><element name='" + e2 + "'/></schema>";

            XmlSchema schema = XmlSchema.Read(new StringReader(xsd), null);
            return schema;
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v1 - GlobalElements on empty collection", Priority = 0)]
        [Fact]
        public static void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaObjectTable table = sc.GlobalElements;

            Assert.NotNull(table);
        }

        [Theory]
        // params is a pair of the following info: (namaespace, e2 e2) two schemas are made from this info
        //[Variation(Desc = "v2.1 - GlobalElements with set with two schemas, both without NS", Params = new object[] { "", "e1", "e2", "", "e3", "e4" })]
        [InlineData("", "e1", "e2", "", "e3", "e4")]
        //[Variation(Desc = "v2.2 - GlobalElements with set with two schemas, one without NS one with NS", Params = new object[] { "a", "e1", "e2", "", "e3", "e4" })]
        [InlineData("a", "e1", "e2", "", "e3", "e4")]
        //[Variation(Desc = "v2.2 - GlobalElements with set with two schemas, both with NS", Params = new object[] { "a", "e1", "e2", "b", "e3", "e4" })]
        [InlineData("a", "e1", "e2", "b", "e3", "e4")]
        {
            XmlSchema s1 = GetSchema(ns1, e1, e2);
            XmlSchema s2 = GetSchema(ns2, e3, e4);

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(s1);
            ss.Compile();
            ss.Add(s2);
            Assert.Equal(2, ss.GlobalElements.Count); //+1 for anyType
            ss.Compile();

            //Verify
            Assert.Equal(4, ss.GlobalElements.Count);
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e1, ns1)));
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e2, ns1)));
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e3, ns2)));
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e4, ns2)));

            //Now reprocess one schema and check
            ss.Reprocess(s1);
            ss.Compile();
            Assert.Equal(4, ss.GlobalElements.Count);

            //Now Remove one schema and check
            ss.Remove(s1);
            Assert.Equal(2, ss.GlobalElements.Count);
            ss.Compile();
            Assert.Equal(2, ss.GlobalElements.Count);
        }

        [Theory]
        // params is a pair of the following info: (namaespace, e1 e2)*, doCompile?
        //[Variation(Desc = "v3.1 - GlobalElements with a set having schema (nons) to another set with schema(nons)", Params = new object[] { "", "e1", "e2", "", "e3", "e4", true })]
        [InlineData("", "e1", "e2", "", "e3", "e4", true)]
        //[Variation(Desc = "v3.2 - GlobalElements with a set having schema (ns) to another set with schema(nons)", Params = new object[] { "a", "e1", "e2", "", "e3", "e4", true })]
        [InlineData("a", "e1", "e2", "", "e3", "e4", true)]
        //[Variation(Desc = "v3.3 - GlobalElements with a set having schema (nons) to another set with schema(ns)", Params = new object[] { "", "e1", "e2", "a", "e3", "e4", true })]
        [InlineData("", "e1", "e2", "a", "e3", "e4", true)]
        //[Variation(Desc = "v3.4 - GlobalElements with a set having schema (ns) to another set with schema(ns)", Params = new object[] { "a", "e1", "e2", "b", "e3", "e4", true })]
        [InlineData("a", "e1", "e2", "b", "e3", "e4", true)]
        //[Variation(Desc = "v3.5 - GlobalElements with a set having schema (nons) to another set with schema(nons), no compile", Params = new object[] { "", "e1", "e2", "", "e3", "e4", false })]
        [InlineData("", "e1", "e2", "", "e3", "e4", false)]
        //[Variation(Desc = "v3.6 - GlobalElements with a set having schema (ns) to another set with schema(nons), no compile", Params = new object[] { "a", "e1", "e2", "", "e3", "e4", false })]
        [InlineData("a", "e1", "e2", "", "e3", "e4", false)]
        //[Variation(Desc = "v3.7 - GlobalElements with a set having schema (nons) to another set with schema(ns), no compile", Params = new object[] { "", "e1", "e2", "a", "e3", "e4", false })]
        [InlineData("", "e1", "e2", "a", "e3", "e4", false)]
        //[Variation(Desc = "v3.8 - GlobalElements with a set having schema (ns) to another set with schema(ns), no compile", Params = new object[] { "a", "e1", "e2", "b", "e3", "e4", false })]
        [InlineData("a", "e1", "e2", "b", "e3", "e4", false)]
        {
            XmlSchema s1 = GetSchema(ns1, e1, e2);
            XmlSchema s2 = GetSchema(ns2, e3, e4);

            XmlSchemaSet ss1 = new XmlSchemaSet();
            XmlSchemaSet ss2 = new XmlSchemaSet();
            ss1.Add(s1);
            ss1.Compile();

            ss2.Add(s2);

            if (doCompile)
                ss2.Compile();

            // add one schemaset to another
            ss1.Add(ss2);

            if (!doCompile)
                ss1.Compile();
            //Verify
            Assert.Equal(4, ss1.GlobalElements.Count);
            Assert.True(ss1.GlobalElements.Contains(new XmlQualifiedName(e1, ns1)));
            Assert.True(ss1.GlobalElements.Contains(new XmlQualifiedName(e2, ns1)));
            Assert.True(ss1.GlobalElements.Contains(new XmlQualifiedName(e3, ns2)));
            Assert.True(ss1.GlobalElements.Contains(new XmlQualifiedName(e4, ns2)));

            //Now reprocess one schema and check
            ss1.Reprocess(s1);
            ss1.Compile();
            Assert.Equal(4, ss1.GlobalElements.Count);

            //Now Remove one schema and check
            ss1.Remove(s1);
            Assert.Equal(2, ss1.GlobalElements.Count); // count should still be 4
            ss1.Compile();
            Assert.Equal(2, ss1.GlobalElements.Count); // count should NOW still be 2
        }

        //-----------------------------------------------------------------------------------
        [Theory]
        //[Variation(Desc = "v4.1 - GlobalElements with set having one which imports another, remove one", Priority = 1, Params = new object[] { "import_v1_a.xsd", "ns-a", "e1", "", "e2" })]
        [InlineData("import_v1_a.xsd", "ns-a", "e1", "", "e2")]
        //[Variation(Desc = "v4.2 - GlobalElements with set having one which imports another, remove one", Priority = 1, Params = new object[] { "import_v2_a.xsd", "ns-a", "e1", "ns-b", "e2" })]
        [InlineData("import_v2_a.xsd", "ns-a", "e1", "ns-b", "e2")]
        public static void v4(string uri1, string ns1, string e1, string ns2, string e2)
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            XmlSchema schema1 = ss.Add(null, Path.Combine(TestData._Root, uri1));
            ss.Compile();
            Assert.Equal(3, ss.GlobalElements.Count); // +1 for root in ns-a
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e1, ns1)));
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e2, ns2)));

            //get the SOM for the imported schema
            foreach (XmlSchema s in ss.Schemas(ns2))
            {
                ss.Remove(s);
            }

            ss.Compile();
            Assert.Equal(2, ss.GlobalElements.Count);
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e1, ns1)));
            Assert.False(ss.GlobalElements.Contains(new XmlQualifiedName(e2, ns2)));
        }

        [Theory]
        //[Variation(Desc = "v5.1 - GlobalElements with set having one which imports another, then removerecursive", Priority = 1, Params = new object[] { "import_v1_a.xsd", "ns-a", "e1", "", "e2" })]
        [InlineData("import_v1_a.xsd", "ns-a", "e1", "", "e2")]
        //[Variation(Desc = "v5.2 - GlobalElements with set having one which imports another, then removerecursive", Priority = 1, Params = new object[] { "import_v2_a.xsd", "ns-a", "e1", "ns-b", "e2" })]
        [InlineData("import_v2_a.xsd", "ns-a", "e1", "ns-b", "e2")]
        public static void v5(string uri1, string ns1, string e1, string ns2, string e2)
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(null, Path.Combine(TestData._Root, "xsdauthor.xsd"));
            XmlSchema schema1 = ss.Add(null, Path.Combine(TestData._Root, uri1));
            ss.Compile();
            Assert.Equal(4, ss.GlobalElements.Count);  // +1 for root in ns-a and xsdauthor
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e1, ns1)));
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e2, ns2)));

            ss.RemoveRecursive(schema1); // should not need to compile for RemoveRecursive to take effect
            Assert.Equal(1, ss.GlobalElements.Count);
            Assert.False(ss.GlobalElements.Contains(new XmlQualifiedName(e1, ns1)));
            Assert.False(ss.GlobalElements.Contains(new XmlQualifiedName(e2, ns2)));
        }

        [Theory]
        //[Variation(Desc = "v6 - GlobalElements with set with two schemas, second schema will fail to compile, no elements from it should be added", Params = new object[] { "", "e1", "e2" })]
        [InlineData("", "e1", "e2")]
        public static void v6(string ns1, string e1, string e2)
        {
            XmlSchema s1 = GetSchema(ns1, e1, e2);
            XmlSchema s2 = XmlSchema.Read(new StreamReader(new FileStream(Path.Combine(TestData._Root, "invalid.xsd"), FileMode.Open, FileAccess.Read)), null);

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(s1);
            ss.Compile();
            ss.Add(s2);
            Assert.Equal(2, ss.GlobalElements.Count); //+1 for anyType

            Assert.Throws<XmlSchemaException>(() => ss.Compile());
            Assert.Equal(2, ss.GlobalElements.Count);
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e1, ns1)));
            Assert.True(ss.GlobalElements.Contains(new XmlQualifiedName(e2, ns1)));
        }
    }
}
