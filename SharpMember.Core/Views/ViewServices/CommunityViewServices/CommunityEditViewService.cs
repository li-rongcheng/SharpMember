﻿using Microsoft.EntityFrameworkCore;
using SharpMember.Core.Data.Repositories.MemberSystem;
using SharpMember.Core.Views.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMember.Core.Views.ViewServices.CommunityViewServices
{
    public interface ICommunityEditViewService
    {
        CommunityUpdateVM Get(int commId);
        Task PostAsync(CommunityUpdateVM data);
    }

    public class CommunityEditViewService : ICommunityEditViewService
    {
        ICommunityRepository _communityRepository;
        IMemberProfileItemTemplateRepository _memberProfileItemTemplateRepository;

        public CommunityEditViewService(
            ICommunityRepository orgRepo,
            IMemberRepository memberRepository,
            IMemberProfileItemTemplateRepository memberProfileItemTemplateRepository
        )
        {
            _communityRepository = orgRepo;
            _memberProfileItemTemplateRepository = memberProfileItemTemplateRepository;
        }

        public CommunityUpdateVM Get(int commId)
        {
            var community = _communityRepository.GetMany(c => c.Id == commId).Include(c => c.MemberProfileItemTemplates).Single();

            CommunityUpdateVM result = community.ConvertToCommunityUpdateVM();
            result.ItemTemplateVMs = community.MemberProfileItemTemplates.Select(x => new MemberProfileItemTemplateVM { ItemTemplate = x}).ToList();

            return result;
        }

        /// <summary>
        /// Unit test: <see cref="U.ViewServices.CommunityViewServiceTests.Community_edit_view_service"/>
        /// </summary>
        public async Task PostAsync(CommunityUpdateVM data)
        {
            _communityRepository.Update(data.ConvertToCommunityWithoutNavProp());
            await _communityRepository.CommitAsync();

            _memberProfileItemTemplateRepository.DeleteRange(data.ItemTemplateVMs.Where(x => x.Delete).Select(x => x.ItemTemplate).ToList());
            _memberProfileItemTemplateRepository.AddOrUpdateItemTemplates(data.Id, data.ItemTemplateVMs.Where(x => !x.Delete).Select(x => x.ItemTemplate).ToList());

            await _communityRepository.CommitAsync();
        }
    }
}
