﻿using Microsoft.Extensions.Logging;
using SharpMember.Core.Data.Models;
using SharpMember.Core.Data.RepositoryBase;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Linq.Expressions;
using SharpMember.Core.Data.Models.MemberSystem;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using SharpMember.Core.Definitions;

namespace SharpMember.Core.Data.Repositories.MemberSystem
{
    public interface IMemberRepository : IRepositoryBase<Member, ApplicationDbContext>
    {
        int GetNextUnassignedMemberNumber(int commId);
        IQueryable<Member> GetByMemberNumber(int commId, int memberNumber);
        IQueryable<Member> GetByCommunity(int commId);
        Task<Member> GenerateNewMemberWithProfileItemsAsync(int commId, string appUserId);
        Task<int> AssignMemberNubmerAsync(int memberId, int nextMemberNumber);
    }

    public class MemberRepository : RepositoryBase<Member, ApplicationDbContext>, IMemberRepository
    {
        public MemberRepository(IUnitOfWork<ApplicationDbContext> unitOfWork, ILogger<MemberRepository> logger) : base(unitOfWork, logger)
        { }

        public override Member Add(Member entity)
        {
            if (null == this.UnitOfWork.Context.Communities.Find(entity.CommunityId))
            {
                throw new CommunityNotExistsException(entity.CommunityId);
            }
            return base.Add(entity);
        }

        public int GetNextUnassignedMemberNumber(int commId)
        {
            int nextMemberNumber = 1;
            var member = this.GetMany(m => m.CommunityId == commId).OrderBy(m => m.MemberNumber).LastOrDefault();
            if(member != null)
            {
                nextMemberNumber = member.MemberNumber + 1;
            }
            return nextMemberNumber;
        }

        /// <summary>
        /// Try to assign the <paramref name="nextMemberNumber"/> to the relevant member.
        /// 
        ///     * If <paramref name="nextMemberNumber"/> is zero or an negative value, a new
        ///       <paramref name="nextMemberNumber"/> will be queried from the database.
        ///       
        ///     * After saving the change to the database, the method will check if there is
        ///       a duplication and try to resolve it if a duplication is found.
        ///       
        /// </summary>
        /// <returns>The successfully assigned member number.</returns>
        public async Task<int> AssignMemberNubmerAsync(int memberId, int nextMemberNumber)
        {
            var member = this.GetById(memberId);
            
            if(nextMemberNumber <= 0)
            {
                nextMemberNumber = this.GetNextUnassignedMemberNumber(member.CommunityId);
            }
            
            if(member.MemberNumber <= 0)
            {
                member.MemberNumber = nextMemberNumber;
            }

            await this.CommitAsync();

            // check if there is a duplication
            while(this.GetByMemberNumber(member.CommunityId, nextMemberNumber).Count() > 1)
            {
                nextMemberNumber = await AssignMemberNubmerAsync(memberId, 0);
                member.MemberNumber = nextMemberNumber;
                await this.CommitAsync();
            }

            return nextMemberNumber;
        }

        public async Task<Member> GenerateNewMemberWithProfileItemsAsync(int commId, string appUserId)
        {
            if (null == this.UnitOfWork.Context.Communities.Find(commId))
            {
                throw new CommunityNotExistsException(commId);
            }

            var memberProfileItems = await this.UnitOfWork.Context.MemberProfileItemTemplates
                .Where(t => t.CommunityId == commId)
                .Select(t => new MemberProfileItem { MemberProfileItemTemplateId = t.Id })
                .ToListAsync();

            Member returned = new Member { MemberProfileItems = memberProfileItems, CommunityId = commId, ApplicationUserId = appUserId};

            return returned;
        }

        public IQueryable<Member> GetByMemberNumber(int orgId, int memberNumber)
        {
            return this.GetMany(m => m.MemberNumber == memberNumber && m.CommunityId == orgId);
        }

        public IQueryable<Member> GetByCommunity(int commId)
        {
            return this.GetMany(m => m.CommunityId == commId);
        }

       
    }
}
