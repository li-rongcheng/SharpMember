﻿using SharpMember.Core.Data.Models.TaskSystem;
using System;
using System.Collections.Generic;
using System.Text;

namespace SharpMember.Core.Data.Models.TaskSystem
{
    public class TaskCommentEntity
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime DateTime { get; set; }
    }

    public class TaskComment : TaskCommentEntity
    {
        public virtual WorkTask WorkTask { get; set; }
    }
}
