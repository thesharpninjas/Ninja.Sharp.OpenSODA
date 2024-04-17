// (c) 2024 thesharpninjas
// This code is licensed under MIT license (see LICENSE.txt for details)

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Provider;
using Ninja.Sharp.OpenSODA.Driver.Sql.Native.Services;
using Ninja.Sharp.OpenSODA.Exceptions;
using Ninja.Sharp.OpenSODA.Interfaces;
using Ninja.Sharp.OpenSODA.Models.Configuration;
using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace Ninja.Sharp.OpenSODA.Driver.Sql.Native.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOracleSodaSqlNativeServices(this IServiceCollection services, IConfiguration configuration)
        {
            SodaSqlConfiguration? config = configuration.GetSection("Soda").Get<SodaSqlConfiguration>();
            return config == null ? throw new SodaConfigurationException("Invalid SODA configuration") : services.AddOracleSodaSqlNativeServices(config);
        }

        public static IServiceCollection AddOracleSodaSqlNativeServices(this IServiceCollection services, SodaSqlConfiguration configuration)
        {
            services.AddOracleDatabaseContext(configuration);
            services.AddScoped<IDocumentDbProvider, SqlNativeProvider>();
            services.AddScoped<IPaginationService, SqlPaginationService>();
            services.AddScoped<Provider.IQueryProvider, ResxQueryProvider>();
            return services;
        }

        #region private
        internal static IServiceCollection AddOracleDatabaseContext(this IServiceCollection services, SodaSqlConfiguration configuration)
        {
            if (string.IsNullOrWhiteSpace(configuration.Host) ||
                string.IsNullOrWhiteSpace(configuration.Username) ||
                string.IsNullOrWhiteSpace(configuration.Password) ||
                string.IsNullOrWhiteSpace(configuration.Schema))
            {
                throw new SodaConfigurationException("Invalid SODA configuration");
            }

            var connectionString = $"Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST={configuration.Host})(PORT={configuration.Port}))(CONNECT_DATA=(SERVICE_NAME={configuration.ServiceName})));User Id={configuration.Username};password={configuration.Password};";

            services.AddScoped<IDbConnection>(db => new OracleConnection(connectionString));

            return services;
        }
        #endregion
    }
}
