// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Xml.Schema;
using Xunit;
using Xunit.Abstractions;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_ProhibitDTD", Desc = "")]
    public static class TC_SchemaSet_ProhibitDTD
    {
        //hook up validaton callback
        public static void ValidationCallback(object sender, ValidationEventArgs args)
        {
            switch (args.Severity)
            {
                case XmlSeverityType.Warning:
                    _output.WriteLine("WARNING: ");
                    bWarningCallback = true;
                    warningCount++;
                    break;

                case XmlSeverityType.Error:
                    _output.WriteLine("ERROR: ");
                    bErrorCallback = true;
                    errorCount++;
                    break;
            }

            _output.WriteLine("Exception Message:" + args.Exception.Message + "\n");

            if (args.Exception.InnerException != null)
            {
                _output.WriteLine("InnerException Message:" + args.Exception.InnerException.Message + "\n");
            }
            else
            {
                _output.WriteLine("Inner Exception is NULL\n");
            }
        }

        private static XmlReaderSettings GetSettings(bool prohibitDtd)
        {
            return new XmlReaderSettings
            {
#pragma warning disable 0618
                ProhibitDtd = prohibitDtd,
#pragma warning restore 0618
                XmlResolver = new XmlUrlResolver()
            };
        }

        private static XmlReader CreateReader(string xmlFile, bool prohibitDtd)
        {
            return XmlReader.Create(xmlFile, GetSettings(prohibitDtd));
        }

        private static XmlReader CreateReader(string xmlFile)
        {
            return XmlReader.Create(xmlFile);
        }

        private static XmlReader CreateReader(XmlReader reader, bool prohibitDtd)
        {
            return XmlReader.Create(reader, GetSettings(prohibitDtd));
        }

        private static XmlReader CreateReader(string xmlFile, XmlSchemaSet ss, bool prohibitDTD)
        {
            var settings = GetSettings(prohibitDTD);

            settings.Schemas = new XmlSchemaSet();
            settings.Schemas.XmlResolver = new XmlUrlResolver();
            settings.Schemas.ValidationEventHandler += ValidationCallback;
            settings.Schemas.Add(ss);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings |
                               XmlSchemaValidationFlags.ProcessSchemaLocation |
                               XmlSchemaValidationFlags.ProcessIdentityConstraints |
                               XmlSchemaValidationFlags.ProcessInlineSchema;

            settings.ValidationEventHandler += ValidationCallback;

            return XmlReader.Create(xmlFile, settings);
        }

        private static XmlReader CreateReader(XmlReader reader, XmlSchemaSet ss, bool prohibitDTD)
        {
            var settings = GetSettings(prohibitDTD);

            settings.Schemas = new XmlSchemaSet();
            settings.Schemas.XmlResolver = new XmlUrlResolver();
            settings.Schemas.ValidationEventHandler += ValidationCallback;
            settings.Schemas.Add(ss);
            settings.ValidationType = ValidationType.Schema;
            settings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings |
                               XmlSchemaValidationFlags.ProcessSchemaLocation |
                               XmlSchemaValidationFlags.ProcessIdentityConstraints |
                               XmlSchemaValidationFlags.ProcessInlineSchema;
            settings.ValidationEventHandler += ValidationCallback;
            return XmlReader.Create(reader, settings);
        }

        //TEST DEFAULT VALUE FOR SCHEMA COMPILATION
        //[Variation(Desc = "v1- Test Default value of ProhibitDTD for Add(URL) of schema with DTD", Priority = 1)]
        [Fact]
        public static void v1()
        {
            XmlSchemaSet xss = new XmlSchemaSet();

            xss.ValidationEventHandler += (sender, e) => { /* Do nothing */ };

            XmlException ex = Assert.Throws<XmlException>(() => xss.Add(null, Path.Combine(TestData._Root, "bug356711_a.xsd")));
            Assert.Contains("DTD", ex.Message);
        }

        //[Variation(Desc = "v2- Test Default value of ProhibitDTD for Add(XmlReader) of schema with DTD", Priority = 1)]
        [Fact]
        public static void v2()
        {
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.ValidationEventHandler += (sender, e) => { /* Do nothing */ };
            XmlReader r = CreateReader(Path.Combine(TestData._Root, "bug356711_a.xsd"));
            XmlException ex = Assert.Throws<XmlException>(() => xss.Add(null, r));
            Assert.Contains("DTD", ex.Message);
        }

        //[Variation(Desc = "v3- Test Default value of ProhibitDTD for Add(URL) containing xs:import for schema with DTD", Priority = 1)]
        [Fact]
        public static void v3()
        {
            int warningCount = 0;
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            xss.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                warningCount++;
            };
            xss.Add(null, Path.Combine(TestData._Root, "bug356711.xsd"));
            // expect a validation warning for unresolvable schema location
            Assert.Equal(1, warningCount);
        }

        //[Variation(Desc = "v4- Test Default value of ProhibitDTD for Add(XmlReader) containing xs:import for scehma with DTD", Priority = 1)]
        [Fact]
        public static void v4()
        {
            int warningCount = 0;
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();

            xss.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                warningCount++;
            };
            XmlReader r = CreateReader(Path.Combine(TestData._Root, "bug356711.xsd"));

            xss.Add(null, r);
            // expect a validation warning for unresolvable schema location
            Assert.Equal(1, warningCount);
        }

        [Theory]
        //[Variation(Desc = "v5.2- Test Default value of ProhibitDTD for Add(TextReader) for schema with DTD", Priority = 1, Params = new object[] { "bug356711_a.xsd", 0 })]
        [InlineData("bug356711_a.xsd")]
        //[Variation(Desc = "v5.1- Test Default value of ProhibitDTD for Add(TextReader) with an xs:import for schema with DTD", Priority = 1, Params = new object[] { "bug356711.xsd", 0 })]
        [InlineData("bug356711.xsd")]
        public static void v5(string fileName)
        {
            bool handlerValidationError = false;
            bool readerValidationError = false;
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            xss.ValidationEventHandler += (sender, e) => handlerValidationError = true;
            XmlSchema schema = XmlSchema.Read(new StreamReader(new FileStream(Path.Combine(TestData._Root, fileName), FileMode.Open, FileAccess.Read)), (sender, e) => readerValidationError = true);
#pragma warning disable 0618
            schema.Compile(ValidationCallback, new XmlUrlResolver());
#pragma warning restore 0618

            xss.Add(schema);
            Assert.False(handlerValidationError);
            Assert.False(readerValidationError);
        }

        [Theory]
        //[Variation(Desc = "v6.2- Test Default value of ProhibitDTD for Add(XmlTextReader) for schema with DTD", Priority = 1, Params = new object[] { "bug356711_a.xsd" })]
        [InlineData("bug356711_a.xsd")]
        //[Variation(Desc = "v6.1- Test Default value of ProhibitDTD for Add(XmlTextReader) with an xs:import for schema with DTD", Priority = 1, Params = new object[] { "bug356711.xsd" })]
        [InlineData("bug356711.xsd")]
        public static void v6(string filename)
        {
            bool handlerValidationError = false;
            bool readerValidationError = false;
            bool compileValidationError = false;

            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();

            xss.ValidationEventHandler += (sender, e) => handlerValidationError = true;
            var reader = new XmlTextReader(Path.Combine(TestData._Root, filename));
            reader.XmlResolver = new XmlUrlResolver();
            XmlSchema schema = XmlSchema.Read(reader, (sender, e) => readerValidationError = true);
#pragma warning disable 0618
            schema.Compile((sender, e) => compileValidationError = true);
#pragma warning restore 0618

            xss.Add(schema);

            Assert.False(handlerValidationError);
            Assert.False(readerValidationError);
            Assert.False(compileValidationError);
        }

        //[Variation(Desc = "v7- Test Default value of ProhibitDTD for Add(XmlReader) for schema with DTD", Priority = 1, Params = new object[] { "bug356711_a.xsd" })]
        [Fact]
        public static void v7()
        {
            XmlSchema schema = XmlSchema.Read(CreateReader(Path.Combine(TestData._Root, "bug356711_a.xsd")), (sender, e) => { /* Do nothing */ });
            XmlException ex = Assert.Throws<XmlException>(() =>
#pragma warning disable 0618
                schema.Compile(ValidationCallback)
#pragma warning restore 0618
            );
            Assert.Contains("DTD", ex.Message);
        }

        //[Variation(Desc = "v8- Test Default value of ProhibitDTD for Add(XmlReader) with xs:import for schema with DTD", Priority = 1, Params = new object[] { "bug356711.xsd" })]
        [Fact]
        public static void v8()
        {
            bool readerValidationError = false;
            bool compileValidationError = false;

            XmlSchema schema = XmlSchema.Read(CreateReader(Path.Combine(TestData._Root, "bug356711.xsd")), (sender, e) => readerValidationError = true);
#pragma warning disable 0618
            schema.Compile((sender, e) => compileValidationError = true);
#pragma warning restore 0618

            Assert.False(readerValidationError);
            Assert.False(compileValidationError);
        }

        //TEST CUSTOM VALUE FOR SCHEMA COMPILATION
        [Theory]
        //[Variation(Desc = "v10.1- Test Custom value of ProhibitDTD for SchemaSet.Add(XmlReader) with xs:import for schema with DTD", Priority = 1, Params = new object[] { "bug356711.xsd" })]
        [InlineData("bug356711.xsd")]
        //[Variation(Desc = "v10.2- Test Custom value of ProhibitDTD for SchemaSet.Add(XmlReader) for schema with DTD", Priority = 1, Params = new object[] { "bug356711_a.xsd" })]
        [InlineData("bug356711_a.xsd")]
        public static void v10(string filename)
        {
            bool validationError = false;
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            xss.ValidationEventHandler += (sender, e) => validationError = true;

            XmlReader r = CreateReader(Path.Combine(TestData._Root, filename), false);
            xss.Add(null, r);

            Assert.False(validationError);
        }

        [Theory]
        //[Variation(Desc = "v11.2- Test Custom value of ProhibitDTD for XmlSchema.Add(XmlReader) for schema with DTD", Priority = 1, Params = new object[] { "bug356711_a.xsd" })]
        [InlineData("bug356711_a.xsd")]
        //[Variation(Desc = "v11.1- Test Custom value of ProhibitDTD for XmlSchema.Add(XmlReader) with an xs:import for schema with DTD", Priority = 1, Params = new object[] { "bug356711.xsd" })]
        [InlineData("bug356711.xsd")]
        public static void v11(string filename)
        {
            bool readerValidationError = false;
            bool compileValidationError = false;

            XmlSchema schema = XmlSchema.Read(CreateReader(Path.Combine(TestData._Root, filename), false), (sender, e) => readerValidationError = true);

#pragma warning disable 0618
            schema.Compile((sender, e) => compileValidationError = true);
#pragma warning restore 0618

            Assert.False(readerValidationError);
            Assert.False(compileValidationError);
        }

        //[Variation(Desc = "v12- Test with underlying reader with ProhibitDTD=true, and new Setting with True for schema with DTD", Priority = 1, Params = new object[] { "bug356711_a.xsd" })]
        [Fact]
        public static void v12()
        {
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.ValidationEventHandler += (sender, e) => { /* Do nothing */ };

            XmlReader r = CreateReader(Path.Combine(TestData._Root, "bug356711_a.xsd"), false);

            XmlReader r2 = CreateReader(r, true);
            XmlException ex = Assert.Throws<XmlException>(() => xss.Add(null, r2));
            Assert.Contains("DTD", ex.Message);
        }

        //[Variation(Desc = "v13- Test with underlying reader with ProhibitDTD=true, and new Setting with True for a schema with xs:import for schema with DTD", Priority = 1, Params = new object[] { "bug356711.xsd" })]
        [Fact]
        public static void v13()
        {
            int handlerWarnings = 0;
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            xss.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                handlerWarnings++;
            };

            XmlReader r = CreateReader(Path.Combine(TestData._Root, "bug356711.xsd"), false);
            XmlReader r2 = CreateReader(r, true);

            xss.Add(null, r2);
            Assert.Equal(1, handlerWarnings);
}

        //[Variation(Desc = "v14 - SchemaSet.Add(XmlReader) with pDTD False ,then a SchemaSet.Add(URL) for schema with DTD", Priority = 1)]
        [Fact]
        public static void v14()
        {
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            xss.ValidationEventHandler += (sender, e) => { /* Do nothing */ };

            XmlReader r = CreateReader(Path.Combine(TestData._Root, "bug356711.xsd"), false);

            xss.Add(null, r);
            Assert.Equal(2, xss.Count);
            XmlException ex = Assert.Throws<XmlException>(() => xss.Add(null, Path.Combine(TestData._Root, "bug356711_b.xsd")));
            Assert.Contains("DTD", ex.Message);
        }

        //[Variation(Desc = "v15 - SchemaSet.Add(XmlReader) with pDTD True ,then a SchemaSet.Add(XmlReader) with pDTD False with DTD", Priority = 1)]
        [Fact]
        public static void v15()
        {
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.ValidationEventHandler += (sender, e) => { /* Do nothing */ };

            XmlReader r1 = CreateReader(Path.Combine(TestData._Root, "bug356711_a.xsd"));
            XmlReader r2 = CreateReader(Path.Combine(TestData._Root, "bug356711_b.xsd"), false);

            XmlException e = Assert.Throws<XmlException>(() => xss.Add(null, r1));
            Assert.Contains("DTD", e.Message);
            Assert.Equal(0, xss.Count);

            xss.Add(null, r2);
            Assert.Equal(1, xss.Count);
        }

        //TEST DEFAULT VALUE FOR INSTANCE VALIDATION
        [Theory]
        //[Variation(Desc = "v20.1- Test Default value of ProhibitDTD for XML containing noNamespaceSchemaLocation for schema which contains xs:import for schema with DTD", Priority = 1, Params = new object[] { "bug356711_1.xml" })]
        [InlineData("bug356711_1.xml")]
        //[Variation(Desc = "v20.2- Test Default value of ProhibitDTD for XML containing schemaLocation for schema with DTD", Priority = 1, Params = new object[] { "bug356711_2.xml" })]
        [InlineData("bug356711_2.xml")]
        //[Variation(Desc = "v20.3- Test Default value of ProhibitDTD for XML containing Inline schema containing xs:import of a schema with DTD", Priority = 1, Params = new object[] { "bug356711_3.xml" })]
        [InlineData("bug356711_3.xml")]
        //[Variation(Desc = "v20.4- Test Default value of ProhibitDTD for XML containing Inline schema containing xs:import of a schema which has a xs:import of schema with DTD", Priority = 1, Params = new object[] { "bug356711_4.xml" })]
        [InlineData("bug356711_4.xml")]
        public static void v20(string filename)
        {
            int handlerWarnings = 0;
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            xss.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                handlerWarnings++;
            };
            xss.Add(null, Path.Combine(TestData._Root, "bug356711_root.xsd"));

            XmlReader reader = CreateReader(Path.Combine(TestData._Root, filename), xss, true);
            while (reader.Read()) ;
            Assert.Equal(2, handlerWarnings);
        }

        //[Variation(Desc = "v21- Underlying XmlReader with ProhibitDTD=False and Create new Reader with ProhibitDTD=True", Priority = 1)]
        [Fact]
        public static void v21()
        {
            int handlerWarnings = 0;
            var xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            xss.ValidationEventHandler += (sender, e) =>
            {
                Assert.Equal(XmlSeverityType.Warning, e.Severity);
                handlerWarnings++;
            };
            xss.Add(null, Path.Combine(TestData._Root, "bug356711_root.xsd"));

            using (var r1 = CreateReader(Path.Combine(TestData._Root, "bug356711_1.xml"), false))
            using (var r2 = CreateReader(r1, xss, true))
            {
                while (r2.Read()) { }
            }

            Assert.Equal(2, handlerWarnings);
        }

        //TEST CUSTOM VALUE FOR INSTANCE VALIDATION
        [Theory]
        //[Variation(Desc = "v22.1- Test Default value of ProhibitDTD for XML containing noNamespaceSchemaLocation for schema which contains xs:import for schema with DTD", Priority = 1, Params = new object[] { "bug356711_1.xml" })]
        [InlineData("bug356711_1.xml")]
        //[Variation(Desc = "v22.2- Test Default value of ProhibitDTD for XML containing schemaLocation for schema with DTD", Priority = 1, Params = new object[] { "bug356711_2.xml" })]
        [InlineData("bug356711_2.xml")]
        //[Variation(Desc = "v22.3- Test Default value of ProhibitDTD for XML containing Inline schema containing xs:import of a schema with DTD", Priority = 1, Params = new object[] { "bug356711_3.xml" })]
        [InlineData("bug356711_3.xml")]
        //[Variation(Desc = "v22.4- Test Default value of ProhibitDTD for XML containing Inline schema containing xs:import of a schema which has a xs:import of schema with DTD", Priority = 1, Params = new object[] { "bug356711_4.xml" })]
        [InlineData("bug356711_4.xml")]
        public static void v22(string filename)
        {
            bool validationError = false;
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.XmlResolver = new XmlUrlResolver();
            xss.ValidationEventHandler += (sender, e) => validationError = true;
            xss.Add(null, Path.Combine(TestData._Root, "bug356711_root.xsd"));

            XmlReader reader = CreateReader(Path.Combine(TestData._Root, filename), xss, false);
            while (reader.Read()) ;
            Assert.False(validationError);
        }

        //[Variation(Desc = "v23- Underlying XmlReader with ProhibitDTD=True and Create new Reader with ProhibitDTD=False", Priority = 1)]
        [Fact]
        public static void v23()
        {
            bool validationError = false;
            XmlSchemaSet xss = new XmlSchemaSet();
            xss.ValidationEventHandler += (sender, e) => validationError = true;
            xss.Add(null, Path.Combine(TestData._Root, "bug356711_root.xsd"));

            XmlReader r1 = CreateReader(Path.Combine(TestData._Root, "bug356711_1.xml"), true);
            XmlReader r2 = CreateReader(r1, xss, false);
            while (r2.Read()) ;

            Assert.False(validationError);
        }
    }
}
