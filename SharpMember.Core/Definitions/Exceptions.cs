﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMember.Core.Definitions
{
    public class MemberNameExistsException : Exception
    {
        public MemberNameExistsException(string message) : base(message) { }
    }

    public class MemberNotExistsException : Exception
    {
        public MemberNotExistsException(int invalidMemberId):base($"The member with Id {invalidMemberId} does not exist.") { }
    }

    public class MemberIdMismatchesException : Exception
    {
        public MemberIdMismatchesException(int expected, int actual)
        : base($"A member ID of {expected} is expected, but the actual value of {actual} was passed in.")
        { }
    }

    public class CommunityNotExistsException : Exception
    {
        public CommunityNotExistsException(int invalidOrgId):base($"The community with Id {invalidOrgId} does not exist.") { }
    }

    public class CommunityIdMismatchesException : Exception
    {
        public CommunityIdMismatchesException(int expected, int actual)
        : base($"A member ID of {expected} is expected, but the actual value of {actual} was passed in.")
        { }
    }
}
