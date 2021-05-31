using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scv.Db.Models
{
    public class Audit
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public DateTimeOffset Created { get; set; }
        public string IpAddress { get; set; }
        public string UserId { get; set; }
        public string Path { get; set; }
        public string Action { get; set; }
        [Column(TypeName = "jsonb")]
        public string JsonBody { get; set; }
        public string ResponseCode { get; set; }
    }
}
