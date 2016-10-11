// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

<<<<<<< HEAD
using Xunit;
using Xunit.Abstractions;
=======
using System.Collections.Generic;
>>>>>>> Compile
using System.IO;
using System.Xml.Schema;
using Xunit;

namespace System.Xml.Tests
{
    //[TestCase(Name = "TC_SchemaSet_Compile", Desc = "", Priority = 0)]
    public static class TC_SchemaSet_Compile
    {
        [Fact]
        //[Variation(Desc = "v1 - Compile on empty collection")]
        public static void v1()
        {
            XmlSchemaSet sc = new XmlSchemaSet();
            sc.Compile();
        }

        [Fact]
        //[Variation(Desc = "v2 - Compile after error in Add")]
        public static void v2()
        {
            XmlSchemaSet sc = new XmlSchemaSet();

            Assert.Throws<XmlSchemaException>(() => sc.Add(null, Path.Combine(TestData._Root, "schema1.xdr")));
            sc.Compile();
            // GLOBALIZATION
        }

        [Fact]
        //[Variation(Desc = "TFS_470021 Unexpected local particle qualified name when chameleon schema is added to set")]
        public static void TFS_470021()
        {
            string cham = @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema id='a0'
                  elementFormDefault='qualified'
                  xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:complexType name='ctseq1_a'>
    <xs:sequence>
      <xs:element name='foo'/>
    </xs:sequence>
    <xs:attribute name='abt0' type='xs:string'/>
  </xs:complexType>
  <xs:element name='gect1_a' type ='ctseq1_a'/>
</xs:schema>";
            string main = @"<?xml version='1.0' encoding='utf-8' ?>
<xs:schema id='m0'
                  targetNamespace='http://tempuri.org/chameleon1'
                  elementFormDefault='qualified'
                  xmlns='http://tempuri.org/chameleon1'
                  xmlns:mstns='http://tempuri.org/chameleon1'
                  xmlns:xs='http://www.w3.org/2001/XMLSchema'>
  <xs:include schemaLocation='cham.xsd' />

  <xs:element name='root'>
    <xs:complexType>
      <xs:sequence maxOccurs='unbounded'>
        <xs:any namespace='##any' processContents='lax'/>
      </xs:sequence>
    </xs:complexType>
  </xs:element>
</xs:schema>";
            using (XmlWriter w = XmlWriter.Create("cham.xsd"))
            {
                using (XmlReader r = XmlReader.Create(new StringReader(cham)))
                    w.WriteNode(r, true);
            }
            XmlSchemaSet ss = new XmlSchemaSet();

            int warningCount = 0;
            int errorCount = 0;
            List<XmlSchemaException> exceptions = new List<XmlSchemaException>();

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
                if (e.Exception != null)
                {
                    exceptions.Add(e.Exception);
                }
            };

            ss.Add(null, XmlReader.Create(new StringReader(cham)));
            ss.Add(null, XmlReader.Create(new StringReader(main)));
            ss.Compile();

            Assert.Equal(ss.Count, 2);
            XmlSchemaElement chameleon = (XmlSchemaElement)ss.GlobalElements[new XmlQualifiedName("gect1_a")];
            {
                XmlSchemaComplexType type = chameleon.ElementSchemaType as XmlSchemaComplexType;
                XmlSchemaSequence seq = type.ContentTypeParticle as XmlSchemaSequence;
                Assert.Equal(1, seq.Items.Count);
                XmlSchemaElement elem = Assert.IsType<XmlSchemaElement>(seq.Items[0]);
                Assert.NotEqual(string.Empty, elem.QualifiedName.ToString());
            }
            XmlSchemaElement other = (XmlSchemaElement)ss.GlobalElements[new XmlQualifiedName("root", "http://tempuri.org/chameleon1")];
            {
                XmlSchemaComplexType type = other.ElementSchemaType as XmlSchemaComplexType;
                XmlSchemaSequence seq = type.ContentTypeParticle as XmlSchemaSequence;
                Assert.Equal(1, seq.Items.Count);
                Assert.IsType<XmlSchemaAny>(seq.Items[0]);
            }
            Assert.Equal(0, warningCount);
            Assert.Equal(0, errorCount);
            Assert.Empty(exceptions);
        }
    }
}
