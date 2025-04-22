using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Samed.Areas.SAdmin.ViewModels
{
    public class AddUserToRoleViewModel
    {
        #region Constructor

        public AddUserToRoleViewModel()
        {
            UserRoles = new List<UserRolesViewModel>();
        }

        public AddUserToRoleViewModel(int userId, IList<UserRolesViewModel> userRoles)
        {
            UserId = userId;
            UserRoles = userRoles;
        }


        #endregion


        public int? OfficeId { get; set; }
        public int UserId { get; set; }

        public IList<UserRolesViewModel> UserRoles { get; set; }
    }

    public class UserRolesViewModel
    {

        #region Constructor

        public UserRolesViewModel()
        {
        }

        public UserRolesViewModel(string roleName, string roleNameFa)
        {
            RoleName = roleName;
            RoleNameFa = roleNameFa;
        }


        #endregion

        public string RoleName { get; set; }
        public string RoleNameFa { get; set; }
        public bool IsSelected { get; set; }
    }
}
