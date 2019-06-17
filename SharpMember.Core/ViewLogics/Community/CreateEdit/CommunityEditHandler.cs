﻿using Microsoft.EntityFrameworkCore;
using NetCoreUtils.Database;
using SharpMember.Core.Data.Models.MemberSystem;
using SharpMember.Core.Data.DataServices.MemberSystem;
using SharpMember.Core.Views.ViewModels;
using SharpMember.Core.Views.ViewModels.CommunityVms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharpMember.Core.Views.ViewServices.CommunityViewServices
{
    public interface ICommunityEditHandler
    {
        CommunityUpdateVm Get(int commId, int addMore);
        Task PostAsync(CommunityUpdateVm data);
    }

    public class CommunityEditHandler : ICommunityEditHandler
    {
        IRepository<Community> _communityRepository;
        IRepository<MemberProfileItemTemplate> _memberProfileItemTemplateRepo;
        IMemberProfileItemTemplateRepository _memberProfileItemTemplateRepository;

        public CommunityEditHandler(
            IRepository<Community> orgRepo,
            IRepository<MemberProfileItemTemplate> memberProfileItemTemplateRepo,
            IMemberProfileItemTemplateRepository memberProfileItemTemplateRepository
        )
        {
            _communityRepository = orgRepo;
            _memberProfileItemTemplateRepo = memberProfileItemTemplateRepo;
            _memberProfileItemTemplateRepository = memberProfileItemTemplateRepository;
        }

        public CommunityUpdateVm Get(int commId, int addMore)
        {
            var community = _communityRepository.Query(c => c.Id == commId).Include(c => c.MemberProfileItemTemplates).Single();

            CommunityUpdateVm result = community.ConvertToCommunityUpdateVM();
            result.ItemTemplateVMs = community.MemberProfileItemTemplates.Select(x => new MemberProfileItemTemplateVm { ItemTemplate = x}).ToList();

            for(int i = 0; i < addMore; i++)
            {
                result.ItemTemplateVMs.Add(new MemberProfileItemTemplateVm());
            }

            return result;
        }

        /// <summary>
        /// Unit test: <see cref="U.ViewServices.CommunityViewServiceTests.Community_edit_view_service"/>
        /// </summary>
        public async Task PostAsync(CommunityUpdateVm data)
        {
            _communityRepository.Update(new Community().CopyFrom(data));
            await _communityRepository.CommitAsync();

            _memberProfileItemTemplateRepo.RemoveRange(data.ItemTemplateVMs.Where(x => x.Delete).Select(x => x.ItemTemplate).ToList());
            _memberProfileItemTemplateRepository.AddOrUpdateItemTemplates(data.Id, data.ItemTemplateVMs.Where(x => !x.Delete && !string.IsNullOrWhiteSpace(x.ItemTemplate.ItemName)).Select(x => x.ItemTemplate).ToList());

            await _communityRepository.CommitAsync();
        }
    }
}
