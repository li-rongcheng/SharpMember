﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpMember.Core.Definitions
{
    static public class GlobalConsts
    {
        public const string SqliteDbFileName = "Members.sqlite";

        private const string AuthRoleCommunityManagerOnly = "CommunityManager";

        public const string AuthRoleAdmin = "Admin";
        public const string AuthRoleMemberOnly = "MemberOnly";
        public const string AuthRoleCommunityManager = AuthRoleAdmin + "," + AuthRoleCommunityManagerOnly;
    }
}
