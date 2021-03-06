﻿using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextFeedAggregator.Data {
    public class UserMastodonToken {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Host { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccessToken { get; set; }

        public DateTimeOffset? LastRead { get; set; }
    }
}
