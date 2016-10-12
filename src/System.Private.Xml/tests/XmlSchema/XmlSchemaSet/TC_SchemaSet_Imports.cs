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
    //[TestCase(Name = "TC_SchemaSet_Imports", Desc = "")]
    public static class TC_SchemaSet_Imports
    {
        //-----------------------------------------------------------------------------------
        [Theory]
        //[Variation(Desc = "v1.3 - Import: A(ns-a) which improts B (no ns)", Priority = 0, Params = new object[] { "import_v4_a.xsd", "import_v4_b.xsd", 2, null })]
        [InlineData("import_v4_a.xsd", "import_v4_b.xsd", 2, null)]
        //[Variation(Desc = "v1.2 - Import: A(ns-a) improts B (ns-b)", Priority = 0, Params = new object[] { "import_v2_a.xsd", "import_v2_b.xsd", 2, "ns-b" })]
        [InlineData("import_v2_a.xsd", "import_v2_b.xsd", 2, "ns-b")]
        //[Variation(Desc = "v1.1 - Import: A with NS imports B with no NS", Priority = 0, Params = new object[] { "import_v1_a.xsd", "include_v1_b.xsd", 2, null })]
        [InlineData("import_v1_a.xsd", "include_v1_b.xsd", 2, null)]
        public static void v1(string filename1, string filename2, int count, string targetNamespace)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, filename1));
            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);
            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(count, sc.Count);
            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
           {
               Assert.Equal(filename2, imp.SchemaLocation);
               Assert.Equal(targetNamespace, imp.Schema.TargetNamespace);
           });
        }

        //-----------------------------------------------------------------------------------
        [Theory]
        //[Variation(Desc = "v2.2 - Import: Add B(no ns) with ns-b , then A(ns-a) which imports B (no ns)", Priority = 1, Params = new object[] { "import_v5_a.xsd", "import_v4_b.xsd", 3, "ns-b", null })]
        [InlineData("import_v5_a.xsd", "import_v4_b.xsd", 3, "ns-b", null)]
        //[Variation(Desc = "v2.1 - Import: Add B(ns-b) , then A(ns-a) which improts B (ns-b)", Priority = 1, Params = new object[] { "import_v2_a.xsd", "import_v2_b.xsd", 2, null, "ns-b" })]
        [InlineData("import_v2_a.xsd", "import_v2_b.xsd", 2, null, "ns-b")]
        public static void v3(string namespace1, string namespace2, int count, string targetNamespace1, string targetNamespace2)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            sc.Add(targetNamespace1, Path.Combine(TestData._Root, namespace2));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, namespace1));

            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal(namespace2, imp.SchemaLocation);
                Assert.Equal(targetNamespace2, imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v3 - Import: Add A(ns-a) which imports B (no ns), then Add B(no ns) again", Priority = 1)]
        public static void v6()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v5_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
            XmlSchema orig = sc.Add(null, Path.Combine(TestData._Root, "import_v4_b.xsd")); // should be already present in the set

            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
            Assert.True(orig.SourceUri.Contains("import_v4_b.xsd"));

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v4_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v4 - Import: Add A(ns-a) which improts B (no ns), then Add B to ns-b", Priority = 1)]
        public static void v7()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v5_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Add("ns-b", Path.Combine(TestData._Root, "import_v4_b.xsd")); // should be already present in the set

            Assert.Equal(3, sc.Count);
            Assert.False(sc.IsCompiled);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v4_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v5 - Import: Add A(ns-a) which improts B (ns-b), then Add B(ns-b) again", Priority = 1)]
        public static void v8()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v2_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
            sc.Add("ns-b", Path.Combine(TestData._Root, "import_v2_b.xsd")); // should be already present in the set
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v2_b.xsd", imp.SchemaLocation);
                Assert.Equal("ns-b", imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v6 - Import: A(ns-a) imports B(ns-b) imports C (ns-c)", Priority = 1)]
        public static void v9()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v9_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            XmlSchema sch_B = sc.Add(null, Path.Combine(TestData._Root, "import_v9_b.xsd")); // should be already present in the set
            sc.Add(null, Path.Combine(TestData._Root, "import_v9_c.xsd"));				   // should be already present in the set

            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v9_b.xsd", imp.SchemaLocation);
                Assert.Equal("ns-b", imp.Schema.TargetNamespace);
            });
            // check that schema C in sch_b.Includes and its NS correct.
            Assert.All(sch_B.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v9_c.xsd", imp.SchemaLocation);
                Assert.Equal("ns-c", imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v7 - Import: A(ns-a) imports B(NO NS) imports C (ns-c)", Priority = 1)]
        public static void v10()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v10_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);
            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            XmlSchema sch_B = sc.Add(null, Path.Combine(TestData._Root, "import_v10_b.xsd")); // should be already present in the set
            sc.Add(null, Path.Combine(TestData._Root, "import_v10_c.xsd"));				   // should be already present in the set

            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v10_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });
            Assert.All(sch_B.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v10_c.xsd", imp.SchemaLocation);
                Assert.Equal("ns-c", imp.Schema.TargetNamespace);
            });

            // try adding no ns schema with an ns
            sc.Add("ns-b", Path.Combine(TestData._Root, "import_v10_b.xsd"));
            Assert.Equal(4, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v8 - Import: A(ns-a) imports B(ns-b) imports C (ns-a)", Priority = 1)]
        public static void v11()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v11_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-a").Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v9 - Import: A(ns-a) imports B(ns-b) and C (ns-b)", Priority = 1)]
        public static void v12()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v12_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-b").Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v10 - Import: A imports B and B and C, B imports C and D, C imports D and A", Priority = 1)]
        public static void v13()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v13_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Compile();
            Assert.Equal(4, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v11 - Import: A(ns-a) imports B(ns-b) and C (ns-b), B and C include each other", Priority = 2)]
        public static void v14()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v14_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-b").Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v12 - Import: A(ns-a) imports B(BOGUS) and C (ns-c)", Priority = 2)]
        public static void v15()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v15_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v13 - Import: B(ns-b) added, A(ns-a) imports B with bogus url", Priority = 2)]
        public static void v16()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add(null, Path.Combine(TestData._Root, "import_v16_b.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v16_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v14 - Import: A(ns-a) includes B(ns-a) which imports C(ns-c)", Priority = 2)]
        public static void v17()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v17_a.xsd"));

            Assert.Equal(2, sc.Count);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);
            Assert.Equal(2, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v15 - Import: A(ns-a) includes A(ns-a) of v17", Priority = 2)]
        public static void v18()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v18_a.xsd"));

            Assert.Equal(2, sc.Count);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);
            Assert.Equal(2, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v16 - Import: A(ns-b) imports A(ns-a) of v17", Priority = 2)]
        public static void v19()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v19_a.xsd"));
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(2, sc.Schemas("ns-c").Count);
            Assert.Equal(3, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v17 - Import: A,B,C,D all import and reference each other for types", Priority = 2)]
        public static void v20()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v20_a.xsd"));
            Assert.Equal(4, sc.Count);

            sc.Compile();
            // try to add each individually
            XmlSchema b = sc.Add(null, Path.Combine(TestData._Root, "import_v20_b.xsd"));
            XmlSchema c = sc.Add(null, Path.Combine(TestData._Root, "import_v20_c.xsd"));
            XmlSchema d = sc.Add(null, Path.Combine(TestData._Root, "import_v20_d.xsd"));

            Assert.Equal(4, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.True(b.SourceUri.Contains("import_v20_b.xsd"));
            Assert.True(c.SourceUri.Contains("import_v20_c.xsd"));
            Assert.True(d.SourceUri.Contains("import_v20_d.xsd"));
        }

        [Theory]
        //[Variation(Desc = "v21- Import: Bug 114549 , A imports only B but refers to C and D both", Priority = 1, Params = new object[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v21_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v21_a.xsd" } })]
        //[Variation(Desc = "v22- Import: Bug 114549 , A imports only B's NS, but refers to B,C and D both", Priority = 1, Params = new object[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" } })]
        //[Variation(Desc = "v23- Import: Bug 114549 , A imports only B's NS, and B also improts A's NS AND refers to A's types", Priority = 1, Params = new object[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" } })]
        public static void v21(string[] filenames)
        {
            XmlSchemaSet ss = new XmlSchemaSet();

            foreach (string fn in filenames)
            {
                ss.Add(null, Path.Combine(TestData._Root, fn));
            }
            Assert.Equal(filenames.Length, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(filenames.Length, ss.Count);
            Assert.True(ss.IsCompiled);
        }

        [Theory]
        //[Variation(Desc = "v24- Import: Bug 114549 , A imports only B's NS, and B also refers to A's types (WARNING)", Priority = 1, Params = new object[] { "import_v23_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData("import_v23_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd")]
        //[Variation(Desc = "v25- Import: Bug 114549 , A imports only B's NS, and B also improts A's NS AND refers to A's type, D refers to A's type (WARNING)", Priority = 1, Params = new object[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v25_d.xsd", "import_v22_a.xsd" })]
        [InlineData("import_v24_b.xsd", "import_v21_c.xsd", "import_v25_d.xsd", "import_v22_a.xsd")]
        public static void v24(object param0, object param1, object param2, object param3)
        {
            XmlSchemaSet ss = new XmlSchemaSet();

            ss.Add(null, Path.Combine(TestData._Root, param0.ToString()));
            ss.Add(null, Path.Combine(TestData._Root, param1.ToString()));
            ss.Add(null, Path.Combine(TestData._Root, param2.ToString()));
            ss.Add(null, Path.Combine(TestData._Root, param3.ToString()));
            Assert.Equal(4, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(4, ss.Count);
            Assert.True(ss.IsCompiled);
        }

        [Fact]
        //[Variation(Desc = "v100 - Import: Bug 105897", Priority = 1)]
        public static void v100()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(null, Path.Combine(TestData._Root, "105897.xsd"));
            ss.Add(null, Path.Combine(TestData._Root, "105897_a.xsd"));
            Assert.Equal(3, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(3, ss.Count);
            Assert.True(ss.IsCompiled);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings |
                                       XmlSchemaValidationFlags.ProcessSchemaLocation |
                                       XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.Schemas = new XmlSchemaSet();
            settings.Schemas.Add(ss);

            using (XmlReader vr = XmlReader.Create(Path.Combine(TestData._Root, "105897.xml"), settings))
            {
                while (vr.Read()) ;
            }
        }

        /********* reprocess compile import**************/

        //[Variation(Desc = "v101.3 - Import: A(ns-a) which improts B (no ns)", Priority = 0, Params = new object[] { "import_v4_a.xsd", "import_v4_b.xsd", 2, null })]
        [InlineData("import_v4_a.xsd", "import_v4_b.xsd", 2, null)]
        //[Variation(Desc = "v101.2 - Import: A(ns-a) improts B (ns-b)", Priority = 0, Params = new object[] { "import_v2_a.xsd", "import_v2_b.xsd", 2, "ns-b" })]
        [InlineData("import_v2_a.xsd", "import_v2_b.xsd", 2, "ns-b")]
        //[Variation(Desc = "v101.1 - Improt: A with NS imports B with no NS", Priority = 0, Params = new object[] { "import_v1_a.xsd", "include_v1_b.xsd", 2, null })]
        [InlineData("import_v1_a.xsd", "include_v1_b.xsd", 2, null)]
        [Theory]
        public static void v101(string filename1, string schemaLocation, int count, string targetNamespace)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, filename1));
            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(count, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(count, sc.Count);
            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal(schemaLocation, imp.SchemaLocation);
                Assert.Equal(targetNamespace, imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------

        //[Variation(Desc = "v102.1 - Import: Add B(ns-b) , then A(ns-a) which improts B (ns-b)", Priority = 1, Params = new object[] { "import_v2_a.xsd", "import_v2_b.xsd", 2, null, "ns-b" })]
        [Theory]
        [InlineData("import_v2_a.xsd", "import_v2_b.xsd", 2, null, "ns-b")]
        public static void v102(string filename1, string schemaLocation, int count, string target, string targetNamespace)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema sch = sc.Add(target, Path.Combine(TestData._Root, schemaLocation));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(sch);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, filename1));
            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal(schemaLocation, imp.SchemaLocation);
                Assert.Equal(targetNamespace, imp.Schema.TargetNamespace);
            });
        }

        //[Variation(Desc = "v102.2 - Import: Add B(no ns) with ns-b , then A(ns-a) which imports B (no ns)", Priority = 1, Params = new object[] { "import_v5_a.xsd", "import_v4_b.xsd", 3, "ns-b", null })]
        [Theory]
        [InlineData("import_v5_a.xsd", "import_v4_b.xsd", "ns-b")]
        public static void v102a(string filename1, string filename2, object targetNamespace)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema sch = sc.Add((string)targetNamespace, Path.Combine(TestData._Root, filename2));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(sch);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            Assert.Throws<XmlSchemaException>(() => sc.Add(null, Path.Combine(TestData._Root, filename1)));

            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v103 - Import: Add A(ns-a) which imports B (no ns), then Add B(no ns) again", Priority = 1)]
        public static void v103()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v5_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            XmlSchema orig = sc.Add(null, Path.Combine(TestData._Root, "import_v4_b.xsd")); // should be already present in the set
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
            Assert.True(orig.SourceUri.Contains("import_v4_b.xsd"));

            sc.Reprocess(orig);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v4_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v104 - Import: Add A(ns-a) which improts B (no ns), then Add B to ns-b", Priority = 1)]
        public static void v104()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v5_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            XmlSchema sch_B = sc.Add("ns-b", Path.Combine(TestData._Root, "import_v4_b.xsd")); // should be already present in the set
            Assert.Equal(3, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v4_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v105 - Import: Add A(ns-a) which improts B (ns-b), then Add B(ns-b) again", Priority = 1)]
        public static void v105()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v2_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            XmlSchema sch_B = sc.Add("ns-b", Path.Combine(TestData._Root, "import_v2_b.xsd")); // should be already present in the set
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v2_b.xsd", imp.SchemaLocation);
                Assert.Equal("ns-b", imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v106 - Import: A(ns-a) imports B(ns-b) imports C (ns-c)", Priority = 1)]
        public static void v106()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v9_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            XmlSchema sch_B = sc.Add(null, Path.Combine(TestData._Root, "import_v9_b.xsd")); // should be already present in the set
            sc.Add(null, Path.Combine(TestData._Root, "import_v9_c.xsd"));                  // should be already present in the set
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v9_b.xsd", imp.SchemaLocation);
                Assert.Equal("ns-b", imp.Schema.TargetNamespace);
            });
            // check that schema C in sch_b.Includes and its NS correct.
            Assert.All(sch_B.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v9_c.xsd", imp.SchemaLocation);
                Assert.Equal("ns-c", imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v107 - Import: A(ns-a) imports B(NO NS) imports C (ns-c)", Priority = 1)]
        public static void v107()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v10_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            XmlSchema sch_B = sc.Add(null, Path.Combine(TestData._Root, "import_v10_b.xsd")); // should be already present in the set
            sc.Add(null, Path.Combine(TestData._Root, "import_v10_c.xsd"));                 // should be already present in the set
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v10_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });
            // check that schema C in sch_b.Includes and its NS correct.
            Assert.All(sch_B.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v10_c.xsd", imp.SchemaLocation);
                Assert.Equal("ns-c", imp.Schema.TargetNamespace);
            });

            // try adding no ns schema with an ns
            sch_B = sc.Add("ns-b", Path.Combine(TestData._Root, "import_v10_b.xsd"));
            Assert.Equal(4, sc.Count);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Compile();
            Assert.Equal(4, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v108 - Import: A(ns-a) imports B(ns-b) imports C (ns-a)", Priority = 1)]
        public static void v108()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v11_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-a").Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v109 - Import: A(ns-a) imports B(ns-b) and C (ns-b)", Priority = 1)]
        public static void v109()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v12_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-b").Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v110 - Import: A imports B and B and C, B imports C and D, C imports D and A", Priority = 1)]
        public static void v110()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v13_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Compile();
            Assert.Equal(4, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v111 - Import: A(ns-a) imports B(ns-b) and C (ns-b), B and C include each other", Priority = 2)]
        public static void v111()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v14_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-b").Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v112 - Import: A(ns-a) imports B(BOGUS) and C (ns-c)", Priority = 2)]
        public static void v112()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v15_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v113 - Import: B(ns-b) added, A(ns-a) imports B with bogus url", Priority = 2)]
        public static void v113()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Add(null, Path.Combine(TestData._Root, "import_v16_b.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v16_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v114 - Import: A(ns-a) includes B(ns-a) which imports C(ns-c)", Priority = 2)]
        public static void v114()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v17_a.xsd"));

            Assert.Equal(2, sc.Count);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);
            Assert.Equal(2, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v115 - Import: A(ns-a) includes A(ns-a) of v17", Priority = 2)]
        public static void v115()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v18_a.xsd"));

            Assert.Equal(2, sc.Count);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);
            Assert.Equal(2, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v116 - Import: A(ns-b) imports A(ns-a) of v17", Priority = 2)]
        public static void v116()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v19_a.xsd"));
            Assert.Equal(3, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(2, sc.Schemas("ns-c").Count);
            Assert.Equal(3, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v117 - Import: A,B,C,D all import and reference each other for types", Priority = 2)]
        public static void v117()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v20_a.xsd"));
            Assert.Equal(4, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            XmlSchema b = sc.Add(null, Path.Combine(TestData._Root, "import_v20_b.xsd"));
            XmlSchema c = sc.Add(null, Path.Combine(TestData._Root, "import_v20_c.xsd"));
            XmlSchema d = sc.Add(null, Path.Combine(TestData._Root, "import_v20_d.xsd"));

            Assert.Equal(4, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.True(b.SourceUri.Contains("import_v20_b.xsd"));
            Assert.True(c.SourceUri.Contains("import_v20_c.xsd"));
            Assert.True(d.SourceUri.Contains("import_v20_d.xsd"));

            sc.Reprocess(b);
            Assert.Equal(4, sc.Count);
            sc.Reprocess(c);
            Assert.Equal(4, sc.Count);
            sc.Reprocess(d);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(4, sc.Count);
        }

        [Theory]
        //[Variation(Desc = "v121- Import: Bug 114549 , A imports only B but refers to C and D both", Priority = 1, Params = new object[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v21_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v21_a.xsd" } })]
        //[Variation(Desc = "v122- Import: Bug 114549 , A imports only B's NS, but refers to B,C and D both", Priority = 1, Params = new object[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" } })]
        //[Variation(Desc = "v123- Import: Bug 114549 , A imports only B's NS, and B also improts A's NS AND refers to A's types", Priority = 1, Params = new object[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" } })]
        public static void v118(string[] filenames)
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            XmlSchema[] schemas = filenames.Select(fn => ss.Add(null, Path.Combine(TestData._Root, fn))).ToArray();

            Assert.Equal(4, ss.Count);
            Assert.False(ss.IsCompiled);

            foreach (XmlSchema schema in schemas)
            {
                ss.Reprocess(schema);
                Assert.Equal(4, ss.Count);
            }
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(4, ss.Count);
            Assert.True(ss.IsCompiled);
        }

        [Theory]
        //[Variation(Desc = "v124- Import: Bug 114549 , A imports only B's NS, and B also refers to A's types (WARNING)", Priority = 1, Params = new object[] { "import_v23_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v23_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" } })]
        //[Variation(Desc = "v125- Import: Bug 114549 , A imports only B's NS, and B also improts A's NS AND refers to A's type, D refers to A's type (WARNING)", Priority = 1, Params = new object[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v25_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v25_d.xsd", "import_v22_a.xsd" } })]
        public static void v119(string[] filenames)
        {
            XmlSchemaSet ss = new XmlSchemaSet();

            XmlSchema[] schemas = filenames.Select(fn => ss.Add(null, Path.Combine(TestData._Root, fn))).ToArray();

            Assert.Equal(4, ss.Count);
            Assert.False(ss.IsCompiled);

            foreach (XmlSchema schema in schemas)
            {
                ss.Reprocess(schema);
                Assert.Equal(4, ss.Count);
            }

            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(4, ss.Count);
            Assert.True(ss.IsCompiled);
        }

        [Fact]
        //[Variation(Desc = "v120 - Import: Bug 105897", Priority = 1)]
        public static void v120()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            XmlSchema Schema1 = ss.Add(null, Path.Combine(TestData._Root, "105897.xsd"));
            XmlSchema Schema2 = ss.Add(null, Path.Combine(TestData._Root, "105897_a.xsd"));
            Assert.Equal(3, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Reprocess(Schema1);
            Assert.Equal(3, ss.Count);
            ss.Reprocess(Schema2);
            Assert.False(ss.IsCompiled);
            Assert.Equal(3, ss.Count);

            ss.Compile();
            Assert.Equal(3, ss.Count);
            Assert.True(ss.IsCompiled);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings |
                                       XmlSchemaValidationFlags.ProcessSchemaLocation |
                                       XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.Schemas = new XmlSchemaSet();
            settings.Schemas.Add(ss);

            using (XmlReader vr = XmlReader.Create(Path.Combine(TestData._Root, "105897.xml"), settings))
            {
                while (vr.Read()) ;
            }
        }

        /*********compile reprocess import**************/

        [Theory]
        //[Variation(Desc = "v201.3 - Import: A(ns-a) which improts B (no ns)", Priority = 0, Params = new object[] { "import_v4_a.xsd", "import_v4_b.xsd", 2, null })]
        [InlineData("import_v4_a.xsd", "import_v4_b.xsd", 2, null)]
        //[Variation(Desc = "v201.2 - Import: A(ns-a) improts B (ns-b)", Priority = 0, Params = new object[] { "import_v2_a.xsd", "import_v2_b.xsd", 2, "ns-b" })]
        [InlineData("import_v2_a.xsd", "import_v2_b.xsd", 2, "ns-b")]
        //[Variation(Desc = "v201.1 - Improt: A with NS imports B with no NS", Priority = 0, Params = new object[] { "import_v1_a.xsd", "include_v1_b.xsd", 2, null })]
        [InlineData("import_v1_a.xsd", "include_v1_b.xsd", 2, null)]
        public static void v201(string filename1, string schemaLocation, int count, string targetNamespace)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, filename1));
            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(count, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(count, sc.Count);
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal(schemaLocation, imp.SchemaLocation);
                Assert.Equal(targetNamespace, imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------

        //[Variation(Desc = "v202.1 - Import: Add B(ns-b) , then A(ns-a) which improts B (ns-b)", Priority = 1, Params = new object[] { "import_v2_a.xsd", "import_v2_b.xsd", 2, null, "ns-b" })]
        [Theory]
        [InlineData("import_v2_a.xsd", "import_v2_b.xsd", 2, null, "ns-b")]
        public static void v202(string filename1, string schemaLocation, int count, string target, string targetNamespace)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema sch = sc.Add(target, Path.Combine(TestData._Root, schemaLocation));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, filename1));
            Assert.Equal(count, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal(schemaLocation, imp.SchemaLocation);
                Assert.Equal(targetNamespace, imp.Schema.TargetNamespace);
            });
        }

        //[Variation(Desc = "v202.2 - Import: Add B(no ns) with ns-b , then A(ns-a) which imports B (no ns)", Priority = 1, Params = new object[] { "import_v5_a.xsd", "import_v4_b.xsd", 3, "ns-b", null })]
        [Theory]
        [InlineData("import_v5_a.xsd", "import_v4_b.xsd", "ns-b")]
        public static void v202a(string filename1, string filename2, string target)
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema sch = sc.Add((string)target, Path.Combine(TestData._Root, filename2.ToString()));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(1, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            Assert.Throws<XmlSchemaException>(() => sc.Add(null, TestData._Root + filename1.ToString()));
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            Assert.Throws<XmlSchemaException>(() => sc.Compile());
            Assert.Equal(1, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Reprocess(sch);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v203 - Import: Add A(ns-a) which imports B (no ns), then Add B(no ns) again", Priority = 1)]
        public static void v203()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v5_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            XmlSchema orig = sc.Add(null, Path.Combine(TestData._Root, "import_v4_b.xsd")); // should be already present in the set
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
            Assert.True(orig.SourceUri.Contains("import_v4_b.xsd"));

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(orig);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v4_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v204 - Import: Add A(ns-a) which improts B (no ns), then Add B to ns-b", Priority = 1)]
        public static void v204()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v5_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            XmlSchema sch_B = sc.Add("ns-b", Path.Combine(TestData._Root, "import_v4_b.xsd")); // should be already present in the set
            Assert.Equal(3, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v4_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v205 - Import: Add A(ns-a) which improts B (ns-b), then Add B(ns-b) again", Priority = 1)]
        public static void v205()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v2_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            XmlSchema sch_B = sc.Add("ns-b", Path.Combine(TestData._Root, "import_v2_b.xsd")); // should be already present in the set
            Assert.Equal(2, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v2_b.xsd", imp.SchemaLocation);
                Assert.Equal("ns-b", imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v206 - Import: A(ns-a) imports B(ns-b) imports C (ns-c)", Priority = 1)]
        public static void v206()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v9_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            XmlSchema sch_B = sc.Add(null, Path.Combine(TestData._Root, "import_v9_b.xsd")); // should be already present in the set
            sc.Add(null, Path.Combine(TestData._Root, "import_v9_c.xsd"));                  // should be already present in the set
            Assert.Equal(3, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v9_b.xsd", imp.SchemaLocation);
                Assert.Equal("ns-b", imp.Schema.TargetNamespace);
            });
            // check that schema C in sch_b.Includes and its NS correct.
            Assert.All(sch_B.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v9_c.xsd", imp.SchemaLocation);
                Assert.Equal("ns-c", imp.Schema.TargetNamespace);
            });
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v207 - Import: A(ns-a) imports B(NO NS) imports C (ns-c)", Priority = 1)]
        public static void v207()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v10_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);


            XmlSchema sch_B = sc.Add(null, Path.Combine(TestData._Root, "import_v10_b.xsd")); // should be already present in the set
            sc.Add(null, Path.Combine(TestData._Root, "import_v10_c.xsd"));                 // should be already present in the set
            Assert.Equal(3, sc.Count);
            Assert.False(sc.IsCompiled);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            // check that schema is present in parent.Includes and its NS correct.
            Assert.All(parent.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v10_b.xsd", imp.SchemaLocation);
                Assert.Null(imp.Schema.TargetNamespace);
            });

            // check that schema C in sch_b.Includes and its NS correct.
            Assert.All(sch_B.Includes.Cast<XmlSchemaImport>(), imp =>
            {
                Assert.Equal("import_v10_c.xsd", imp.SchemaLocation);
                Assert.Equal("ns-c", imp.Schema.TargetNamespace);
            });

            // try adding no ns schema with an ns
            sch_B = sc.Add("ns-b", Path.Combine(TestData._Root, "import_v10_b.xsd"));
            Assert.Equal(4, sc.Count);

            sc.Compile();
            Assert.Equal(4, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(sch_B);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v208 - Import: A(ns-a) imports B(ns-b) imports C (ns-a)", Priority = 1)]
        public static void v208()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v11_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-a").Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v209 - Import: A(ns-a) imports B(ns-b) and C (ns-b)", Priority = 1)]
        public static void v209()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v12_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-b").Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v210 - Import: A imports B and B and C, B imports C and D, C imports D and A", Priority = 1)]
        public static void v210()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v13_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Compile();
            Assert.Equal(4, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v211 - Import: A(ns-a) imports B(ns-b) and C (ns-b), B and C include each other", Priority = 2)]
        public static void v211()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v14_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.Equal(3, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.Equal(2, sc.Schemas("ns-b").Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v212 - Import: A(ns-a) imports B(BOGUS) and C (ns-c)", Priority = 2)]
        public static void v212()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v15_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v213 - Import: B(ns-b) added, A(ns-a) imports B with bogus url", Priority = 2)]
        public static void v213()
        {
            XmlSchemaSet sc = new XmlSchemaSet();

            sc.Add(null, Path.Combine(TestData._Root, "import_v16_b.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Count);

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v16_a.xsd"));
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);

            sc.Compile();
            Assert.Equal(2, sc.Count);
            Assert.True(sc.IsCompiled);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v214 - Import: A(ns-a) includes B(ns-a) which imports C(ns-c)", Priority = 2)]
        public static void v214()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v17_a.xsd"));

            Assert.Equal(2, sc.Count);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v215 - Import: A(ns-a) includes A(ns-a) of v17", Priority = 2)]
        public static void v215()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v18_a.xsd"));

            Assert.Equal(2, sc.Count);
            Assert.False(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(1, sc.Schemas("ns-c").Count);
            Assert.Equal(2, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(2, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v216 - Import: A(ns-b) imports A(ns-a) of v17", Priority = 2)]
        public static void v216()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v19_a.xsd"));
            Assert.Equal(3, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(1, sc.Schemas("ns-a").Count);
            Assert.Equal(2, sc.Schemas("ns-c").Count);
            Assert.Equal(3, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(3, sc.Count);
        }

        //-----------------------------------------------------------------------------------
        [Fact]
        //[Variation(Desc = "v217 - Import: A,B,C,D all import and reference each other for types", Priority = 2)]
        public static void v217()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();

            XmlSchema parent = sc.Add(null, Path.Combine(TestData._Root, "import_v20_a.xsd"));
            Assert.Equal(4, sc.Count);

            sc.Reprocess(parent);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            XmlSchema b = sc.Add(null, Path.Combine(TestData._Root, "import_v20_b.xsd"));
            XmlSchema c = sc.Add(null, Path.Combine(TestData._Root, "import_v20_c.xsd"));
            XmlSchema d = sc.Add(null, Path.Combine(TestData._Root, "import_v20_d.xsd"));

            Assert.Equal(4, sc.Count);
            Assert.True(sc.IsCompiled);
            Assert.True(b.SourceUri.Contains("import_v20_b.xsd"));
            Assert.True(c.SourceUri.Contains("import_v20_c.xsd"));
            Assert.True(d.SourceUri.Contains("import_v20_d.xsd"));

            sc.Compile();
            Assert.True(sc.IsCompiled);
            Assert.Equal(4, sc.Count);

            sc.Reprocess(b);
            Assert.Equal(4, sc.Count);
            sc.Reprocess(c);
            Assert.Equal(4, sc.Count);
            sc.Reprocess(d);
            Assert.False(sc.IsCompiled);
            Assert.Equal(4, sc.Count);
        }

        [Theory]
        //[Variation(Desc = "v221- Import: Bug 114549 , A imports only B but refers to C and D both", Priority = 1, Params = new object[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v21_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v21_a.xsd" } })]
        //[Variation(Desc = "v222- Import: Bug 114549 , A imports only B's NS, but refers to B,C and D both", Priority = 1, Params = new object[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v21_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" } })]
        //[Variation(Desc = "v223- Import: Bug 114549 , A imports only B's NS, and B also improts A's NS AND refers to A's types", Priority = 1, Params = new object[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" } })]
        public static void v218(string[] filenames)
        {
            XmlSchemaSet ss = new XmlSchemaSet();

            XmlSchema[] schemas = filenames.Select(fn => ss.Add(null, Path.Combine(TestData._Root, fn))).ToArray();

            Assert.Equal(4, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(4, ss.Count);
            Assert.True(ss.IsCompiled);

            foreach (XmlSchema schema in schemas)
            {
                ss.Reprocess(schema);
                Assert.Equal(4, ss.Count);
            }
            Assert.False(ss.IsCompiled);
        }

        [Theory]
        //[Variation(Desc = "v224- Import: Bug 114549 , A imports only B's NS, and B also refers to A's types (WARNING)", Priority = 1, Params = new object[] { "import_v23_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v23_b.xsd", "import_v21_c.xsd", "import_v21_d.xsd", "import_v22_a.xsd" } })]
        //[Variation(Desc = "v225- Import: Bug 114549 , A imports only B's NS, and B also improts A's NS AND refers to A's type, D refers to A's type (WARNING)", Priority = 1, Params = new object[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v25_d.xsd", "import_v22_a.xsd" })]
        [InlineData(new[] { (object)new[] { "import_v24_b.xsd", "import_v21_c.xsd", "import_v25_d.xsd", "import_v22_a.xsd" } })]
        public static void v219(string[] filenames)
        {
            XmlSchemaSet ss = new XmlSchemaSet();

            XmlSchema[] schemas = filenames.Select(fn => ss.Add(null, Path.Combine(TestData._Root, fn))).ToArray();
            Assert.Equal(4, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(4, ss.Count);
            Assert.True(ss.IsCompiled);

            foreach (XmlSchema schema in schemas)
            {
                ss.Reprocess(schema);
                Assert.Equal(4, ss.Count);
            }
            Assert.False(ss.IsCompiled);
        }

        [Fact]
        //[Variation(Desc = "v220 - Import: Bug 105897", Priority = 1)]
        public static void v220()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();

            XmlSchema Schema1 = ss.Add(null, Path.Combine(TestData._Root, "105897.xsd"));
            XmlSchema Schema2 = ss.Add(null, Path.Combine(TestData._Root, "105897_a.xsd"));
            Assert.Equal(3, ss.Count);
            Assert.False(ss.IsCompiled);

            ss.Compile();
            Assert.Equal(3, ss.Count);
            Assert.True(ss.IsCompiled);

            ss.Reprocess(Schema1);
            Assert.Equal(3, ss.Count);
            ss.Reprocess(Schema2);
            Assert.False(ss.IsCompiled);
            Assert.Equal(3, ss.Count);

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings |
                                       XmlSchemaValidationFlags.ProcessSchemaLocation |
                                       XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.Schemas = new XmlSchemaSet();
            settings.Schemas.Add(ss);

            using (XmlReader vr = XmlReader.Create(Path.Combine(TestData._Root, "105897.xml"), settings))
            {
                while (vr.Read()) ;
            }
        }
    }
}
