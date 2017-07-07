﻿using SharpMember.Core.Data.Models;
using SharpMember.Core.Data.RepositoryBase;
using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.Logging;
using SharpMember.Core.Data.Models.MemberSystem;
using System.Threading.Tasks;

namespace SharpMember.Core.Data.Repositories.MemberSystem
{
    public interface IMemberGroupRepository : IRepositoryBase<MemberGroup, ApplicationDbContext>
    {
        Task<MemberGroup> AddAsync(int orgId, string memberGroupName);
    }

    public class MemberGroupRepository : RepositoryBase<MemberGroup, ApplicationDbContext>, IMemberGroupRepository
    {
        private readonly IOrganizationRepository _organizationRepository;

        public MemberGroupRepository(
            IUnitOfWork<ApplicationDbContext> unitOfWork
            , ILogger logger
            , IOrganizationRepository organizationRepository
        ) : base(unitOfWork, logger)
        {
            this._organizationRepository = organizationRepository;
        }

        /// <summary>
        /// Precondition:
        /// - the organization must exist
        /// - the MemberGroup with the specified name must NOT exist
        /// </summary>
        public async Task<MemberGroup> AddAsync(int orgId, string memberGroupName)
        {
            if (!await _organizationRepository.ExistAsync(o => o.Id == orgId))
            {
                throw new OrganizationNotExistException();
            }

            if (await this.ExistAsync(m => m.Name == memberGroupName))
            {
                throw new MemberNameExistException();
            }

            return base.Add(new MemberGroup { Name = memberGroupName });
        }
    }
}
