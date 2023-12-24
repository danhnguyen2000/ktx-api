using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL.Models.Entity
{
    [Table("user")]
    public class User :BaseEntity
    {

        [Column("id")]
        public int Id { get; set; }

        [Column("email")]
        public string Email { get; set; }

        [Column("password")]
        [JsonIgnore]
        public string Password { get; set; }

        [Column("is_disabled")]
        public bool IsDisabled { get; set; }

        [Column("created_date")]
        public DateTime CreatedDate { get; set; }

    }
}
