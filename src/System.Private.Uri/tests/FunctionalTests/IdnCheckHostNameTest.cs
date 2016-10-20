// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Common.Tests;
using System.Net;
using Xunit;

namespace System.PrivateUri.Tests
{
    public static class IdnCheckHostNameTest
    {
        public static IEnumerable<object[]> IdnCheckHostName_Data()
        {
            yield return new object[] { UriHostNameType.Unknown, string.Empty };
            yield return new object[] { UriHostNameType.Dns, "Host" };
            yield return new object[] { UriHostNameType.Dns, "Host.corp.micorosoft.com" };
            yield return new object[] { UriHostNameType.IPv4, IPAddress.Loopback.ToString() };
            yield return new object[] { UriHostNameType.IPv6, IPAddress.IPv6Loopback.ToString() };
            yield return new object[] { UriHostNameType.IPv6, "[" + IPAddress.IPv6Loopback.ToString() + "]" };
        }

        [Theory]
        [MemberData(nameof(IdnCheckHostName_Data))]
        public static void IdnCheckHostName(UriHostNameType type, string hostName)
        {
            Assert.Equal(type, Uri.CheckHostName(hostName));
        }

        [Fact]
        public static void IdnCheckHostName_UnicodeIdnOffIriOn_Dns()
        {
            using (var helper = new ThreadCultureChange())
            {
                Assert.Equal(UriHostNameType.Dns, Uri.CheckHostName("nZMot\u00E1\u00D3\u0063vKi\u00CD.contoso.com"));
                helper.ChangeCultureInfo("zh-cn");
                Assert.Equal(UriHostNameType.Dns, Uri.CheckHostName("nZMot\u00E1\u00D3\u0063vKi\u00CD.contoso.com"));
            }
        }
    }
}
