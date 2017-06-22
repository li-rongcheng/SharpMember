﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SharpMember.Core.Data.Models
{
    public class BranchEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class Branch : BranchEntity
    {
        public Organization Organization { get; set; }
        public virtual List<MemberProfile> Members { get; set; } = new List<MemberProfile>();
    }
}
