// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ninja.Sharp.OpenSODA.Exceptions;
using Ninja.Sharp.OpenSODA.Interfaces;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Extensions;
using Ninja.Sharp.OpenSODA.Models.Configuration;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Provider;
using Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Services;

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Qbe.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOracleSodaSqlQbeServices(this IServiceCollection services, IConfiguration configuration)
        {
            SodaSqlConfiguration? config = configuration.GetSection("Soda").Get<SodaSqlConfiguration>();
            return config == null ? throw new SodaConfigurationException("Invalid SODA configuration") : services.AddOracleSodaSqlQbeServices(config);
        }

        public static IServiceCollection AddOracleSodaSqlQbeServices(this IServiceCollection services, SodaSqlConfiguration configuration)
        {
            services.AddOracleSodaSqlNativeServices(configuration);
            services.Decorate<IDocumentDbProvider, SqlQbeProvider>();
            services.AddScoped<IPaginationService, SqlQbePaginationService>();
            services.Decorate<Native.Provider.IQueryProvider, QbeResxQueryProvider>();
            return services;
        }

        #region decorator pattern impl
        public static void Decorate<TInterface, TDecorator>(this IServiceCollection services)
            where TInterface : class
            where TDecorator : class, TInterface
        {
            ServiceDescriptor interfaceDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(TInterface)) ?? throw new InvalidOperationException($"{typeof(TInterface).Name} is not registered in injection container");

            ObjectFactory decoratorFactory = ActivatorUtilities.CreateFactory(typeof(TDecorator), [typeof(TInterface)]);

            services.Replace(ServiceDescriptor.Describe(typeof(TInterface), serviceProvider => (TInterface)decoratorFactory(serviceProvider, [serviceProvider.CreateInstance(interfaceDescriptor)]), interfaceDescriptor.Lifetime));
        }

        internal static object CreateInstance(this IServiceProvider services, ServiceDescriptor descriptor)
        {
            if (descriptor.ImplementationInstance != null)
            {
                return descriptor.ImplementationInstance;
            }

            if (descriptor.ImplementationFactory != null)
            {
                return descriptor.ImplementationFactory(services);
            }

            object instance = ActivatorUtilities.GetServiceOrCreateInstance(services, descriptor.ImplementationType!);

            return instance;
        }
        #endregion
    }
}
