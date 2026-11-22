using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BikeShop.Helpers
{
    public static class CurrentUser
    {
        public static int Id { get; set; }
        public static string Fio { get; set; }
        public static string Login { get; set; }
        public static string RoleName { get; set; }
        public static int RoleId { get; set; }

        public static bool IsGuest => RoleId == 1;
        public static bool IsClient => RoleId == 2;
        public static bool IsManager => RoleId == 3;
        public static bool IsAdmin => RoleId == 4;

        public static void Clear()
        {
            Id = 0; Fio = null; Login = null; RoleName = null; RoleId = 0;
        }
    }
 }
