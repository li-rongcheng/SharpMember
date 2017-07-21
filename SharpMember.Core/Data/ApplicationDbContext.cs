﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharpMember.Core.Data.Models;
using SharpMember.Global;
using System.Data.SqlClient;
using SharpMember.Utils;
using SharpMember.Core.Global;
using SharpMember.Core.Data.Models.MemberSystem;
using SharpMember.Core.Data.Models.ActivitySystem;
using SharpMember.Core.Data.Models.TaskSystem;
using Microsoft.EntityFrameworkCore.Metadata;

namespace SharpMember.Core.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<GlobalSettings> GlobalSettings { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<MemberGroup> MemberGroups { get; set; }
        public DbSet<MemberMemberGroupRelation> MemberMemberGroupRelation { get; set; }
        public DbSet<MemberProfileItemTemplate> MemberProfileItemTemplates { get; set; }
        public DbSet<MemberProfileItem> MemberProfileItems { get; set; }
        public DbSet<Organization> Organizations { get; set; }
        //public DbSet<ClubEvent> ClubEvent { get; set; }
        //public DbSet<Club> Clubs { get; set; }

        private static DbContextOptions<ApplicationDbContext> GetOptionsFromConnectionString(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString)) connectionString = $"Filename={GlobalConsts.SqliteDbFileName}";

            var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();

            if (GlobalConfigs.DatabaseType == eDatabaseType.Sqlite)
            {
                optionsBuilder.UseSqlite(connectionString);
            }
            else if (GlobalConfigs.DatabaseType == eDatabaseType.SqlServer)
            {
                optionsBuilder.UseSqlServer(new SqlConnection(connectionString));
            }

            return optionsBuilder.Options;
        }

        /**
         * The following constructor will cause mvc scaffolding to throw out exception of "Unable to resolve service for type 'System.String'"
         * when choose "MVC Controller with read/write actions"
         */
        //public ApplicationDbContext(string connectionString) : base(GetOptionsFromConnectionString(connectionString)) { }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            //builder.Entity<ClubMemberRelation>().HasKey(c => new { c.ClubId, c.MemberId });
            //builder.Entity<WorkTaskLabelRelation>().HasKey(w => new { w.TaskLabelId, w.WorkTaskId });
            builder.Entity<MemberMemberGroupRelation>().HasKey(m => new { m.MemberId, m.MemberGroupId });

            //builder.Entity<Member>().HasMany(m => m.MemberProfileItems).WithOne(m => m.Member).OnDelete(DeleteBehavior.Cascade);
            //builder.Entity<Member>().HasMany(m => m.MemberMemberGroupRelations).WithOne(m => m.Member).OnDelete(DeleteBehavior.Cascade);
            //builder.Entity<MemberGroup>().HasMany(m => m.MemberMemberGroupRelations).WithOne(m => m.MemberGroup).OnDelete(DeleteBehavior.Cascade);
            //builder.Entity<Organization>().HasMany(o => o.MemberGroups).WithOne(m => m.Organization).OnDelete(DeleteBehavior.Cascade);
            //builder.Entity<Organization>().HasMany(o => o.MemberProfileItemTemplates).WithOne(m => m.Organization).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<MemberMemberGroupRelation>().HasOne(m => m.MemberGroup).WithMany(m => m.MemberMemberGroupRelations).OnDelete(DeleteBehavior.Restrict); // make sure the MemberGroup specified by MemberGroupId will not be deleted
            builder.Entity<MemberMemberGroupRelation>().HasOne(m => m.Member).WithMany(m => m.MemberMemberGroupRelations).OnDelete(DeleteBehavior.Restrict);    // make sure the member specified by MemberId will not be deleted
        }
    }
}
