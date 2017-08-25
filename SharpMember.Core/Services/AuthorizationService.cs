﻿using SharpMember.Core.Data.Repositories.MemberSystem;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using SharpMember.Core.Data.Models.MemberSystem;

namespace SharpMember.Core.Services
{
    public interface IAuthorizationService
    {
        void UpdateCommunityRole(int memberId, string roleName);
        void RemoveCommunityRole(int memberId);
        void UpdateGroupRole(int memberId, int groupId, string roleName);
        void RemoveGroupRole(int memberId, int groupId);
    }

    public class AuthorizationService : IAuthorizationService
    {
        IMemberRepository _memberRepo;
        IGroupMemberRelationRepository _memberGroupRoleRelationRepository;

        public AuthorizationService(IMemberRepository memberRepo, IGroupMemberRelationRepository memberGroupRoleRelationRepository)
        {
            this._memberRepo = memberRepo;
            this._memberGroupRoleRelationRepository = memberGroupRoleRelationRepository;
        }

        public void UpdateCommunityRole(int memberId, string roleName)
        {
            _memberRepo.GetById(memberId).CommunityRole = roleName;
        }

        public void RemoveCommunityRole(int memberId)
        {
            _memberRepo.GetById(memberId).CommunityRole = "";
        }

        public void UpdateGroupRole(int memberId, int groupId, string roleName)
        {
            var relation = _memberGroupRoleRelationRepository.GetMany(m => m.MemberId == memberId && m.GroupId == groupId).SingleOrDefault();

            if(relation != null)
            {
                relation.GroupRole = roleName;
            }
            else
            {
                _memberGroupRoleRelationRepository.Add(new GroupMemberRelation { MemberId = memberId, GroupId = groupId, GroupRole = roleName });
            }
        }

        public void RemoveGroupRole(int memberId, int groupId)
        {
            _memberGroupRoleRelationRepository.Delete(m => m.MemberId == memberId && m.GroupId == groupId);
        }
    }
}
