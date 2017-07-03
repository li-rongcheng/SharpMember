﻿using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMember.Core.Data.Models.TaskManagement
{
    public class MilestoneEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime? DueTime { get; set; }
    }

    public class Milestone : MilestoneEntity
    {
        public virtual Project Project {get;set;} 
        public virtual List<WorkTask> WorkTasks { get; set; }
    }
}
