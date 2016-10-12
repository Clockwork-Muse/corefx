// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_GlobalTypes", Desc = "")]
    public static class TC_SchemaSet_GlobalTypes
    {
        public static XmlSchema GetSchema(string ns, string type1, string type2)
        {
            string xsd = String.Empty;
            if (ns.Equals(String.Empty))
                xsd = "<schema xmlns='http://www.w3.org/2001/XMLSchema'><complexType name='" + type1 + "'><sequence><element name='local'/></sequence></complexType><simpleType name='" + type2 + "'><restriction base='int'/></simpleType></schema>";
            else
                xsd = "<schema xmlns='http://www.w3.org/2001/XMLSchema' targetNamespace='" + ns + "'><complexType name='" + type1 + "'><sequence><element name='local'/></sequence></complexType><simpleType name='" + type2 + "'><restriction base='int'/></simpleType></schema>";

            XmlSchema schema = XmlSchema.Read(new StringReader(xsd), null);
            return schema;
        }

        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v1 - GlobalTypes on empty collection")]
        [Fact]
        public static void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            XmlSchemaObjectTable table = sc.GlobalTypes;

            Assert.NotNull(table);
        }

        //-----------------------------------------------------------------------------------
        [Theory]
        // params is a pair of the following info: (namaespace, type1 type2) two schemas are made from this info
        //[Variation(Desc = "v2.1 - GlobalTypes with set with two schemas, both without NS", Params = new object[] { "", "t1", "t2", "", "t3", "t4" })]
        [InlineData("", "t1", "t2", "", "t3", "t4")]
        //[Variation(Desc = "v2.2 - GlobalTypes with set with two schemas, one without NS one with NS", Params = new object[] { "a", "t1", "t2", "", "t3", "t4" })]
        [InlineData("a", "t1", "t2", "", "t3", "t4")]
        //[Variation(Desc = "v2.2 - GlobalTypes with set with two schemas, both with NS", Params = new object[] { "a", "t1", "t2", "b", "t3", "t4" })]
        [InlineData("a", "t1", "t2", "b", "t3", "t4")]
        public static void v2(string ns1, string type1, string type2, string ns2, string type3, string type4)
        {
            XmlSchema s1 = GetSchema(ns1, type1, type2);
            XmlSchema s2 = GetSchema(ns2, type3, type4);

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.Add(s1);
            Assert.Equal(0, ss.GlobalTypes.Count);
            ss.Compile();
            ss.Add(s2);
            Assert.Equal(3, ss.GlobalTypes.Count); //+1 for anyType
            ss.Compile();

            //Verify
            Assert.Equal(5, ss.GlobalTypes.Count); //+1 for anyType
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type1, ns1)));
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type2, ns1)));
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type3, ns2)));
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type4, ns2)));

            //Now reprocess one schema and check
            ss.Reprocess(s1);
            Assert.Equal(3, ss.GlobalTypes.Count); //+1 for anyType
            ss.Compile();
            Assert.Equal(5, ss.GlobalTypes.Count); //+1 for anyType

            //Now Remove one schema and check
            ss.Remove(s1);
            Assert.Equal(3, ss.GlobalTypes.Count);
            ss.Compile();
            Assert.Equal(3, ss.GlobalTypes.Count);
        }

        [Theory]
        // params is a pair of the following info: (namaespace, type1 type2)*, doCompile?
        //[Variation(Desc = "v3.1 - GlobalTypes with a set having schema (nons) to another set with schema(nons)", Params = new object[] { "", "t1", "t2", "", "t3", "t4", true })]
        [InlineData("", "t1", "t2", "", "t3", "t4", true)]
        //[Variation(Desc = "v3.2 - GlobalTypes with a set having schema (ns) to another set with schema(nons)", Params = new object[] { "a", "t1", "t2", "", "t3", "t4", true })]
        [InlineData("a", "t1", "t2", "", "t3", "t4", true)]
        //[Variation(Desc = "v3.3 - GlobalTypes with a set having schema (nons) to another set with schema(ns)", Params = new object[] { "", "t1", "t2", "a", "t3", "t4", true })]
        [InlineData("", "t1", "t2", "a", "t3", "t4", true)]
        //[Variation(Desc = "v3.4 - GlobalTypes with a set having schema (ns) to another set with schema(ns)", Params = new object[] { "a", "t1", "t2", "b", "t3", "t4", true })]
        [InlineData("a", "t1", "t2", "b", "t3", "t4", true)]
        //[Variation(Desc = "v3.5 - GlobalTypes with a set having schema (nons) to another set with schema(nons), no compile", Params = new object[] { "", "t1", "t2", "", "t3", "t4", false })]
        [InlineData("", "t1", "t2", "", "t3", "t4", false)]
        //[Variation(Desc = "v3.6 - GlobalTypes with a set having schema (ns) to another set with schema(nons), no compile", Params = new object[] { "a", "t1", "t2", "", "t3", "t4", false })]
        [InlineData("a", "t1", "t2", "", "t3", "t4", false)]
        //[Variation(Desc = "v3.7 - GlobalTypes with a set having schema (nons) to another set with schema(ns), no compile", Params = new object[] { "", "t1", "t2", "a", "t3", "t4", false })]
        [InlineData("", "t1", "t2", "a", "t3", "t4", false)]
        //[Variation(Desc = "v3.8 - GlobalTypes with a set having schema (ns) to another set with schema(ns), no compile", Params = new object[] { "a", "t1", "t2", "b", "t3", "t4", false })]
        [InlineData("a", "t1", "t2", "b", "t3", "t4", false)]
        public static void v3(string ns1, string type1, string type2, string ns2, string type3, string type4, bool doCompile)
        {
            XmlSchema s1 = GetSchema(ns1, type1, type2);
            XmlSchema s2 = GetSchema(ns2, type3, type4);

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
            Assert.Equal(5, ss1.GlobalTypes.Count); //+1 for anyType
            Assert.True(ss1.GlobalTypes.Contains(new XmlQualifiedName(type1, ns1)));
            Assert.True(ss1.GlobalTypes.Contains(new XmlQualifiedName(type2, ns1)));
            Assert.True(ss1.GlobalTypes.Contains(new XmlQualifiedName(type3, ns2)));
            Assert.True(ss1.GlobalTypes.Contains(new XmlQualifiedName(type4, ns2)));

            //Now reprocess one schema and check
            ss1.Reprocess(s1);
            Assert.Equal(3, ss1.GlobalTypes.Count); //+1 for anyType
            ss1.Compile();
            Assert.Equal(5, ss1.GlobalTypes.Count); //+1 for anyType

            //Now Remove one schema and check
            ss1.Remove(s1);
            Assert.Equal(3, ss1.GlobalTypes.Count);
            ss1.Compile();
            Assert.Equal(3, ss1.GlobalTypes.Count);
        }

        //-----------------------------------------------------------------------------------
        [Theory]
        //[Variation(Desc = "v4.1 - GlobalTypes with set having one which imports another, remove one", Priority = 1, Params = new object[] { "import_v1_a.xsd", "ns-a", "ct-A", "", "ct-B" })]
        [InlineData("import_v1_a.xsd", "ns-a", "ct-A", "", "ct-B")]
        //[Variation(Desc = "v4.2 - GlobalTypes with set having one which imports another, remove one", Priority = 1, Params = new object[] { "import_v2_a.xsd", "ns-a", "ct-A", "ns-b", "ct-B" })]
        [InlineData("import_v2_a.xsd", "ns-a", "ct-A", "ns-b", "ct-B")]
        public static void v4(string uri1, string ns1, string type1, string ns2, string type2)
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            XmlSchema schema1 = ss.Add(null, Path.Combine(TestData._Root, uri1));
            ss.Compile();
            Assert.Equal(3, ss.GlobalTypes.Count); //+1 for anyType
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type1, ns1)));
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type2, ns2)));

            //get the SOM for the imported schema
            foreach (XmlSchema s in ss.Schemas(ns2))
            {
                ss.Remove(s);
            }

            ss.Compile();
            Assert.Equal(2, ss.GlobalTypes.Count); //+1 for anyType
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type1, ns1)));
            Assert.False(ss.GlobalTypes.Contains(new XmlQualifiedName(type2, ns2)));
        }

        [Theory]
        //[Variation(Desc = "v5.1 - GlobalTypes with set having one which imports another, then removerecursive", Priority = 1, Params = new object[] { "import_v1_a.xsd", "ns-a", "ct-A", "", "ct-B" })]
        [InlineData("import_v1_a.xsd", "ns-a", "ct-A", "", "ct-B")]
        //[Variation(Desc = "v5.2 - GlobalTypes with set having one which imports another, then removerecursive", Priority = 1, Params = new object[] { "import_v2_a.xsd", "ns-a", "ct-A", "ns-b", "ct-B" })]
        [InlineData("import_v2_a.xsd", "ns-a", "ct-A", "ns-b", "ct-B")]
        public static void v5(string uri1, string ns1, string type1, string ns2, string type2)
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(null, Path.Combine(TestData._Root, "xsdauthor.xsd"));
            XmlSchema schema1 = ss.Add(null, Path.Combine(TestData._Root, uri1));
            ss.Compile();
            Assert.Equal(3, ss.GlobalTypes.Count); //+1 for anyType
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type1, ns1)));
            Assert.True(ss.GlobalTypes.Contains(new XmlQualifiedName(type2, ns2)));

            ss.RemoveRecursive(schema1); // should not need to compile for RemoveRecursive to take effect
            Assert.Equal(1, ss.GlobalTypes.Count); //+1 for anyType
            Assert.False(ss.GlobalTypes.Contains(new XmlQualifiedName(type1, ns1)));
            Assert.False(ss.GlobalTypes.Contains(new XmlQualifiedName(type2, ns2)));
        }

        //-----------------------------------------------------------------------------------
        //REGRESSIONS
        //[Variation(Desc = "v100 - XmlSchemaSet: Components are not added to the global tabels if it is already compiled", Priority = 1)]
        [Fact]
        public static void v100()
        {
            // anytype t1 t2
            XmlSchema schema1 = XmlSchema.Read(new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' targetNamespace='a'><xs:element name='e1' type='xs:anyType'/><xs:complexType name='t1'/><xs:complexType name='t2'/></xs:schema>"), null);
            // anytype t3 t4
            XmlSchema schema2 = XmlSchema.Read(new StringReader("<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' ><xs:element name='e1' type='xs:anyType'/><xs:complexType name='t3'/><xs:complexType name='t4'/></xs:schema>"), null);
            XmlSchemaSet ss1 = new XmlSchemaSet();
            XmlSchemaSet ss2 = new XmlSchemaSet();

            ss1.Add(schema1);
            ss2.Add(schema2);
            ss2.Compile();
            ss1.Add(ss2);
            ss1.Compile();
            Assert.Equal(5, ss1.GlobalTypes.Count);
            Assert.True(ss1.GlobalTypes.Contains(new XmlQualifiedName("t1", "a")));
            Assert.True(ss1.GlobalTypes.Contains(new XmlQualifiedName("t2", "a")));
        }
    }
}

//todo: add sanity test for include
//todo: copy count checks from element
