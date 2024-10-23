using System;
using static WpfApp1.Helpers.DatabaseHelper;
using WpfApp1.Models;

namespace WpfApp1.Helpers
{
    public static class SessionManager
    {
        public static event EventHandler LoginStateChanged;

        public static User CurrentUser { get; private set; }

        public static void Login(User user)
        {
            CurrentUser = user;
            LoginStateChanged?.Invoke(null, EventArgs.Empty);
        }

        public static void Logout()
        {
            CurrentUser = null;
            LoginStateChanged?.Invoke(null, EventArgs.Empty);
        }

        public static bool IsLoggedIn()
        {
            return CurrentUser != null;
        }
    }
}