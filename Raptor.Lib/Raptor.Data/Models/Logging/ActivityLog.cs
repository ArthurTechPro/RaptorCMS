﻿using Raptor.Data.Models.Users;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Raptor.Data.Models.Logging
{
    public class ActivityLog
    {
        [Key]
        public int ActivityLogId { get; set; }

        public int ActivityLogTypeId { get; set; }

        public string Comment { get; set; }

        [ForeignKey("Person")]
        public int PersonId { get; set; }

        public string IpAddress { get; set; }

        public DateTime DateCreatedUtc { get; set; }

        public virtual ActivityLogType ActivityLogType { get; set; }

        public virtual Person Person { get; set; }
    }
}
