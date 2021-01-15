using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Scv.Db.Models
{
    public class MigrationHistory
    {
        [Key]
        public string MigrationId { get; set; }
        public string ProductVersion { get; set; }
    }
}
