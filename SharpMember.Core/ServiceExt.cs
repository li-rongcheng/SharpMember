﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using SharpMember.Core.Data.DataServices;
using SharpMember.Core.Services.Excel;
using SharpMember.Core.Services;
using SharpMember.Core.Data;
using NetCoreUtils.Database;
using SharpMember.Core.Definitions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SharpMember.Core.Data.Models;
using SharpMember.Core.Data.DataServices.MemberSystem;
using SharpMember.Core.Views.ViewModels;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using SharpMember.Core.Data.Models.MemberSystem;
using SharpMember.Core.Views.ViewServices.CommunityViewServices;
using SharpMember.Core.Views.ViewServices.MemberViewServices;
using SharpMember.Core.Views.ViewServices.GroupViewServices;
using SharpMember.Core.Views.ViewModels.CommunityVms;

namespace SharpMember.Core
{
    public class RepositoryReader<TEntity>
        : RepositoryRead<TEntity, ApplicationDbContext>
        where TEntity : class
    {
        public RepositoryReader(IUnitOfWork<ApplicationDbContext> unitOfWork)
            : base(unitOfWork)
        { }
    }

    public class RepositoryWriter<TEntity>
        : RepositoryWrite<TEntity, ApplicationDbContext>
        where TEntity : class
    {
        public RepositoryWriter(IUnitOfWork<ApplicationDbContext> unitOfWork)
            : base(unitOfWork)
        { }
    }

    public class Repository<TEntity>
        : Repository<TEntity, ApplicationDbContext>
        where TEntity : class
    {
        public Repository(
            IUnitOfWork<ApplicationDbContext> unitOfWork,
            IRepositoryRead<TEntity, ApplicationDbContext> repoReader,
            IRepositoryWrite<TEntity, ApplicationDbContext> repoWriter
        ) : base(unitOfWork, repoReader, repoWriter)
        { }
    }

    public static class ServiceExt
    {
        static private bool automapper_initialized = false;
        static private readonly object locker = new object();
        static private void AutoMapperConfiguration()
        {
            /* Add thread locking to prevent this exception in xunit for the latest AutoMapper:
             * Mapper already initialized. You must call Initialize once per application domain/process
             */
            lock (locker)
            {
                if (!automapper_initialized)
                {
                    Mapper.Initialize(cfg =>
                    {
                        cfg.CreateMap<Member, MemberUpdateVm>();
                        cfg.CreateMap<MemberUpdateVm, Member>();

                        cfg.CreateMap<GroupUpdateVm, Group>();
                        cfg.CreateMap<Group, GroupUpdateVm>();

                        cfg.CreateMap<CommunityUpdateVm, Community>();
                        cfg.CreateMap<Community, CommunityUpdateVm>();
                    });
                    automapper_initialized = true;
                }
            }
        }

        static private void AddRepositoryServices(this IServiceCollection services)
        {
            services.AddRepositories();

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped(typeof(IRepositoryRead<>), typeof(RepositoryReader<>));
            services.AddScoped(typeof(IRepositoryWrite<>), typeof(RepositoryWriter<>));

            //services.AddScoped<IGroupRepository, GroupRepository>();
            services.AddScoped<IMemberProfileItemService, MemberProfileItemService>();
            services.AddScoped<IMemberRepository, MemberRepository>();
            //services.AddScoped<IApplicationUserRepository, ApplicationUserRepository>();
            services.AddScoped<IMemberProfileItemTemplateRepository, MemberProfileItemTemplateRepository>();
            //services.AddScoped<IGroupMemberRelationRepository, GroupMemberRelationRepository>();
        }
        
        static private void AddServices(this IServiceCollection services)
        {
            services.AddTransient<IFullMemberPageReader, FullMemberPageReader>();
            services.AddTransient<IZjuaaaMemberExcelFileReadService, ZjuaaaMemberExcelFileReadService>();
            services.AddTransient<IAssociatedMemberPageReader, AssociatedMemberPageReader>();
            services.AddTransient<IAccountService, AccountService>();
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddTransient<IAuthorizationService, AuthorizationService>();
            services.AddTransient<ICommunityService, CommunityService>();
        }

        static private void AddViewServices(this IServiceCollection services)
        {
            services.AddTransient<IMemberCreateHandler, MemberCreateHandler>();
            services.AddTransient<IMemberEditHandler, MemberEditHandler>();

            services.AddTransient<ICommunityIndexHandler, CommunityIndexHandler>();
            services.AddTransient<ICommunityCreateHandler, CommunityCreateHandler>();
            services.AddTransient<ICommunityEditHandler, CommunityEditHandler>();

            services.AddTransient<ICommunityMembersHandler, CommunityMembersHandler>();
            services.AddTransient<ICommunityGroupsHandler, CommunityGroupsHandler>();

            services.AddTransient<IGroupCreateHandler, GroupCreateHandler>();
            services.AddTransient<IGroupEditHandler, GroupEditHandler>();
            services.AddTransient<IGroupAddMemberHandler, GroupAddMemberHandler>();
        }

        static public void AddSharpMemberCore(this IServiceCollection services, IConfiguration Configuration)
        {
            AutoMapperConfiguration();

            switch (GlobalConfigs.DatabaseType)
            {
                case eDatabaseType.Sqlite:
                    services.AddDbContext<ApplicationDbContext>(
                        options => options.UseSqlite($"Filename={DbConsts.SqliteDbFileName}")
                    );
                    break;
                case eDatabaseType.SqlServer:
                    services.AddDbContext<ApplicationDbContext>(
                        options =>
                        {
                            string migrationAssembly = "SharpMember.Migrations.SqlServer";
                            bool config_UnitTestConnectionEnabled = Configuration.GetValue<bool>("UnitTestConnectionEnabled");
                            if ( config_UnitTestConnectionEnabled == true)
                                options.UseSqlServer(
                                    Configuration.GetConnectionString("UnitTestConnection"), 
                                    sqlServerOption => sqlServerOption.MigrationsAssembly(migrationAssembly)
                                );
                            else
                                options.UseSqlServer(
                                    Configuration.GetConnectionString("DefaultConnection"),
                                    sqlServerOption => sqlServerOption.MigrationsAssembly(migrationAssembly)
                                );
                        }
                    );
                    break;
                case eDatabaseType.Postgres:
                    var postgresConnStr = Configuration.GetConnectionString("PostgresConnection");
                    Console.WriteLine($"postgresConnStr: {postgresConnStr}");
                    services.AddDbContext<ApplicationDbContext>( options =>
                        options.UseNpgsql(
                            postgresConnStr, 
                            postgresOption => postgresOption.MigrationsAssembly("SharpMember.Migrations.Postgres")
                        ));
                    break;
                default:
                    throw new Exception("Unknown database type for DbContext dependency injection");
            }

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddRepositoryServices();
            services.AddServices();
            services.AddViewServices();
        }
    }
}
