﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SharpMember.Core.Data.Models.MemberSystem
{
    public class MemberProfileItemEntity
    {
        public int Id { get; set; }
        public string ItemName { get; set; }
        public string ItemType { get; set; }    // for validation, e.g. date, int, string
        public string ItemValue { get; set; }
    }

    public class MemberProfileItem : MemberProfileItemEntity
    {
        [ForeignKey(nameof(MemberProfileId))]
        public virtual MemberProfile MemberProfile { get; set; }
        public int MemberProfileId { get; set; }
    }
}
