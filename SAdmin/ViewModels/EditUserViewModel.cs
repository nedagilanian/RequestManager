using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Samed.Areas.SAdmin.ViewModels
{
    public class EditUserViewModel
    {
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [Display(Name = "نام")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [Display(Name = "نام خانوادگی")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [Display(Name = "کدملی")]
        public string NationalCode { get; set; }
        // public DateTime? BirthDate { get; set; }
        [Display(Name = "شماره همراه")]
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [StringLength(maximumLength: 11, ErrorMessage = " طول مجاز {0} {1} کاراکتر است.", MinimumLength = 11)]
        [Phone]
        public string PhoneNumber { get; set; }
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [Display(Name = "نام کاربری")]
        public string UserName { get; set; }

        [Display(Name = "پست الکترونیک")]
        [EmailAddress]
        public string Email { get; set; }
        public string Id { get; set; }


    }
}
