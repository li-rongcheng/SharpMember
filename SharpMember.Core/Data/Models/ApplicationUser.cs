﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace SharpMember.Core.Data.Models
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUser : IdentityUser
    {
        public virtual List<MemberProfile> MemberProfiles { get; set; } = new List<MemberProfile>();
        public virtual List<WorkTask> WorkTasks { get; set; } = new List<WorkTask>();   // private tasks
    }
}
