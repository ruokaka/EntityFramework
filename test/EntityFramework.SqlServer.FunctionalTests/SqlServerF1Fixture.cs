// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.FunctionalTests;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Fallback;

namespace Microsoft.Data.Entity.SqlServer.FunctionalTests
{
    public class SqlServerF1Fixture : RelationalF1Fixture<SqlServerTestStore>
    {
        public static readonly string DatabaseName = "OptimisticConcurrencyTest";

        private readonly IServiceProvider _serviceProvider;

        private readonly string _connectionString = SqlServerTestStore.CreateConnectionString(DatabaseName);

        public SqlServerF1Fixture()
        {
            _serviceProvider = new ServiceCollection()
                .AddEntityFramework()
                .AddSqlServer()
                .ServiceCollection
                .AddTestModelSource(OnModelCreating)
                .BuildServiceProvider();
        }

        public override SqlServerTestStore CreateTestStore()
        {
            return SqlServerTestStore.GetOrCreateSharedAsync(DatabaseName, async () =>
                {
                    var options = new DbContextOptions();
                    options.UseSqlServer(_connectionString);

                    using (var context = new F1Context(_serviceProvider, options))
                    {
                        if (await context.Database.EnsureCreatedAsync())
                        {
                            await ConcurrencyModelInitializer.SeedAsync(context);
                        }
                    }
                }).Result;
        }

        public override F1Context CreateContext(SqlServerTestStore testStore)
        {
            var options = new DbContextOptions();
            options.UseSqlServer(testStore.Connection);

            var context = new F1Context(_serviceProvider, options);
            context.Database.AsRelational().Connection.UseTransaction(testStore.Transaction);
            return context;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ForSqlServer().UseSequence();
        }
    }
}
