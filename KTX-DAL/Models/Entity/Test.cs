﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KTX_DAL.Models.Entity
{
    [Table("Test")]
    public class Test : BaseEntity
    {
        [Column("Id")]
        public int Id { get; set; }
        [Column("Name")]
        public string Name { get; set; }
        [Column("Dess")]
        public string Dess { get; set; }
    }
}