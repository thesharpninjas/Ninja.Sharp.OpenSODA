using AutoFixture;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Ninja.Sharp.OpenSODA.Models.Configuration;
using System.Data;
using System.Diagnostics;

namespace Ninja.Sharp.OpenSODA.Unit.Tests
{
    public class AppFixture : IDisposable
    {
        public class TestClassNested
        {
            public string NestedName { get; set; } = string.Empty;
            public DateTime? NullableDate { get; set; }
        }

        [Attributes.Collection("TestClass")]
        public class TestClass
        {
            public string Name { get; set; } = string.Empty;
            public int Id { get; set; }
            public TestClassNested Nested { get; set; } = new();
        }

        [Attributes.Collection("")]
        public class TestClassEmptyName
        {
        }

        [Attributes.Collection("!?=")]
        public class TestClassNotAlphaName
        {
        }

        public interface ITestClassNoAttribute
        {
        }

        internal ServiceProvider ServiceProvider { get; private set; }
        internal SodaSqlConfiguration SqlConfiguration { get; private set; }
        internal SodaRestConfiguration RestConfiguration { get; private set; }
        internal Fixture Fixture { get; private set; }

        public AppFixture()
        {
            Fixture = new();
            Fixture.Behaviors
                .OfType<ThrowingRecursionBehavior>()
                .ToList()
                .ForEach(b => Fixture.Behaviors.Remove(b));
            Fixture.Behaviors.Add(new OmitOnRecursionBehavior(recursionDepth: 1));

            ServiceCollection services = new();
            ServiceProvider = services.BuildServiceProvider();

            SqlConfiguration = new()
            {
                Schema = "schema"
            };

            RestConfiguration = new()
            {
                Schema = "schema"
            };
        }

        public ILogger<T> Logger<T>()
        {
            var logger = new Mock<ILogger<T>>();

            logger.Setup(x => x.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()))
                .Callback(new InvocationAction(invocation =>
                {
                    var logLevel = (LogLevel)invocation.Arguments[0]; // The first two will always be whatever is specified in the setup above
                    var state = invocation.Arguments[2];
                    var exception = (Exception)invocation.Arguments[3];
                    var formatter = invocation.Arguments[4];

                    var invokeMethod = formatter.GetType().GetMethod("Invoke");
                    var logMessage = invokeMethod!.Invoke(formatter, [state, exception]);

                    Trace.WriteLine($"{logLevel} - {logMessage}");
                }));
            return logger.Object;
        }

        internal static IHttpClientFactory GetClientFactory(Mock<HttpMessageHandler> mockHttpMessageHandler)
        {
            Mock<IHttpClientFactory> clientFactoryMockNotFound = new(MockBehavior.Strict);
            clientFactoryMockNotFound.Setup(cf => cf.CreateClient(It.IsAny<string>())).Returns(() => new(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://test.it")
            });
            return clientFactoryMockNotFound.Object;
        }

        internal static IDbConnection GetDbConnection(Mock<IDataReader>? mockDataReader = null, Mock<IDbCommand>? mockDbCommand = null)
        {
            Mock<IDbConnection> mockDbConnection = new(MockBehavior.Strict);
            if (mockDbCommand == null)
            {
                mockDbCommand = new(MockBehavior.Strict);
                Mock<IDataParameterCollection> mockParameterCollection = new();
                mockDbCommand.Setup(c => c.Parameters).Returns(mockParameterCollection.Object);
                mockDbCommand.Setup(c => c.ExecuteNonQuery()).Returns(1);
            }
            mockDataReader ??= new(MockBehavior.Strict);
            mockDbCommand.Setup(c => c.ExecuteReader()).Returns(mockDataReader.Object);
            mockDbCommand.Setup(c => c.Dispose());
            mockDbCommand.SetupSet(p => p.CommandText = It.IsAny<string>());
            mockDbCommand.SetupGet(p => p.CommandText).Returns(string.Empty);
            mockDbConnection.Setup(c => c.CreateCommand()).Returns(mockDbCommand.Object);
            mockDbConnection.Setup(c => c.Open());
            mockDbConnection.Setup(c => c.Close());
            mockDbConnection.Setup(c => c.Dispose());

            return mockDbConnection.Object;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        protected virtual void Dispose(bool disposing)
        {
        }
    }
}
