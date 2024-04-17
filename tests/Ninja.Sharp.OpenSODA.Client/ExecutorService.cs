using Ninja.Sharp.OpenSODA.Client.Models;
using Ninja.Sharp.OpenSODA.Enums;
using Ninja.Sharp.OpenSODA.Interfaces;
using Ninja.Sharp.OpenSODA.Models;
using Ninja.Sharp.OpenSODA.Queries;
using Ninja.Sharp.OpenSODA.Queries.Operations;
using Ninja.Sharp.OpenSODA.Queries.Primitives;

namespace Ninja.Sharp.OpenSODA.Client
{
    public interface IExecutorService
    {
        public Task ExecuteAsync(string[] args);
    }

    internal class ExecutorService(IDocumentDbProvider sodaProvider) : IExecutorService
    {
        private readonly IDocumentDbProvider sodaProvider = sodaProvider;

        public async Task ExecuteAsync(string[] args)
        {
            /// Capire se la creazione delle collection può essere messa automatica, senza farla fare all'utente
            await sodaProvider.CreateCollectionIfNotExistsAsync<TestObject>();

            var value = Guid.NewGuid().ToString();
            Item<TestObject> item = await sodaProvider.UpsertAsync(new TestObject { One = value });

            item = await sodaProvider.RetrieveAsync<TestObject>(item.Id);

            item.Value.One = value + "-one-updated";
            item.Value.Two = value + "-two-updated";
            item = await sodaProvider.UpsertAsync(new TestObject { One = item.Value.One, Two = item.Value.Two });

            var results1 = await sodaProvider.ListAsync<TestObject>(new Page
            {
                ItemsPerPage = 10,
                PageNumber = 1,
                Ordering = Ordering.Descending,
                OrderingPath = nameof(TestObject.One),
            });

            var results2 = await sodaProvider.ListAsync<TestObject>();

            Query query = new();
            query.With(new And()
                .With(new OString(nameof(TestObject.One), item.Value.One))
                .With(new OString(nameof(TestObject.Two), item.Value.Two))
                );

            var results3 = await sodaProvider.FilterAsync<TestObject>(query);

            await sodaProvider.DeleteAsync<TestObject>(item.Id);
        }
    }
}
