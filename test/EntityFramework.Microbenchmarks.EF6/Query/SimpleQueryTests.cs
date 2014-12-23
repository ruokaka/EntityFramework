﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using EntityFramework.Microbenchmarks.Core;
using EntityFramework.Microbenchmarks.EF6.Models.Orders;
using System;
using System.Data.Entity;
using System.Linq;
using Xunit;

namespace EntityFramework.Microbenchmarks.Query
{
    public class SimpleQueryTests
    {
        private static string _connectionString = String.Format(@"Server={0};Database=Perf_Query_Simple_EF6;Integrated Security=True;MultipleActiveResultSets=true;", TestConfig.Instance.DataSource);

        [Fact]
        public void LoadAll()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_LoadAll_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Products.ToList();
                        collector.Stop();
                        Assert.Equal(1000, result.Count);
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void Where()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_Where_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Products.Where(p => p.Retail < 15).ToList();
                        collector.Stop();
                        Assert.Equal(500, result.Count);
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void OrderBy()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_OrderBy_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Products.OrderBy(p => p.Retail).ToList();
                        collector.Stop();
                        Assert.Equal(1000, result.Count);
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void Count()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_Count_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Products.Count();
                        collector.Stop();
                        Assert.Equal(1000, result);
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void SkipTake()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_SkipTake_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Products
                            .OrderBy(p => p.ProductId)
                            .Skip(500).Take(500)
                            .ToList();

                        collector.Stop();
                        Assert.Equal(500, result.Count);
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void GroupBy()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_GroupBy_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Products
                            .GroupBy(p => p.Retail)
                            .Select(g => new
                            {
                                Retail = g.Key,
                                Products = g
                            })
                            .ToList();

                        collector.Stop();
                        Assert.Equal(10, result.Count);
                        Assert.All(result, g => Assert.Equal(100, g.Products.Count()));
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void Include()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_Include_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Customers.Include(c => c.Orders).ToList();
                        collector.Stop();
                        Assert.Equal(1000, result.Count);
                        Assert.Equal(2000, result.SelectMany(c => c.Orders).Count());
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void Projection()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_Projection_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Products.Select(p => new { p.Name, p.Retail }).ToList();
                        collector.Stop();
                        Assert.Equal(1000, result.Count);
                    }
                }
            }.RunTest();
        }

        [Fact]
        public void ProjectionAcrossNavigation()
        {
            new TestDefinition
            {
                TestName = "Query_Simple_ProjectionAcrossNavigation_EF6",
                IterationCount = 100,
                WarmupCount = 5,
                Setup = EnsureDatabaseSetup,
                RunWithCollector = collector =>
                {
                    using (var context = new OrdersContext(_connectionString))
                    {
                        collector.Start();
                        var result = context.Orders.Select(o => new
                                {
                                    CustomerName = o.Customer.Name,
                                    OrderDate = o.Date
                                })
                            .ToList();

                        collector.Stop();
                        Assert.Equal(2000, result.Count);
                    }
                }
            }.RunTest();
        }

        private static void EnsureDatabaseSetup()
        {
            OrdersSeedData.EnsureCreated(
                _connectionString,
                productCount: 1000,
                customerCount: 1000,
                ordersPerCustomer: 2,
                linesPerOrder: 2);
        }
    }
}
