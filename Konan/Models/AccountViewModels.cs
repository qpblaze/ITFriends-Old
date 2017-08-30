using Konan.Context;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace Konan.Models
{
    public class Account
    {
        [Key]
        public string Id { get; set; }
        public string Email { get; set; }
        public int Points { get; set; }
        public string Password { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ImageUrl { get; set; }
        public string ResetPassword { get; set; }

        [NotMapped]
        public string ConnectionId { get; set; }

        public ICollection<Post> Posts { get; set; }
        public ICollection<Like> Likes { get; set; }
        public ICollection<Solution> Solutions { get; set; }
        public ICollection<Comment> Comments { get; set; }

        public static bool ValidEmail(string email)
        {
            using(KonanDBContext _dc = new KonanDBContext())
            {
                var account = _dc.Accounts.Where(e => e.Email == email).FirstOrDefault();
                if(account == null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool ValidAccount(SignInViewModel model)
        {
            using (KonanDBContext _dc = new KonanDBContext())
            {
                var password = PasswordHash(model.Password);
                var account = _dc.Accounts.Where(e => e.Email == model.Email && e.Password == password).FirstOrDefault();
                if (account == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public static string PasswordHash(string password)
        {
            var hash = (new SHA1Managed()).ComputeHash(Encoding.UTF8.GetBytes(password));
            return string.Join("", hash.Select(b => b.ToString("x2")).ToArray());
        }
    }

    public class EditProfileViewModel
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
        public string ImageUrl { get; set; }
        public HttpPostedFileBase File { get; set; }
    }

    public class SignInViewModel
    {
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public class RecoverPasswordModel
    {
        public string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        public string NewPassword { get; set; }
        public string ConfirmNewPassword { get; set; }
    }

    public class SignUpViewModel
    {
        public string Email { get; set; }

        [DataType(DataType.Password)]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; }

        public string FirstName { get; set; }

        public string LastName { get; set; }
        
        public string ImageUrl { get; set; }
    } 
}