﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace SharpMember.Core.Data.Models
{
    public class MemberProfileEntity
    {
        public int Id { get; set; } // some members may not have been assigned a member number, so an Id field is still required
        public int MemberNumber { get; set; }
        public bool Renewed { get; set; }
        public DateTime? RegisterDate { get; set; }
        public DateTime? CeaseDate { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public string Remarks { get; set; }
    }

    public class MemberProfile : MemberProfileEntity
    {
        public virtual Branch Branch { get; set; }
        public virtual ApplicationUser ApplicationUser { get; set; }

        public virtual List<ClubMemberProfileRelation> ClubMemberProfileRelations { get; set; } = new List<ClubMemberProfileRelation>();

        public virtual List<Club> Clubs { get; set; } = new List<Club>();

        [InverseProperty(nameof(WorkTask.WorkTaskOwner))]
        public virtual List<WorkTask> OwnedWorkTasks { get; set; } = new List<WorkTask>();

        [InverseProperty(nameof(WorkTask.WorkTaskCreator))]
        public virtual List<WorkTask> CreatedWorkTasks { get; set; } = new List<WorkTask>();
    }
}
