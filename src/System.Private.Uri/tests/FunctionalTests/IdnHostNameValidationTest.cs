// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Xunit;

namespace System.PrivateUri.Tests
{
    public static class IdnHostNameValidationTest
    {
        [Fact]
        public static void IdnHost_Call()
        {
            Uri u = new Uri("http://someHost\u1234.com");
            Assert.Equal("xn--somehost-vk7a.com", u.IdnHost);
        }

        [Fact]
        public static void IdnHost_Call_ThrowsException()
        {
            Uri u = new Uri("/test/test2/file.tst", UriKind.Relative);
            Assert.Throws<InvalidOperationException>(() => u.IdnHost);
        }

        [Fact]
        public static void IdnHost_Internal_Call_ThrowsException()
        {
            Uri u = new Uri("/test/test2/file.tst", UriKind.Relative);
            Assert.Throws<InvalidOperationException>(() => u.DnsSafeHost);
        }

        public static IEnumerable<object[]> CommonSchemes_Data()
        {
            string unicodeHost = "a\u00FChost.dom\u00FCin.n\u00FCet";
            string punycodeHost = "xn--ahost-kva.xn--domin-mva.xn--net-hoa";

            foreach (string scheme in new[] {
                "unknown",
                "http",
                "ftp",
                "file",
                "net.pipe",
                "net.tcp",
                "vsmacros",
                "gopher",
                "nntp",
                "telnet",
                "ldap",
            })
            {
                yield return new object[] { scheme, unicodeHost, unicodeHost, unicodeHost, punycodeHost };
                yield return new object[] { scheme, punycodeHost, punycodeHost, punycodeHost, punycodeHost };
            }
        }

        [Theory]
        [MemberData(nameof(CommonSchemes_Data))]
        public static void CommonSchemes_Test(
            string scheme,
            string host,
            string expectedHost,
            string expectedDnsSafeHost,
            string expectedIdnHost)
        {
            Uri uri;
            Assert.True(Uri.TryCreate(scheme + "://" + host, UriKind.Absolute, out uri));
            Assert.Equal(expectedHost, uri.Host);
            Assert.Equal(expectedDnsSafeHost, uri.DnsSafeHost);
            Assert.Equal(UriHostNameType.Dns, uri.HostNameType);
            Assert.Equal(UriHostNameType.Dns, Uri.CheckHostName(host));
            Assert.Equal(expectedIdnHost, uri.IdnHost);
        }
    }
}
