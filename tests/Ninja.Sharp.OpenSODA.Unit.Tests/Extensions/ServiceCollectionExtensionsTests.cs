using AutoFixture.Xunit2;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Ninja.Sharp.OpenSODA.Driver.Rest.Extensions;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Extensions;
using Ninja.Sharp.OpenSODA.Exceptions;
using ServiceCollectionExtensionsRest = Ninja.Sharp.OpenSODA.Driver.Rest.Extensions.ServiceCollectionExtensions;
using ServiceCollectionExtensionsSqlQbe = Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Extensions.ServiceCollectionExtensions;
using ServiceCollectionExtensionsSql = Ninja.Sharp.OpenSODA.Driver.Sql.Native.Extensions.ServiceCollectionExtensions;
using AutoFixture;
using Ninja.Sharp.OpenSODA.Models.Configuration;
using System.Net.Security;
using System.Net;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Services;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Services;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Provider;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Provider;

namespace Ninja.Sharp.OpenSODA.Unit.Tests.Extensions
{
    public class ServiceCollectionExtensionsTests(AppFixture fixture) : IClassFixture<AppFixture>
    {
        private static Mock<IConfiguration> GetMockConfiguration(string? toOmit = null, string bypassValue = "True", bool secureValue = false)
        {
            Mock<IConfiguration> mockConfiguration = new();
            Mock<IConfigurationSection> mockConfigurationSodaSection = new();
            mockConfigurationSodaSection.SetupGet(s => s.Key).Returns("Soda");
            mockConfigurationSodaSection.SetupGet(s => s.Path).Returns("Soda");
            Mock<IConfigurationSection> mockConfigurationSectionSodaBypassCertificate = new();
            mockConfigurationSectionSodaBypassCertificate.SetupGet(s => s.Key).Returns("BypassCertificate");
            mockConfigurationSectionSodaBypassCertificate.SetupGet(s => s.Path).Returns("Soda:BypassCertificate");
            mockConfigurationSectionSodaBypassCertificate.SetupGet(s => s.Value).Returns(bypassValue);
            Mock<IConfigurationSection> mockConfigurationSectionSodaHost = new();
            mockConfigurationSectionSodaHost.SetupGet(s => s.Key).Returns("Host");
            mockConfigurationSectionSodaHost.SetupGet(s => s.Path).Returns("Soda:Host");
            mockConfigurationSectionSodaHost.SetupGet(s => s.Value).Returns("localhost");
            Mock<IConfigurationSection> mockConfigurationSectionSodaPassword = new();
            mockConfigurationSectionSodaPassword.SetupGet(s => s.Key).Returns("Password");
            mockConfigurationSectionSodaPassword.SetupGet(s => s.Path).Returns("Soda:Password");
            mockConfigurationSectionSodaPassword.SetupGet(s => s.Value).Returns("password");
            Mock<IConfigurationSection> mockConfigurationSectionSodaPortRest = new();
            mockConfigurationSectionSodaPortRest.SetupGet(s => s.Key).Returns("PortRest");
            mockConfigurationSectionSodaPortRest.SetupGet(s => s.Path).Returns("Soda:PortRest");
            mockConfigurationSectionSodaPortRest.SetupGet(s => s.Value).Returns("8080");
            Mock<IConfigurationSection> mockConfigurationSectionSodaPortSql = new();
            mockConfigurationSectionSodaPortSql.SetupGet(s => s.Key).Returns("PortSql");
            mockConfigurationSectionSodaPortSql.SetupGet(s => s.Path).Returns("Soda:PortSql");
            mockConfigurationSectionSodaPortSql.SetupGet(s => s.Value).Returns("1521");
            Mock<IConfigurationSection> mockConfigurationSectionSodaSchema = new();
            mockConfigurationSectionSodaSchema.SetupGet(s => s.Key).Returns("Schema");
            mockConfigurationSectionSodaSchema.SetupGet(s => s.Path).Returns("Soda:Schema");
            mockConfigurationSectionSodaSchema.SetupGet(s => s.Value).Returns("myschema");
            Mock<IConfigurationSection> mockConfigurationSectionSodaServiceName = new();
            mockConfigurationSectionSodaServiceName.SetupGet(s => s.Key).Returns("ServiceName");
            mockConfigurationSectionSodaServiceName.SetupGet(s => s.Path).Returns("Soda:ServiceName");
            mockConfigurationSectionSodaServiceName.SetupGet(s => s.Value).Returns("MYSERVICENAME");
            Mock<IConfigurationSection> mockConfigurationSectionSodaUsername = new();
            mockConfigurationSectionSodaUsername.SetupGet(s => s.Key).Returns("Username");
            mockConfigurationSectionSodaUsername.SetupGet(s => s.Path).Returns("Soda:Username");
            mockConfigurationSectionSodaUsername.SetupGet(s => s.Value).Returns("myuser");
            Mock<IConfigurationSection> mockConfigurationSectionSodaIsSecure = new();
            mockConfigurationSectionSodaIsSecure.SetupGet(s => s.Key).Returns("IsSecure");
            mockConfigurationSectionSodaIsSecure.SetupGet(s => s.Path).Returns("Soda:IsSecure");
            mockConfigurationSectionSodaIsSecure.SetupGet(s => s.Value).Returns(secureValue.ToString());
            mockConfigurationSodaSection.Setup(s => s.GetChildren()).Returns(
            [
                mockConfigurationSectionSodaBypassCertificate.Object,
                mockConfigurationSectionSodaHost.Object,
                mockConfigurationSectionSodaPassword.Object,
                mockConfigurationSectionSodaPortRest.Object,
                mockConfigurationSectionSodaPortSql.Object,
                mockConfigurationSectionSodaSchema.Object,
                mockConfigurationSectionSodaServiceName.Object,
                mockConfigurationSectionSodaUsername.Object,
                mockConfigurationSectionSodaIsSecure.Object
            ]);

            if (toOmit != "BypassCertificate")
            {
                mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "BypassCertificate"))).Returns(mockConfigurationSectionSodaBypassCertificate.Object);
            }
            if (toOmit != "Host")
            {
                mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "Host"))).Returns(mockConfigurationSectionSodaHost.Object);
            }
            if (toOmit != "Password")
            {
                mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "Password"))).Returns(mockConfigurationSectionSodaPassword.Object);
            }
            if (toOmit != "PortRest")
            {
                mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "PortRest"))).Returns(mockConfigurationSectionSodaPortRest.Object);
            }
            if (toOmit != "PortSql")
            {
                mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "PortSql"))).Returns(mockConfigurationSectionSodaPortSql.Object);
            }
            if (toOmit == "Schema")
            {
                mockConfigurationSectionSodaSchema.SetupGet(s => s.Value).Returns(string.Empty);
            }
            mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "Schema"))).Returns(mockConfigurationSectionSodaSchema.Object);
            if (toOmit != "ServiceName")
            {
                mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "ServiceName"))).Returns(mockConfigurationSectionSodaServiceName.Object);
            }
            if (toOmit != "Username")
            {
                mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "Username"))).Returns(mockConfigurationSectionSodaUsername.Object);
            }
            if (toOmit != "IsSecure")
            {
                mockConfigurationSodaSection.Setup(s => s.GetSection(It.Is<string>(s => s == "IsSecure"))).Returns(mockConfigurationSectionSodaIsSecure.Object);
            }
            mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(mockConfigurationSodaSection.Object);

            return mockConfiguration;
        }

        [Fact]
        public void AddOracleRdbmsRestServices_Ok()
        {
            ServiceCollection serviceCollection = new();

            Exception exception = Record.Exception(() => serviceCollection.AddOracleSodaRestServices(GetMockConfiguration().Object));

            Assert.Null(exception);
        }

        [Fact]
        public void AddOracleRdbmsSqlServices_Ok()
        {
            ServiceCollection serviceCollection = new();

            Exception exception = Record.Exception(() => serviceCollection.AddOracleSodaSqlNativeServices(GetMockConfiguration().Object));

            Assert.Null(exception);
        }

        [Theory]
        [InlineAutoData("Host")]
        [InlineAutoData("Username")]
        [InlineAutoData("Password")]
        [InlineAutoData("Schema")]
        public void AddOracleRdbmsRestServices_Ko_ConfigMissing(string configurationToOmit)
        {
            // Arrange
            ServiceCollection serviceCollection = new();

            // Act

            // Assert
            Assert.Throws<SodaConfigurationException>(() => ServiceCollectionExtensionsRest.AddOracleSodaRestServices(serviceCollection, GetMockConfiguration(configurationToOmit).Object));
        }

        [Fact]
        public void AddOracleRdbmsRestServices_Ko_ConfigNull()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            Mock<IConfiguration> mockConfiguration = new();
            Mock<IConfigurationSection> mockConfigurationSodaSection = new();
            mockConfigurationSodaSection.SetupGet(s => s.Key).Returns("Soda");
            mockConfigurationSodaSection.SetupGet(s => s.Path).Returns("Soda");
            mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(mockConfigurationSodaSection.Object);

            // Act

            // Assert
            Assert.Throws<SodaConfigurationException>(() => ServiceCollectionExtensionsRest.AddOracleSodaRestServices(serviceCollection, mockConfiguration.Object));
        }

        [Fact]
        public void AddOracleSodaSqlQbeServices_Ok()
        {
            // Arrange
            ServiceCollection serviceCollection = new();

            // Act
            IServiceCollection result = ServiceCollectionExtensionsSqlQbe.AddOracleSodaSqlQbeServices(serviceCollection, GetMockConfiguration().Object);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData("Host")]
        [InlineAutoData("Username")]
        [InlineAutoData("Password")]
        [InlineAutoData("Schema")]
        public void AddOracleSodaSqlQbeServices_Ko_ConfigMissing(string configurationToOmit)
        {
            // Arrange
            ServiceCollection serviceCollection = new();

            // Act

            // Assert
            Assert.Throws<SodaConfigurationException>(() => ServiceCollectionExtensionsSqlQbe.AddOracleSodaSqlQbeServices(serviceCollection, GetMockConfiguration(configurationToOmit).Object));
        }

        [Fact]
        public void AddOracleSodaSqlQbeServices_Ko_ConfigNull()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            Mock<IConfiguration> mockConfiguration = new();
            Mock<IConfigurationSection> mockConfigurationSodaSection = new();
            mockConfigurationSodaSection.SetupGet(s => s.Key).Returns("Soda");
            mockConfigurationSodaSection.SetupGet(s => s.Path).Returns("Soda");
            mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(mockConfigurationSodaSection.Object);

            // Act

            // Assert
            Assert.Throws<SodaConfigurationException>(() => ServiceCollectionExtensionsSqlQbe.AddOracleSodaSqlQbeServices(serviceCollection, mockConfiguration.Object));
        }

        [Fact]
        public void AddOracleSodaSqlServices_Ko_ConfigNull()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            Mock<IConfiguration> mockConfiguration = new();
            Mock<IConfigurationSection> mockConfigurationSodaSection = new();
            mockConfigurationSodaSection.SetupGet(s => s.Key).Returns("Soda");
            mockConfigurationSodaSection.SetupGet(s => s.Path).Returns("Soda");
            mockConfiguration.Setup(c => c.GetSection(It.IsAny<string>())).Returns(mockConfigurationSodaSection.Object);

            // Act

            // Assert
            Assert.Throws<SodaConfigurationException>(() => ServiceCollectionExtensionsSql.AddOracleSodaSqlNativeServices(serviceCollection, mockConfiguration.Object));
        }
        [Theory]
        [InlineAutoData]
        public void AddHttpClient_Ok(SodaRestConfiguration sodaConfiguration)
        {
            // Arrange
            ServiceCollection serviceCollection = new();

            // Act
            IServiceCollection result = ServiceCollectionExtensionsRest.AddHttpClient(serviceCollection, sodaConfiguration);

            // Assert
            Assert.NotNull(result);
        }

        [Theory]
        [InlineAutoData(HttpStatusCode.NotFound, true)]
        [InlineAutoData(HttpStatusCode.BadRequest, true)]
        [InlineAutoData(HttpStatusCode.BadGateway, true)]
        [InlineAutoData(HttpStatusCode.InternalServerError, true)]
        [InlineAutoData(HttpStatusCode.OK, false)]
        public void CheckRetryPolicyResult(HttpStatusCode statusCode, bool result)
        {
            // Arrange
            HttpResponseMessage responseMessage = new()
            {
                StatusCode = statusCode
            };

            // Act
            bool checkResult = ServiceCollectionExtensionsRest.CheckResultForRetryPolicy(responseMessage);

            // Assert
            Assert.Equal(result, checkResult);
        }

        [Theory]
        [InlineAutoData(true)]
        [InlineAutoData(false)]
        public void ConfigureHttpClient_Ok(bool secureValue)
        {
            // Arrange
            HttpClient client = new();
            ServiceCollection serviceCollection = new();
            ServiceCollectionExtensionsRest.AddOracleSodaRestServices(serviceCollection, GetMockConfiguration().Object);
            SodaRestConfiguration config = fixture.Fixture.Create<SodaRestConfiguration>();
            config.IsSecure = secureValue;

            // Act
            Exception exception = Record.Exception(() => ServiceCollectionExtensionsRest.ConfigureHttpClient(client, config));

            // Assert
            Assert.Null(exception);
        }


        [Theory]
        [InlineAutoData(true, SslPolicyErrors.RemoteCertificateNotAvailable, true)]
        [InlineAutoData(false, SslPolicyErrors.None, true)]
        [InlineAutoData(false, SslPolicyErrors.RemoteCertificateNameMismatch, false)]
        public void CertificateCallback_Ok(bool bypass, SslPolicyErrors policyErrors, bool expectedResult)
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            ServiceCollectionExtensionsRest.AddOracleSodaRestServices(serviceCollection, GetMockConfiguration(bypassValue: bypass ? "True" : "False").Object);
            SodaRestConfiguration sodaConfig = fixture.Fixture.Create<SodaRestConfiguration>();
            sodaConfig.BypassCertificate = bypass;

            // Act
            bool result = ServiceCollectionExtensionsRest.CertificateCallback(policyErrors, sodaConfig);

            // Assert
            Assert.Equal(expectedResult, result);
        }

        [Fact]
        public void ConfigurePrimaryHandler_Ok()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            ServiceCollectionExtensionsRest.AddOracleSodaRestServices(serviceCollection, GetMockConfiguration().Object);

            // Act
            HttpClientHandler handler = ServiceCollectionExtensionsRest.ConfigurePrimaryHandler(fixture.Fixture.Create<SodaRestConfiguration>());

            // Assert
            Assert.NotNull(handler);
        }

        [Fact]
        public void Decorate_Ok()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            serviceCollection.AddScoped<Driver.Sql.Native.Provider.IQueryProvider, ResxQueryProvider>();

            // Act
            Exception? exception = Record.Exception(() => ServiceCollectionExtensionsSqlQbe.Decorate<Driver.Sql.Native.Provider.IQueryProvider, QbeResxQueryProvider>(serviceCollection));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void Decorate_NotRegistered_InvalidOperationException()
        {
            // Arrange
            ServiceCollection serviceCollection = new();

            // Act + Assert
            Assert.Throws<InvalidOperationException>(() => ServiceCollectionExtensionsSqlQbe.Decorate<Driver.Sql.Native.Provider.IQueryProvider, QbeResxQueryProvider>(serviceCollection));
        }

        [Fact]
        public void CreateInstance_NoInstanceOrFactory_Ok()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            serviceCollection.AddScoped<Driver.Sql.Native.Provider.IQueryProvider, ResxQueryProvider>();
            ServiceDescriptor descriptor = serviceCollection.Single(s => s.ServiceType == typeof(Driver.Sql.Native.Provider.IQueryProvider));
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            object instance = ServiceCollectionExtensionsSqlQbe.CreateInstance(serviceProvider, descriptor);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CreateInstance_WithInstance_Ok()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            serviceCollection.AddSingleton<Driver.Sql.Native.Provider.IQueryProvider>(new ResxQueryProvider());
            ServiceDescriptor descriptor = serviceCollection.Single(s => s.ServiceType == typeof(Driver.Sql.Native.Provider.IQueryProvider));
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            object instance = ServiceCollectionExtensionsSqlQbe.CreateInstance(serviceProvider, descriptor);

            // Assert
            Assert.NotNull(instance);
        }

        [Fact]
        public void CreateInstance_WithFactory_Ok()
        {
            // Arrange
            ServiceCollection serviceCollection = new();
            serviceCollection.AddScoped<Driver.Sql.Native.Provider.IQueryProvider>(p => new ResxQueryProvider());
            ServiceDescriptor descriptor = serviceCollection.Single(s => s.ServiceType == typeof(Driver.Sql.Native.Provider.IQueryProvider));
            ServiceProvider serviceProvider = serviceCollection.BuildServiceProvider();

            // Act
            object instance = ServiceCollectionExtensionsSqlQbe.CreateInstance(serviceProvider, descriptor);

            // Assert
            Assert.NotNull(instance);
        }
    }
}
