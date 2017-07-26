﻿using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using SharpMember.Core.Data.Repositories;
using SharpMember.Core.Data.Repositories.MemberSystem;
using Microsoft.Extensions.DependencyInjection;
using SharpMember.Core.Data.Models.MemberSystem;
using System.Linq;
using System.Linq.Expressions;
using SharpMember.Core;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace U.DataRepositories
{
    class LocalUtil: DependencyEnabled
    {
        public int GetExistingOrganizationId()
        {
            var repo = this.serviceProvider.GetService<IOrganizationRepository>();
            var org = repo.Add(Guid.NewGuid().ToString());
            repo.Commit();
            return org.Id;
        }

        public int GetNonexistentOrganizationId()
        {
            var repo = this.serviceProvider.GetService<IOrganizationRepository>();
            var org = repo.GetAll().OrderBy(o => o.Id).LastOrDefault();
            if(org == null)
            {
                return new Random().Next(); // return non-negative integer
            }
            else
            {
                return org.Id + new Random().Next();
            }
        }
    }

    public class MemberProfileItemTemplateRepositoryTests: DependencyEnabled
    {
        LocalUtil util = new LocalUtil();

        [Fact]
        public void TestAdd_Update_Delete_MemberProfileItemTemplate()
        {
            int existingOrgId = this.util.GetExistingOrganizationId();
            string[] itemNames = { Guid.NewGuid().ToString(), Guid.NewGuid().ToString() };

            // ======================================================================================
            // add
            var repo = this.serviceProvider.GetService<IMemberProfileItemTemplateRepository>();
            foreach(var name in itemNames)
            {
                repo.AddWithExceptionAsync(existingOrgId, name);
            }
            repo.Commit();

            // verify add
            var repoOrg = this.serviceProvider.GetService<IOrganizationRepository>();
            var readItemNames = repoOrg.GetMany(o => o.Id == existingOrgId)
                .Include(o => o.MemberProfileItemTemplates)
                .SelectMany(o => o.MemberProfileItemTemplates)
                .Select(t => t.ItemName)
                .ToList();

            Assert.Equal(2, readItemNames.Count);
            foreach(var name in itemNames)
            {
                Assert.True(readItemNames.Contains(name));
            }

            // ======================================================================================
            // update
            var repoUpdate = this.serviceProvider.GetService<IMemberProfileItemTemplateRepository>();
            var updated = repoUpdate.GetMany(t => t.OrganizationId == existingOrgId).First();
            string newItemName = $"updated-{Guid.NewGuid().ToString()}";
            updated.ItemName = newItemName;
            repoUpdate.Commit();

            // verify upate
            var repoRead = this.serviceProvider.GetService<IMemberProfileItemTemplateRepository>();
            var updateItemNames = repoUpdate.GetMany(t => t.OrganizationId == existingOrgId).Select(t => t.ItemName).ToList();
            Assert.Equal(2, updateItemNames.Count());
            Assert.True(updateItemNames.Contains(newItemName));

            // ======================================================================================
            // delete
            var repoDelete = this.serviceProvider.GetService<IMemberProfileItemTemplateRepository>();
            var deleteTarget = repoDelete.GetMany(t => t.OrganizationId == existingOrgId).Last();
            repoDelete.Delete(deleteTarget);
            repoDelete.Commit();

            // verify delete
            var repoRead2 = this.serviceProvider.GetService<IMemberProfileItemTemplateRepository>();
            var remained = repoRead2.GetMany(t => t.OrganizationId == existingOrgId).ToList();
            Assert.Equal(1, remained.Count());
        }

        [Fact]
        public async Task TestAddTo_NonexitentOrganizationId_Should_ThrowsException()
        {
            int nonExistentOrgId = this.util.GetNonexistentOrganizationId();
            var repo = this.serviceProvider.GetService<IMemberProfileItemTemplateRepository>();
            OrganizationNotExistsException ex = await Assert.ThrowsAsync<OrganizationNotExistsException>(() => repo.AddWithExceptionAsync(nonExistentOrgId, Guid.NewGuid().ToString()));
            Assert.Equal($"The organization with Id {nonExistentOrgId} does not exist.", ex.Message);
        }
    }
}
