using Microsoft.AspNetCore.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TextFeedAggregator.Data {
    public class UserTwitterToken {
        [Key]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccessToken { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccessTokenSecret { get; set; }

        public DateTimeOffset? LastRead { get; set; }
    }
}
