using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL.Models.Entity
{
    [Table("users")]
    public class User :BaseEntity
    {

        [Column("id")]
        public int Id { get; set; }

        [Column("username")]
        public string Username { get; set; }

        [Column("password")]
        [JsonIgnore]
        public string Password { get; set; }
        [Column("full_name")]
        public string FullName { get; set; }
        [Column("phone_number")]
        public string PhoneNumber { get; set; }
        [Column("is_disabled")]
        public bool IsDisabled { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; }

    }
}
