﻿using SharpMember.Core.Data.Models.MemberSystem;
using SharpMember.Core.Data.Repositories.MemberSystem;
using SharpMember.Core.Definitions;
using SharpMember.Core.Views.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMember.Core.Views.ViewServices.CommunityViewServices
{
    public interface ICommunityCreateViewService
    {
        CommunityUpdateVm Get();
        Task<int> PostAsync(string appUserId, CommunityUpdateVm data);
    }

    public class CommunityCreateViewService : ICommunityCreateViewService
    {
        ICommunityRepository _communityRepository;
        IMemberRepository _memberRepository;
        IMemberProfileItemTemplateRepository _memberProfileItemTemplateRepository;

        public CommunityCreateViewService(
            ICommunityRepository orgRepo,
            IMemberRepository memberRepository,
            IMemberProfileItemTemplateRepository memberProfileItemTemplateRepository
        )
        {
            _communityRepository = orgRepo;
            _memberRepository = memberRepository;
            _memberProfileItemTemplateRepository = memberProfileItemTemplateRepository;
        }

        public CommunityUpdateVm Get()
        {
            CommunityUpdateVm model = new CommunityUpdateVm
            {
                ItemTemplateVMs = Enumerable.Range(0, 5).Select(i => new MemberProfileItemTemplateVm()).ToList()
            };

            return model;
        }

        public async Task<int> PostAsync(string appUserId, CommunityUpdateVm data)
        {
            Community community = new Community { Name = data.Name };
            _communityRepository.Add(community);
            await _communityRepository.CommitAsync();

            Member newMember = await _memberRepository.GenerateNewMemberWithProfileItemsAsync(community.Id, appUserId);
            newMember.CommunityRole = RoleNames.CommunityOwner;
            await _memberRepository.CommitAsync();

            var required = data.ItemTemplateVMs.Where(p => p.ItemTemplate.IsRequired == true).Select(p => p.ItemTemplate.ItemName);
            await _memberProfileItemTemplateRepository.AddTemplatesAsync(community.Id, required, true);

            var optional = data.ItemTemplateVMs.Where(p => p.ItemTemplate.IsRequired == false).Select(p => p.ItemTemplate.ItemName);
            await _memberProfileItemTemplateRepository.AddTemplatesAsync(community.Id, optional, false);

            await _memberProfileItemTemplateRepository.CommitAsync();

            return community.Id;
        }
    }
}
