﻿using FluentAssertions;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace IISExpressBootstrapper.AcceptanceTests
{
    [TestFixture]
    public class IISExpressHostTests
    {
        private IDictionary<string, string> environmentVariables;

        [SetUp]
        public void SetUp()
        {
            environmentVariables = new Dictionary<string, string> { { "Foo1", "Bar1" }, { "Sample2", "It work's!" } };

            IISExpressHost.Start("IISExpressBootstrapper.SampleApp", 8088, environmentVariables);
        }

        [Test]
        public void ShouldRunTheWebApplication()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://localhost:8088/");

            var response = (HttpWebResponse)request.GetResponse();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Test]
        public void ShouldRunTheWebApiApplication()
        {
            var request = (HttpWebRequest)WebRequest.Create("http://localhost:8088/api/sampleapi/10");

            var response = (HttpWebResponse)request.GetResponse();
            var sr = new StreamReader(response.GetResponseStream(), encoding: Encoding.UTF8);
            var content = sr.ReadToEnd();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be("\"You send me 10\"");
        }

        [Test]
        public void ShouldSetEnvironmentVariables()
        {
            TestEnvironmentVariable("Foo1", environmentVariables["Foo1"]);
            TestEnvironmentVariable("Sample2", environmentVariables["Sample2"]);
        }

        private static void TestEnvironmentVariable(string variable, string expected)
        {
            const string url = "http://localhost:8088/Home/EnvironmentVariables?name={0}";

            var request = (HttpWebRequest) WebRequest.Create(string.Format(url, variable));

            var response = (HttpWebResponse) request.GetResponse();
            var sr = new StreamReader(response.GetResponseStream(), encoding: Encoding.UTF8);
            var content = sr.ReadToEnd();

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            content.Should().Be(expected);
        }

        [Test]
        public void ThrowExceptionWhenNotFoundIISExpressPath()
        {
            const string iisExpressPath = @"Z:\Foo\Bar\iis.exe";

            Action action = () => IISExpressHost.Start(null, iisExpressPath: iisExpressPath);

            action.ShouldThrow<IISExpressNotFoundException>();
        }

        [Test]
        public void ThrowExceptionWhenNotFoundWebApplicationPath()
        {
            Action action = () => IISExpressHost.Start("Foo.Bar.Web", 8088);

            action.ShouldThrow<DirectoryNotFoundException>()
                .WithMessage("Could not infer the web application folder path.");
        }

        [Test]
        public void StartingWithInvalidConfigurationShouldWriteMessage()
        {
            string s = null;

            IISExpressHost.Start(new ConfigFileParameters { ConfigFile = "", SiteName = "Does not exist" },
                output: message => { s = message; });

            s.Should().NotBeNullOrEmpty();
        }
    }
}
