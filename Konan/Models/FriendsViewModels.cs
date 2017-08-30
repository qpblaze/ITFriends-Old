using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Konan.Models
{
    public class Friend
    {
        [Key]
        public string Id { get; set; }
        public string Id_A { get; set; }
        public string Id_F { get; set; }
        public string Status { get; set; }

        [ForeignKey("Id_A")]
        public Account Account { get; set; }

        [ForeignKey("Id_F")]
        public Account A_Friend { get; set; }
    }

    public class FriendRequest
    {
        [Key]
        public string Id { get; set; }
        public string Id_A { get; set; }
        public string Id_F { get; set; }
    }

    [Table("Chat")]
    public class Chat
    {
        [Key]
        public string Id { get; set; }
        public string FromId { get; set; }
        public string ToId { get; set; }
        public string Message { get; set; }
        public DateTime Date { get; set; }
    }
}