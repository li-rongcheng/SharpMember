﻿using AutoMapper;
using SharpMember.Core.Data.Models.MemberSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpMember.Core.Views.ViewModels
{
    public class CommunityMembersVm
    {
        public int CommunityId { get; set; }
        public List<MemberItemVm> MemberItemVms { get; set; } = new List<MemberItemVm>();
    }
}
