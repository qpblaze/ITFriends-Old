using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web;
using System.Web.Mvc;

namespace Konan.Models
{
    public class CommentViewModel
    {
        public string Comment { get; set; }
    }

    public class Post
    {
        [Key]
        public string Id { get; set; }
        public string Id_A { get; set; }
        public string Title { get; set; }
        [AllowHtml]
        public string Description { get; set; }
        public string Visibility { get; set; }
        public string ImageUrl { get; set; }
        public string Input { get; set; }
        public string Output { get; set; }
        [DisplayFormat(DataFormatString = "{0: dd MMM 'at' HH:mm}",
            ApplyFormatInEditMode = true)]
        public DateTime Date { get; set; }
        [NotMapped]
        public HttpPostedFileBase File { get; set; }
        public string Id_S { get; set; }
        public string Status { get; set; }
        [ForeignKey("Id_A")]
        public virtual Account Account { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Solution> Solutions { get; set; }
    }

    public class Like
    {
        [Key]
        public string Id { get; set; }
        public string Id_A { get; set; }
        public string Id_P { get; set; }
        public string Status { get; set; }

        [ForeignKey("Id_A")]
        public virtual Account Account { get; set; }
        [ForeignKey("Id_P")]
        public virtual Post Post { get; set; }
    }

    public class Comment
    {
        [Key]
        public string Id { get; set; }
        public string Id_A { get; set; }
        public string Id_P { get; set; }
        [Column("Comment")]
        public string CommentT { get; set; }
        public string Date { get; set; }
        public string Status { get; set; }

        [ForeignKey("Id_A")]
        public virtual Account Account { get; set; }
        [ForeignKey("Id_P")]
        public virtual Post Post { get; set; }
    }
}