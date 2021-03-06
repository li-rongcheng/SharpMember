﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using NetCoreUtils.Database;
using SharpMember.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using U.TestEnv.TestService;
using Xunit;

namespace U.TestEnv
{
    public class ServiceProviderFixture : IDisposable
    {
        public IServiceProvider ServiceProvider { get; private set; }

        /// <summary>
        /// This method will update ServiceProvide as well.
        /// </summary>
        public T GetServiceNewScope<T>()
        {
            this.ServiceProvider = this.ServiceProvider.CreateScope().ServiceProvider;
            return this.ServiceProvider.GetService<T>();
        }

        public ServiceProviderFixture()
        {
            string projectDir = AppContext.BaseDirectory.Substring(0, AppContext.BaseDirectory.IndexOf("bin"));
            string sharpMemberDir = Path.Combine(projectDir, "../SharpMember");

            IConfigurationRoot configuration = new ConfigurationBuilder()
                .SetBasePath(sharpMemberDir)
                .AddJsonFile(TestGlobalSettings.sharpMemberJsonSettingName, optional: true, reloadOnChange: true)
                .AddJsonFile(TestGlobalSettings.sharpMemberJsonSettingNameForUnitTest, optional: true, reloadOnChange: true)
                .AddUserSecrets(userSecretsId: "aspnet-SharpMember-4C3332C6-4145-4408-BDD4-63A97039ED0D") // use project SharpMember's secret id
                .Build();

            IServiceCollection serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(c => c.AddDebug()); // or use mock: serviceCollection.AddTransient<ILogger>(f => new Mock<ILogger>().Object);
            serviceCollection.AddSharpMemberCore(configuration);
            serviceCollection.AddTransient<ICommunityTestDataProvider, CommunityTestDataProvider>();

            this.ServiceProvider = serviceCollection.BuildServiceProvider();
            this.Util = new TestUtil(this.ServiceProvider);
        }

        public TestUtil Util { get; private set; }

        public void Dispose() { }
    }

    [CollectionDefinition(nameof(ServiceProviderCollection))]
    public class ServiceProviderCollection : ICollectionFixture<ServiceProviderFixture> { }
}
