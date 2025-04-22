using System.ComponentModel.DataAnnotations;
using System.Xml.Linq;

namespace Samed.Areas.SAdmin.ViewModels
{
    public class SetPasswordViewModel
    {
        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        //[StringLength(8, ErrorMessage = "{0} باید بیشتر از {1} کاراکتر باشد", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "رمز عبور")]
        public string NewPassword { get; set; }

        [Required(ErrorMessage = "وارد کردن {0} الزامی است.")]
        [DataType(DataType.Password)]
        [Display(Name = "تکرار رمز عبور")]
        [Compare("NewPassword", ErrorMessage = "{0} باید با رمز عبور مطابقت داشته باشد")]
        public string ConfirmNewPassword { get; set; }


        public string id { get; set; }
    }
}
