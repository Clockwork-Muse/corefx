// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

<<<<<<< HEAD
using Xunit;
using Xunit.Abstractions;
using System.IO;
=======
using System.Collections.Generic;
using System.IO;
using System.Linq;
>>>>>>> Misc
using System.Xml.Schema;
using System.Xml.XPath;
using Xunit;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_Misc", Desc = "")]
    public static class TC_SchemaSet_Misc
    {
        //-----------------------------------------------------------------------------------
        //[Variation(Desc = "v1 - Bug110823 - SchemaSet.Add is holding onto some of the schema files after adding", Priority = 1)]
        [Fact]
        // TODO: ADD ASSERTS
        public static void v1()
        {
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            using (XmlTextReader xtr = new XmlTextReader(Path.Combine(TestData._Root, "bug110823.xsd")))
            {
                xss.Add(XmlSchema.Read(xtr, null));
            }
        }

        [Fact]
        //[Variation(Desc = "v2 - Bug115049 - XSD: content model validation for an invalid root element should be abandoned", Priority = 2)]
        public static void v2()
        {
            List<XmlSchemaException> schemaValidationExceptions = new List<XmlSchemaException>(); ;
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.ValidationEventHandler += (sender, e) =>
            {
                schemaValidationExceptions.Add(e.Exception);
            };
            ss.Add(null, Path.Combine(TestData._Root, "bug115049.xsd"));
            ss.Compile();
            Assert.Empty(schemaValidationExceptions);

            //create reader
            List<XmlSchemaException> settingsValidationExceptions = new List<XmlSchemaException>();
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings |
                                       XmlSchemaValidationFlags.ProcessSchemaLocation |
                                       XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Error, e.Severity);
                settingsValidationExceptions.Add(e.Exception);
            };
            settings.Schemas.Add(ss);
            XmlReader vr = XmlReader.Create(Path.Combine(TestData._Root, "bug115049.xml"), settings);
            while (vr.Read()) ;
            Assert.Empty(schemaValidationExceptions);
            Assert.Single(settingsValidationExceptions);
        }

        [Fact]
        //[Variation(Desc = "v4 - 243300 - We are not correctly handling xs:anyType as xsi:type in the instance", Priority = 2)]
        public static void v4()
        {
            string xml = @"<a xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xsd='http://www.w3.org/2001/XMLSchema' xsi:type='xsd:anyType'>1242<b/></a>";
            int errorCount = 0;
            int warningCount = 0;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = new XmlUrlResolver();
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings |
                                       XmlSchemaValidationFlags.ProcessSchemaLocation |
                                       XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationEventHandler += (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    errorCount++;
                }
                else
                {
                    warningCount++;
                }
            };
            XmlReader vr = XmlReader.Create(new StringReader(xml), settings, (string)null);
            while (vr.Read()) ;
            Assert.Equal(0, errorCount);
            Assert.Equal(1, warningCount);
        }

        /* Parameters = file name , is custom xml namespace System.Xml.Tests */

        [Theory]
        //[Variation(Desc = "v20 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v10.xsd", 2, false })]
        [InlineData("bug264908_v10.xsd", 2, false)]
        //[Variation(Desc = "v19 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v9.xsd", 5, true })]
        [InlineData("bug264908_v9.xsd", 5, true)]
        //[Variation(Desc = "v18 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v8.xsd", 5, false })]
        [InlineData("bug264908_v8.xsd", 5, false)]
        //[Variation(Desc = "v17 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v7.xsd", 4, false })]
        [InlineData("bug264908_v7.xsd", 4, false)]
        //[Variation(Desc = "v16 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v6.xsd", 4, true })]
        [InlineData("bug264908_v6.xsd", 4, true)]
        //[Variation(Desc = "v15 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v5.xsd", 4, false })]
        [InlineData("bug264908_v5.xsd", 4, false)]
        //[Variation(Desc = "v14 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v4.xsd", 4, true })]
        [InlineData("bug264908_v4.xsd", 4, true)]
        //[Variation(Desc = "v13 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v3.xsd", 1, true })]
        [InlineData("bug264908_v3.xsd", 1, true)]
        //[Variation(Desc = "v12 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v2.xsd", 1, true })]
        [InlineData("bug264908_v2.xsd", 1, true)]
        //[Variation(Desc = "v11 - DCR 264908 - XSD: Support user specified schema for http://www.w3.org/XML/1998/namespace System.Xml.Tests", Priority = 1, Params = new object[] { "bug264908_v1.xsd", 3, true })]
        [InlineData("bug264908_v1.xsd", 3, true)]
        public static void v10(string xmlFile, int count, bool custom)
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();

            ss.ValidationEventHandler += (sender, e) => { /* Do Nothing */ };
            ss.Add(null, Path.Combine(TestData._Root, xmlFile));
            ss.Compile();

            //test the count
            Assert.Equal(count, ss.Count);
            //make sure the correct schema is in the set
            if (custom)
            {
                Assert.Contains(ss.GlobalAttributes.Values.Cast<XmlSchemaAttribute>(), a => a.QualifiedName.Name == "blah");
            }
        }

        [Fact]
        //[Variation(Desc = "v21 - Bug 319346 - Chameleon add of a schema into the xml namespace", Priority = 1)]
        public static void v20()
        {
            string xmlns = @"http://www.w3.org/XML/1998/namespace";

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();

            ss.ValidationEventHandler += (sender, e) => { /* Do Nothing */ };
            ss.Add(xmlns, Path.Combine(TestData._Root, "bug264908_v11.xsd"));
            ss.Compile();

            //test the count
            Assert.Equal(3, ss.Count);

            //make sure the correct schema is in the set
            Assert.Contains(ss.GlobalAttributes.Values.Cast<XmlSchemaAttribute>(), a => a.QualifiedName.Name == "blah");
        }

        [Fact]
        //[Variation(Desc = "v22 - Bug 338038 - Component should be additive into the Xml namespace", Priority = 1)]
        public static void v21()
        {
            string xmlns = @"http://www.w3.org/XML/1998/namespace";

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();

            ss.ValidationEventHandler += (sender, e) => { /* Do Nothing */ };
            ss.Add(xmlns, Path.Combine(TestData._Root, "bug338038_v1.xsd"));
            ss.Compile();

            //test the count
            Assert.Equal(4, ss.Count);

            //make sure the correct schema is in the set
            Assert.Contains(ss.GlobalAttributes.Values.Cast<XmlSchemaAttribute>(), a => a.QualifiedName.Name == "blah");
        }

        [Fact]
        //[Variation(Desc = "v23 - Bug 338038 - Conflicting components in custom xml namespace System.Xml.Tests be caught", Priority = 1)]
        public static void v22()
        {
            string xmlns = @"http://www.w3.org/XML/1998/namespace";
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(xmlns, Path.Combine(TestData._Root, "bug338038_v2.xsd"));

            Assert.Throws<XmlSchemaException>(() => ss.Compile());
            Assert.Equal(4, ss.Count);
        }

        [Fact]
        //[Variation(Desc = "v24 - Bug 338038 - Change type of xml:lang to decimal in custom xml namespace System.Xml.Tests", Priority = 1)]
        public static void v24()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v3.xsd"));
            ss.Compile();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v3a.xsd"));
            ss.Compile();

            Assert.Equal(4, ss.Count);

            IEnumerable<XmlSchemaAttribute> modified = ss.GlobalAttributes.Values.Cast<XmlSchemaAttribute>().Where(a => a.QualifiedName.Name == "lang");
            Assert.All(modified, a => Assert.Equal("decimal", a.AttributeSchemaType.QualifiedName.Name));
        }

        [Fact]
        //[Variation(Desc = "v25 - Bug 338038 - Conflicting definitions for xml attributes in two schemas", Priority = 1)]
        public static void v25()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v3.xsd"));
            ss.Compile();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v3a.xsd"));
            ss.Compile();

            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v3b.xsd"));
            Assert.Throws<XmlSchemaException>(() => ss.Compile());

            Assert.Equal(6, ss.Count);
        }

        [Fact]
        //[Variation(Desc = "v26 - Bug 338038 - Change type of xml:lang to decimal and xml:base to short in two steps", Priority = 1)]
        public static void v26()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v3.xsd"));
            ss.Compile();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v4a.xsd"));
            ss.Compile();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v4b.xsd"));
            ss.Compile();

            IEnumerable<XmlSchemaAttribute> modifiedLang = ss.GlobalAttributes.Values.Cast<XmlSchemaAttribute>().Where(a => a.QualifiedName.Name == "lang");
            Assert.All(modifiedLang, a => Assert.Equal("decimal", a.AttributeSchemaType.QualifiedName.Name));
            IEnumerable<XmlSchemaAttribute> modifiedBase = ss.GlobalAttributes.Values.Cast<XmlSchemaAttribute>().Where(a => a.QualifiedName.Name == "base");
            Assert.All(modifiedBase, a => Assert.Equal("short", a.AttributeSchemaType.QualifiedName.Name));
            Assert.Equal(6, ss.Count);
        }

        [Fact]
        //[Variation(Desc = "v27 - Bug 338038 - Add new attributes to the already present xml namespace System.Xml.Tests", Priority = 1)]
        public static void v27()
        {
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v3.xsd"));
            ss.Compile();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v4a.xsd"));
            ss.Compile();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v5b.xsd"));
            ss.Compile();

            IEnumerable<XmlSchemaAttribute> added = ss.GlobalAttributes.Values.Cast<XmlSchemaAttribute>().Where(a => a.QualifiedName.Name == "blah");
            Assert.All(added, a => Assert.Equal("int", a.AttributeSchemaType.QualifiedName.Name));
            Assert.Equal(6, ss.Count);
        }

        [Fact]
        //[Variation(Desc = "v28 - Bug 338038 - Add new attributes to the already present xml namespace System.Xml.Tests, remove default ns schema", Priority = 1)]
        public static void v28()
        {
            string xmlns = @"http://www.w3.org/XML/1998/namespace";

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v3.xsd"));
            ss.Compile();

            XmlSchema schema = ss.Schemas(xmlns).Cast<XmlSchema>().Last();

            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v4a.xsd"));
            ss.Compile();
            ss.Add(null, Path.Combine(TestData._Root, "bug338038_v5b.xsd"));
            ss.Compile();

            ss.Remove(schema);
            ss.Compile();

            IEnumerable<XmlSchemaAttribute> added = ss.GlobalAttributes.Values.Cast<XmlSchemaAttribute>().Where(a => a.QualifiedName.Name == "blah");
            Assert.All(added, a => Assert.Equal("int", a.AttributeSchemaType.QualifiedName.Name));
            Assert.Equal(5, ss.Count);
        }

        [Fact]
        //[Variation(Desc = "v100 - Bug 320502 - XmlSchemaSet: while throwing a warning for invalid externals we do not set the inner exception", Priority = 1)]
        public static void v100()
        {
            string xsd = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:include schemaLocation='bogus'/></xs:schema>";
            int warningCount = 0;
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                Assert.NotNull(e.Exception.InnerException);
                warningCount++;
            };
            ss.Add(null, new XmlTextReader(new StringReader(xsd)));
            ss.Compile();
            Assert.Equal(1, warningCount);
        }

        [Fact]
        //[Variation(Desc = "v101 - Bug 339706 - XmlSchemaSet: Compile on the set fails when a compiled schema containing notation is already present", Priority = 1)]
        public static void v101()
        {
            string xsd1 = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:notation name='a' public='a'/></xs:schema>";
            string xsd2 = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'><xs:element name='root'/></xs:schema>";
            int warningCount = 0;
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                Assert.NotNull(e.Exception.InnerException);
                warningCount++;
            };
            ss.Add(null, new XmlTextReader(new StringReader(xsd1)));
            ss.Compile();
            ss.Add(null, new XmlTextReader(new StringReader(xsd2)));
            ss.Compile();
            Assert.Equal(0, warningCount);
        }

        [Fact]
        //[Variation(Desc = "v102 - Bug 337850 - XmlSchemaSet: Type already declared error when redefined schema is added to the set before the redefining schema.", Priority = 1)]
        public static void v102()
        {
            int warningCount = 0;
            int errorCount = 0;
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.ValidationEventHandler += (sender, e) =>
            {
                if (e.Severity == XmlSeverityType.Error)
                {
                    errorCount++;
                }
                else
                {
                    warningCount++;
                }
            };
            ss.Add(null, Path.Combine(TestData._Root, "schZ013c.xsd"));
            ss.Add(null, Path.Combine(TestData._Root, "schZ013a.xsd"));
            ss.Compile();
            Assert.Equal(0, warningCount);
            Assert.Equal(0, errorCount);
        }

        [Theory]
        //[Variation(Desc = "v104 - CodeCoverage- XmlSchemaSet: add precompiled subs groups, global elements, attributes and types to another compiled SOM.", Priority = 1, Params = new object[] { false })]
        [InlineData(false)]
        //[Variation(Desc = "v103 - CodeCoverage- XmlSchemaSet: add precompiled subs groups, global elements, attributes and types to another compiled set.", Priority = 1, Params = new object[] { true })]
        [InlineData(true)]
        public static void v103(bool addset)
        {
            XmlSchemaSet ss1 = new XmlSchemaSet();
            ss1.XmlResolver = new XmlUrlResolver();
            ss1.ValidationEventHandler += (sender, e) => { /* Do Nothing */ };
            ss1.Add(null, Path.Combine(TestData._Root, "Misc103_x.xsd"));
            ss1.Compile();

            Assert.Equal(1, ss1.Count);

            XmlSchemaSet ss2 = new XmlSchemaSet();
            ss2.XmlResolver = new XmlUrlResolver();
            ss2.ValidationEventHandler += (sender, e) => { /* Do Nothing */ };
            XmlSchema s = ss2.Add(null, Path.Combine(TestData._Root, "Misc103_a.xsd"));
            ss2.Compile();

            Assert.Equal(1, ss1.Count);

            if (addset)
            {
                ss1.Add(ss2);

                Assert.Equal(7, ss1.GlobalElements.Count);
                Assert.Equal(2, ss1.GlobalAttributes.Count);
                Assert.Equal(6, ss1.GlobalTypes.Count);
            }
            else
            {
                ss1.Add(s);

                Assert.Equal(2, ss1.GlobalElements.Count);
                Assert.Equal(0, ss1.GlobalAttributes.Count);
                Assert.Equal(2, ss1.GlobalTypes.Count);
            }

            /***********************************************/
            XmlSchemaSet ss3 = new XmlSchemaSet();
            ss3.XmlResolver = new XmlUrlResolver();

            ss3.ValidationEventHandler += (sender, e) => { /* Do Nothing */ };
            ss3.Add(null, Path.Combine(TestData._Root, "Misc103_c.xsd"));
            ss3.Compile();
            ss1.Add(ss3);

            Assert.Equal(8, ss1.GlobalElements.Count);
        }

        [Fact]
        //[Variation(Desc = "v103 - Reference to a component from no namespace System.Xml.Tests an explicit import of no namespace System.Xml.Tests throw a validation warning", Priority = 1)]
        public static void v105()
        {
            int warningCount = 0;
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.XmlResolver = new XmlUrlResolver();

            schemaSet.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                warningCount++;
            };
            schemaSet.Add(null, Path.Combine(TestData._Root, "Misc105.xsd"));
            Assert.Equal(1, warningCount);
        }

        [Fact]
        //[Variation(Desc = "v106 - Adding a compiled SoS(schema for schema) to a set causes type collision error", Priority = 1)]
        public static void v106()
        {
            bool ss1ValidateError = false;
            bool ssValidateError = false;
            XmlSchemaSet ss1 = new XmlSchemaSet();
            ss1.XmlResolver = new XmlUrlResolver();
            ss1.ValidationEventHandler += (sender, e) => ss1ValidateError = true;
            XmlReaderSettings settings = new XmlReaderSettings();
#pragma warning disable 0618
            settings.ProhibitDtd = false;
#pragma warning restore 0618
            XmlReader r = XmlReader.Create(Path.Combine(TestData._Root, "XMLSchema.xsd"), settings);
            ss1.Add(null, r);
            ss1.Compile();

            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.ValidationEventHandler += (sender, e) => ssValidateError = true; ;

            foreach (XmlSchema s in ss1.Schemas())
            {
                ss.Add(s);
            }

            ss.Add(null, Path.Combine(TestData._Root, "xsdauthor.xsd"));
            ss.Compile();
            Assert.False(ss1ValidateError);
            Assert.False(ssValidateError);
        }

        [Fact]
        //[Variation(Desc = "v107 - XsdValidatingReader: InnerException not set on validation warning of a schemaLocation not loaded.", Priority = 1)]
        public static void v107()
        {
            bool schemaSetValidationError = false;
            string strXml = @"<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='a bug356711_a.xsd' xmlns:a='a'></root>";
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.XmlResolver = new XmlUrlResolver();
            schemaSet.ValidationEventHandler += (sender, e) => schemaSetValidationError = true;
            schemaSet.Add(null, Path.Combine(TestData._Root, "bug356711_root.xsd"));

            int warningCount = 0;
            XmlReaderSettings settings = new XmlReaderSettings();
            settings.XmlResolver = new XmlUrlResolver();
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.Schemas.Add(schemaSet);
            settings.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                Assert.NotNull(e.Exception.InnerException);
                warningCount++;
            };
            settings.ValidationType = ValidationType.Schema;
            XmlReader vr = XmlReader.Create(new StringReader(strXml), settings);

            while (vr.Read()) ;

            Assert.False(schemaSetValidationError);
            Assert.Equal(1, warningCount);
        }

        [Fact]
        //[Variation(Desc = "v108 - XmlSchemaSet.Add() should not trust compiled state of the schema being added", Priority = 1)]
        public static void v108()
        {
            string strSchema1 = @"
<xs:schema targetNamespace='http://bar'
           xmlns='http://bar' xmlns:x='http://foo'
           elementFormDefault='qualified'
           attributeFormDefault='unqualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:import namespace='http://foo'/>
  <xs:element name='bar'>
    <xs:complexType>
      <xs:sequence>
        <xs:element ref='x:foo'/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>
";
            string strSchema2 = @"<xs:schema targetNamespace='http://foo'
           xmlns='http://foo' xmlns:x='http://bar'
           elementFormDefault='qualified'
           attributeFormDefault='unqualified'
           xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:import namespace='http://bar'/>
  <xs:element name='foo'>
    <xs:complexType>
      <xs:sequence>
        <xs:element ref='x:bar'/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";

            XmlSchemaSet set = new XmlSchemaSet();
            set.XmlResolver = new XmlUrlResolver();
            int callback1Errors = 0;
            ValidationEventHandler handler = (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Error, e.Severity);
                callback1Errors++;
            };
            set.ValidationEventHandler += handler;
            XmlSchema s1 = null;
            using (XmlReader r = XmlReader.Create(new StringReader(strSchema1)))
            {
                s1 = XmlSchema.Read(r, handler);
                set.Add(s1);
            }
            set.Compile();
            Assert.Equal(1, callback1Errors);

            // Now load set 2
            set = new XmlSchemaSet();
            bool callback2Error = false;
            set.ValidationEventHandler += (sender, e) => callback2Error = true;
            XmlSchema s2 = null;
            using (XmlReader r = XmlReader.Create(new StringReader(strSchema2)))
            {
                s2 = XmlSchema.Read(r, handler);
            }
            Assert.Equal(1, callback1Errors);
            Assert.False(callback2Error);
            XmlSchemaImport import = (XmlSchemaImport)s2.Includes[0];
            import.Schema = s1;
            import = (XmlSchemaImport)s1.Includes[0];
            import.Schema = s2;
            set.Add(s1);
            set.Reprocess(s1);
            set.Add(s2);
            set.Reprocess(s2);
            set.Compile();
            Assert.Equal(1, callback1Errors);
            Assert.False(callback2Error);

            s2 = null;
            using (XmlReader r = XmlReader.Create(new StringReader(strSchema2)))
            {
                s2 = XmlSchema.Read(r, handler);
            }
            set = new XmlSchemaSet();
            Assert.Equal(1, callback1Errors);
            Assert.False(callback2Error);

            bool callback3Error = false;
            set.ValidationEventHandler += (sender, e) => callback3Error = true;

            import = (XmlSchemaImport)s2.Includes[0];
            import.Schema = s1;
            import = (XmlSchemaImport)s1.Includes[0];
            import.Schema = s2;
            set.Add(s1);
            set.Reprocess(s1);
            set.Add(s2);
            set.Reprocess(s2);
            set.Compile();
            Assert.Equal(1, callback1Errors);
            Assert.False(callback2Error);
            Assert.False(callback3Error);
        }

        [Fact]
        //[Variation(Desc = "v109 - 386243, Adding a chameleon schema agsinst to no namaespace throws unexpected warnings", Priority = 1)]
        public static void v109()
        {
            bool validateError = false;
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.ValidationEventHandler += (sender, e) => validateError = true;
            ss.Add("http://EmployeeTest.org", Path.Combine(TestData._Root, "EmployeeTypes.xsd"));
            ss.Add(null, Path.Combine(TestData._Root, "EmployeeTypes.xsd"));

            Assert.False(validateError);
        }

        [Fact]
        //[Variation(Desc = "v110 - 386246,  ArgumentException 'item arleady added' error on a chameleon add done twice", Priority = 1)]
        public static void v110()
        {
            bool validateError = false;
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.ValidationEventHandler += (sender, e) => validateError = true;
            XmlSchema s1 = ss.Add("http://EmployeeTest.org", Path.Combine(TestData._Root, "EmployeeTypes.xsd"));
            XmlSchema s2 = ss.Add("http://EmployeeTest.org", Path.Combine(TestData._Root, "EmployeeTypes.xsd"));

            Assert.False(validateError);
        }

        [Fact]
        //[Variation(Desc = "v111 - 380805,  Chameleon include compiled in one set added to another", Priority = 1)]
        public static void v111()
        {
            bool newSetValidateError = false;
            XmlSchemaSet newSet = new XmlSchemaSet();
            newSet.XmlResolver = new XmlUrlResolver();
            newSet.ValidationEventHandler += (sender, e) => newSetValidateError = true;
            XmlSchema chameleon = newSet.Add(null, Path.Combine(TestData._Root, "EmployeeTypes.xsd"));
            newSet.Compile();

            Assert.False(newSetValidateError);
            Assert.Equal(10, newSet.GlobalTypes.Count);

            bool scValidateError = false;
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            sc.ValidationEventHandler += (sender, e) => scValidateError = true;
            sc.Add(chameleon);
            sc.Add(null, Path.Combine(TestData._Root, "baseEmployee.xsd"));
            sc.Compile();

            Assert.False(newSetValidateError);
            Assert.False(scValidateError);
        }

        [Fact]
        //[Variation(Desc = "v112 - 382035,  schema set tables not cleared as expected on reprocess", Priority = 1)]
        public static void v112()
        {
            bool set2ValidateError = false;
            XmlSchemaSet set2 = new XmlSchemaSet();
            set2.ValidationEventHandler += (sender, e) => set2ValidateError = true;
            XmlSchema includedSchema = set2.Add(null, Path.Combine(TestData._Root, "bug382035a1.xsd"));
            set2.Compile();
            Assert.False(set2ValidateError);

            bool setValidateError = false;
            XmlSchemaSet set = new XmlSchemaSet();
            set.XmlResolver = new XmlUrlResolver();
            set.ValidationEventHandler += (sender, e) => setValidateError = true;
            XmlSchema mainSchema = set.Add(null, Path.Combine(TestData._Root, "bug382035a.xsd"));
            set.Compile();
            Assert.False(setValidateError);

            bool readValidateError = false;
            XmlReader r = XmlReader.Create(Path.Combine(TestData._Root, "bug382035a1.xsd"));
            XmlSchema reParsedInclude = XmlSchema.Read(r, (sender, e) => readValidateError = true);
            Assert.False(readValidateError);

            ((XmlSchemaExternal)mainSchema.Includes[0]).Schema = reParsedInclude;
            set.Reprocess(mainSchema);
            set.Compile();

            Assert.False(set2ValidateError);
            Assert.False(setValidateError);
            Assert.False(readValidateError);
        }

        [Fact]
        //[Variation(Desc = "v113 - Set InnerException on XmlSchemaValidationException while parsing typed values", Priority = 1)]
        public static void v113()
        {
            string strXml = @"<root xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xmlns:xs='http://www.w3.org/2001/XMLSchema' xsi:type='xs:int'>a</root>";

            int errorCount = 0;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings | XmlSchemaValidationFlags.ProcessSchemaLocation;
            settings.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Error, e.Severity);
                Assert.NotNull(e.Exception);
                errorCount++;
            };
            settings.ValidationType = ValidationType.Schema;
            XmlReader vr = XmlReader.Create(new StringReader(strXml), settings);

            while (vr.Read()) ;

            Assert.Equal(1, errorCount);
        }

        [Fact]
        //[Variation(Desc = "v114 - XmlSchemaSet: InnerException not set on parse errors during schema compilation", Priority = 1)]
        public static void v114()
        {
            string strXsd = @"<xs:schema elementFormDefault='qualified' xmlns:xs='http://www.w3.org/2001/XMLSchema'>
 <xs:element name='date' type='date'/>
 <xs:simpleType name='date'>
  <xs:restriction base='xs:int'>
   <xs:enumeration value='a'/>
  </xs:restriction>
 </xs:simpleType>
</xs:schema>";

            int handlerErrorCount = 0;
            int readErrorCount = 0;
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Error, e.Severity);
                Assert.NotNull(e.Exception);
                handlerErrorCount++;
            };
            ss.Add(XmlSchema.Read(new StringReader(strXsd), (sender, e) => readErrorCount++));

            ss.Compile();

            Assert.Equal(0, readErrorCount);
            Assert.Equal(1, handlerErrorCount);
        }

        [Fact]
        //[Variation(Desc = "v116 - 405327 NullReferenceExceptions while accessing obsolete properties in the SOM", Priority = 1)]
        public static void v116()
        {
#pragma warning disable 0618
            XmlSchemaAttribute attribute = new XmlSchemaAttribute();
            Object attributeType = attribute.AttributeType;
            XmlSchemaElement element = new XmlSchemaElement();
            Object elementType = element.ElementType;
            XmlSchemaType schemaType = new XmlSchemaType();
            Object BaseSchemaType = schemaType.BaseSchemaType;
#pragma warning restore 0618
        }

        [Fact]
        //[Variation(Desc = "v117 - 398474 InnerException not set on XmlSchemaException, when xs:pattern has an invalid regular expression", Priority = 1)]
        public static void v117()
        {
            string strXsdv117 =
            @"<?xml version='1.0' encoding='utf-8' ?>
                  <xs:schema  xmlns:xs='http://www.w3.org/2001/XMLSchema'>
                    <xs:element name='doc'>
                      <xs:complexType>
                         <xs:sequence>
                            <xs:element name='value' maxOccurs='unbounded'>
                              <xs:simpleType>
                                 <xs:restriction base='xs:string'>
                                    <xs:pattern value='(?r:foo)'/>
                                 </xs:restriction>
                              </xs:simpleType>
                            </xs:element>
                         </xs:sequence>
                      </xs:complexType>
                    </xs:element>
                  </xs:schema>";

            int handlerErrorCount = 0;
            int readErrorCount = 0;

            using (StringReader reader = new StringReader(strXsdv117))
            {
                XmlSchemaSet ss = new XmlSchemaSet();
                ss.XmlResolver = new XmlUrlResolver();
                ss.ValidationEventHandler += (sender, e) =>
                {
                    Assert.Equal(XmlSeverityType.Error, e.Severity);
                    Assert.NotNull(e.Exception);
                    handlerErrorCount++;
                };
                ss.Add(XmlSchema.Read(reader, (sender, e) => readErrorCount++));
                ss.Compile();

                Assert.Equal(1, handlerErrorCount);
                Assert.Equal(0, readErrorCount);
            }
        }

        [Fact]
        //[Variation(Desc = "v118 - 424904 Not getting unhandled attributes on particle", Priority = 1)]
        public static void v118()
        {
            using (XmlReader r = new XmlTextReader(Path.Combine(TestData._Root, "Bug424904.xsd")))
            {
                XmlSchema s = XmlSchema.Read(r, null);
                XmlSchemaSet set = new XmlSchemaSet();
                set.XmlResolver = new XmlUrlResolver();
                set.Add(s);
                set.Compile();

                XmlQualifiedName name = new XmlQualifiedName("test2", "http://foo");
                XmlSchemaComplexType test2type = s.SchemaTypes[name] as XmlSchemaComplexType;
                XmlSchemaParticle p = test2type.ContentTypeParticle;
                XmlAttribute[] att = p.UnhandledAttributes;
                Assert.NotNull(att);
                Assert.NotEmpty(att);
            }
        }

        [Fact]
        //[Variation(Desc = "v120 - 397633 line number and position not set on the validation error for an invalid xsi:type value", Priority = 1)]
        public static void v120()
        {
            using (XmlReader schemaReader = XmlReader.Create(Path.Combine(TestData._Root, "Bug397633.xsd")))
            {
                XmlSchemaSet sc = new XmlSchemaSet();
                sc.XmlResolver = new XmlUrlResolver();
                sc.Add("", schemaReader);
                sc.Compile();

                XmlReaderSettings readerSettings = new XmlReaderSettings();
                readerSettings.ValidationType = ValidationType.Schema;
                readerSettings.Schemas = sc;

                using (XmlReader docValidatingReader = XmlReader.Create(Path.Combine(TestData._Root, "Bug397633.xml"), readerSettings))
                {
                    XmlDocument doc = new XmlDocument();
                    XmlSchemaValidationException ex = Assert.Throws<XmlSchemaValidationException>(() => doc.Load(docValidatingReader));
                    Assert.Equal(1, ex.LineNumber);
                    Assert.Equal(2, ex.LinePosition);
                    Assert.NotNull(ex.SourceUri);
                    Assert.NotEmpty(ex.SourceUri);
                }
            }
        }

        [Fact]
        //[Variation(Desc = "v120a.XmlDocument.Load non-validating reader.Expect IOE.")]
        public static void v120a()
        {
            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ValidationType = ValidationType.Schema;
            using (XmlReader reader = XmlReader.Create(Path.Combine(TestData._Root, "Bug397633.xml"), readerSettings))
            {
                XmlDocument doc = new XmlDocument();
                Assert.Throws<XmlSchemaValidationException>(() => doc.Load(reader));
            }
        }

        [Fact]
        //[Variation(Desc = "444196: XmlReader.MoveToNextAttribute returns incorrect results")]
        public static void v124()
        {
            string XamlPresentationNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            string XamlToParse =
        "<pfx0:DrawingBrush TileMode=\"Tile\" Viewbox=\"foobar\" />";

            string xml =
        "	<xs:schema " +
        "		xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"" +
        "		xmlns:xs=\"http://www.w3.org/2001/XMLSchema\"" +
        "		targetNamespace=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\" " +
        "		elementFormDefault=\"qualified\" " +
        "		attributeFormDefault=\"unqualified\"" +
        "	>" +
        "" +
        "		<xs:element name=\"DrawingBrush\" type=\"DrawingBrushType\" />" +
        "" +
        "		<xs:complexType name=\"DrawingBrushType\">" +
        "			<xs:attribute name=\"Viewbox\" type=\"xs:string\" />" +
        "			<xs:attribute name=\"TileMode\" type=\"xs:string\" />" +
        "		</xs:complexType>" +
        "	</xs:schema>";

            XmlSchema schema = XmlSchema.Read(new StringReader(xml), null);
            schema.TargetNamespace = XamlPresentationNamespace;
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.XmlResolver = new XmlUrlResolver();
            schemaSet.Add(schema);
            schemaSet.Compile();

            XmlReaderSettings readerSettings = new XmlReaderSettings();
            readerSettings.ConformanceLevel = ConformanceLevel.Fragment;
            readerSettings.ValidationType = ValidationType.Schema;
            readerSettings.Schemas = schemaSet;

            NameTable nameTable = new NameTable();
            XmlNamespaceManager namespaces = new XmlNamespaceManager(nameTable);
            namespaces.AddNamespace("pfx0", XamlPresentationNamespace);
            namespaces.AddNamespace(string.Empty, XamlPresentationNamespace);
            XmlParserContext parserContext = new XmlParserContext(nameTable, namespaces, null, null, null, null, null, null, XmlSpace.None);

            using (XmlReader xmlReader = XmlReader.Create(new StringReader(XamlToParse), readerSettings, parserContext))
            {
                xmlReader.Read();
                xmlReader.MoveToAttribute(0);
                xmlReader.MoveToNextAttribute();
                xmlReader.MoveToNextAttribute();
                xmlReader.MoveToNextAttribute();

                xmlReader.MoveToAttribute(0);
                Assert.True(xmlReader.MoveToNextAttribute());
            }
        }

        //[Variation(Desc = "615444 XmlSchema.Write ((XmlWriter)null) throws InvalidOperationException instead of ArgumenNullException")]
        [Fact(Skip = "TODO: Fix NotImplementedException")]
        public static void v125()
        {
            XmlSchema xs = new XmlSchema();
            Assert.Throws<InvalidOperationException>(() => xs.Write((XmlWriter)null));
        }

        [Fact]
        //[Variation(Desc = "Dev10_40561 Redefine Chameleon: Unexpected qualified name on local particle")]
        public static void Dev10_40561()
        {
            string xml = @"<?xml version='1.0' encoding='utf-8'?><e1 xmlns='ns-a'>  <c23 xmlns='ns-b'/></e1>";
            XmlSchemaSet set = new XmlSchemaSet();
            set.XmlResolver = new XmlUrlResolver();
            string path = Path.Combine(TestData.StandardPath, "xsd10", "SCHEMA", "schN11_a.xsd");
            set.Add(null, path);
            set.Compile();

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas = set;

            using (XmlReader reader = XmlReader.Create(new StringReader(xml), settings))
            {
                // Probably would be better to figure out which iteration throws.
                Assert.Throws<XmlSchemaValidationException>(() => { while (reader.Read()) ; });
            }
        }

        [Fact(Skip = "TODO: Fix NotImplementedException")]
        public static void GetBuiltinSimpleTypeWorksAsEcpected()
        {
            string xml = "<?xml version=\"1.0\" encoding=\"utf-16\"?>" + Environment.NewLine +
 "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">" + Environment.NewLine +
 "  <xs:simpleType>" + Environment.NewLine +
 "    <xs:restriction base=\"xs:anySimpleType\" />" + "Environment.NewLine +
 "  </xs:simpleType>" + Environment.NewLine +
 "</xs:schema>";
            XmlSchema schema = new XmlSchema();
            XmlSchemaSimpleType stringType = XmlSchemaType.GetBuiltInSimpleType(XmlTypeCode.String);
            schema.Items.Add(stringType);
            StringWriter sw = new StringWriter();
            schema.Write(sw);
            Assert.Equal(xml, sw.ToString());
        }

        [Fact]
        //[Variation(Desc = "Dev10_40509 Assert and NRE when validate the XML against the XSD")]
        // TODO: ADD ASSERTS
        public static void Dev10_40509()
        {
            string xml = Path.Combine(TestData._Root, "bug511217.xml");
            string xsd = Path.Combine(TestData._Root, "bug511217.xsd");
            XmlSchemaSet s = new XmlSchemaSet();
            s.XmlResolver = new XmlUrlResolver();
            XmlReader r = XmlReader.Create(xsd);
            s.Add(null, r);
            s.Compile();
            XmlReaderSettings rs = new XmlReaderSettings();
            rs.ValidationType = ValidationType.Schema;
            using (XmlReader docValidatingReader = XmlReader.Create(xml, rs))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(docValidatingReader);
                doc.Schemas = s;
                doc.Validate(null);
            }
        }

        [Fact]
        //[Variation(Desc = "Dev10_40511 XmlSchemaSet::Compile throws XmlSchemaException for valid schema")]
        // TODO: ADD ASSERTS
        public static void Dev10_40511()
        {
            string xsd = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema'>
<xs:simpleType name='textType'>
    <xs:restriction base='xs:string'>
      <xs:minLength value='1' />
    </xs:restriction>
  </xs:simpleType>
  <xs:simpleType name='statusCodeType'>
    <xs:restriction base='textType'>
      <xs:length value='6' />
    </xs:restriction>
  </xs:simpleType>
</xs:schema>";
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.XmlResolver = new XmlUrlResolver();
            sc.Add("xs", XmlReader.Create(new StringReader(xsd)));
            sc.Compile();
        }

        [Fact]
        //[Variation(Desc = "Dev10_40495 Undefined ComplexType error when loading schemas from in memory strings")]
        public static void Dev10_40495()
        {
            const string schema1Str = @"<xs:schema xmlns:tns=""http://BizTalk_Server_Project2.Schema1"" xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" targetNamespace=""http://BizTalk_Server_Project2.Schema1"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:include schemaLocation=""S3"" />
  <xs:include schemaLocation=""S2"" />
  <xs:element name=""Root"">
    <xs:complexType>
      <xs:sequence>
        <xs:element name=""FxTypeElement"">
          <xs:complexType>
            <xs:complexContent mixed=""false"">
              <xs:extension base=""tns:FxType"">
                <xs:attribute name=""Field"" type=""xs:string"" />
              </xs:extension>
            </xs:complexContent>
          </xs:complexType>
        </xs:element>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";

            const string schema2Str = @"<xs:schema xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:complexType name=""FxType"">
    <xs:attribute name=""Fx2"" type=""xs:string"" />
  </xs:complexType>
</xs:schema>";

            const string schema3Str = @"<xs:schema xmlns:b=""http://schemas.microsoft.com/BizTalk/2003"" attributeFormDefault=""unqualified"" elementFormDefault=""qualified"" xmlns:xs=""http://www.w3.org/2001/XMLSchema"">
  <xs:complexType name=""TestType"">
    <xs:attribute name=""Fx2"" type=""xs:string"" />
  </xs:complexType>
</xs:schema>";
            XmlSchema schema1 = XmlSchema.Read(new StringReader(schema1Str), null);
            XmlSchema schema2 = XmlSchema.Read(new StringReader(schema2Str), null);
            XmlSchema schema3 = XmlSchema.Read(new StringReader(schema3Str), null);

            //schema1 has some xs:includes in it. Since all schemas are string based, XmlSchema on its own cannot load automatically
            //load these included schemas. We will resolve these schema locations schema1 and make them point to the correct
            //in memory XmlSchema objects
            ((XmlSchemaExternal)schema1.Includes[0]).Schema = schema3;
            ((XmlSchemaExternal)schema1.Includes[1]).Schema = schema2;

            bool validateError = false;

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.XmlResolver = new XmlUrlResolver();
            schemaSet.ValidationEventHandler += (sender, e) => validateError = true;

            if (schemaSet.Add(schema1) != null)
            {
                //This compile will complain about Undefined complex Type tns:FxType and schemaSet_ValidationEventHandler will be
                //called with this error.
                schemaSet.Compile();
                schemaSet.Reprocess(schema1);
            }
            Assert.False(validateError);
        }

        [Fact]
        //[Variation(Desc = "Dev10_64765 XmlSchemaValidationException.SourceObject is always null when using XPathNavigator.CheckValidity method")]
        public static void Dev10_64765()
        {
            string xsd =
                "<xsd:schema xmlns:xsd='http://www.w3.org/2001/XMLSchema'>" +
                    "<xsd:element name='some'>" +
                    "</xsd:element>" +
                "</xsd:schema>";
            string xml = "<root/>";

            bool readerValidateError = false;
            bool handlerValidateError = false;
            bool validatorError = false;

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.XmlResolver = new XmlUrlResolver();
            schemaSet.Add(XmlSchema.Read(new StringReader(xsd), (sender, e) => readerValidateError = true));
            schemaSet.ValidationEventHandler += (sender, e) => handlerValidateError = true;
            schemaSet.Compile();
            XPathDocument xPathDoc = new XPathDocument(new StringReader(xml));
            XPathNavigator nav = xPathDoc.CreateNavigator();

            nav.CheckValidity(schemaSet, (sender, e) =>
            {
                var ex = Assert.IsType<XmlSchemaValidationException>(e.Exception);
                Assert.NotNull(ex.SourceObject);
                Assert.Equal("MS.Internal.Xml.Cache.XPathDocumentNavigator", ex.SourceObject.GetType().ToString());
                validatorError = true;
            });

            Assert.False(readerValidateError);
            Assert.False(handlerValidateError);
            Assert.True(validatorError);
        }

        [Fact]
        //[Variation(Desc = "Dev10_40563 XmlSchemaSet: Assert Failure with Chk Build.")]
        // TODO: ADD ASSERTS
        public static void Dev10_40563()
        {
            string xsd =
                "<xsd:schema xmlns:xsd='http://www.w3.org/2001/XMLSchema'>" +
                    "<xsd:element name='some'>" +
                    "</xsd:element>" +
                "</xsd:schema>";
            XmlSchemaSet ss = new XmlSchemaSet();
            ss.XmlResolver = new XmlUrlResolver();
            ss.Add("http://www.w3.org/2001/XMLSchema", XmlReader.Create(new StringReader(xsd)));
            XmlReaderSettings rs = new XmlReaderSettings();
            rs.ValidationType = ValidationType.Schema;
            rs.Schemas = ss;
            string input = "<root xml:space='default'/>";
            using (XmlReader r1 = XmlReader.Create(new StringReader(input), rs))
            {
                using (XmlReader r2 = XmlReader.Create(new StringReader(input), rs))
                {
                    while (r1.Read()) ;
                    while (r2.Read()) ;
                }
            }
        }

        [Fact]
        //[Variation(Desc = "TFS_470020 Schema with substitution groups does not throw when content model is ambiguous")]
        public static void TFS_470020()
        {
            string xml = @"<?xml version='1.0' encoding='utf-8' ?>
            <e3>
            <e2>1</e2>
            <e2>1</e2>
            </e3>";

            string xsd = @"<xs:schema xmlns:xs='http://www.w3.org/2001/XMLSchema' elementFormDefault='qualified'>
              <xs:element name='e1' type='xs:int'/>
              <xs:element name='e2' type='xs:int' substitutionGroup='e1'/>
              <xs:complexType name='t3'>
                <xs:sequence>
                  <xs:element ref='e1' minOccurs='0' maxOccurs='1'/>
                  <xs:element name='e2' type='xs:int' minOccurs='0' maxOccurs='1'/>
                </xs:sequence>
              </xs:complexType>
              <xs:element name='e3' type='t3'/>
            </xs:schema>";
            int errorCount = 0;

            XmlSchemaSet set = new XmlSchemaSet();
            set.XmlResolver = new XmlUrlResolver();
            set.Add(null, XmlReader.Create(new StringReader(xsd)));
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            doc.Schemas = set;
            doc.Validate((sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Error, e.Severity);
                errorCount++;
            });
            Assert.Equal(1, errorCount);
        }
    }
}
