﻿using System;
using System.ComponentModel.DataAnnotations;

namespace Raptor.Data.Models.Logging
{
    public class Log
    {
        [Key]
        public int LogId { get; set; }
        public int LogLevelId { get; set; }
        public string ShortMessage { get; set; }
        public string FullMessage { get; set; }
        public string PageUrl { get; set; }
        public DateTime DateCreatedUtc { get; set; }

        public LogLevel LogLevel {
            get { return (LogLevel)LogLevelId; }
            set { LogLevelId = LogLevelId = (int)value; }
        }
    }
}
