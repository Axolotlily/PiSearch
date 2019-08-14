using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Http.Internal;
using NSubstitute;
using NUnit.Framework;
using StringSearch.Api.Extensions;

namespace UnitTests.Api.Extensions
{
    [TestFixture]
    public class HttpRequestExtensionsTests
    {
        [Theory]
        [TestCase("119.140.0.200")]
        [TestCase("127.0.0.1")]
        [TestCase("192.168.0.1")]
        [TestCase("::1")]
        [TestCase("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
        public void GetClientIp_ForwardedForSingle_ReturnsForwardedFor(string strIp)
        {
            // Arrange
            HttpRequest request = Substitute.For<HttpRequest>();
            request.Headers.Returns(new HeaderDictionary()
            {
                { "X-Forwarded-For", new Microsoft.Extensions.Primitives.StringValues(strIp) }
            });

            IPAddress expected = IPAddress.Parse(strIp);

            // Act
            IPAddress actual = request.GetClientIp();

            // Assert
            Assert.AreEqual(expected, actual);
        }

        [Theory]
        [TestCase("119.140.0.200")]
        [TestCase("127.0.0.1")]
        [TestCase("192.168.0.1")]
        [TestCase("::1")]
        [TestCase("2001:0db8:85a3:0000:0000:8a2e:0370:7334")]
        public void GetClientIp_NoForwardedFor_ReturnsHttpContextConnectionRemoteIpAddress(string strIp)
        {
            // Arrange
            IPAddress expected = IPAddress.Parse(strIp);

            HttpRequest request = Substitute.For<HttpRequest>();
            request.HttpContext.Connection.RemoteIpAddress.Returns(expected);

            // Act
            IPAddress actual = request.GetClientIp();

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
