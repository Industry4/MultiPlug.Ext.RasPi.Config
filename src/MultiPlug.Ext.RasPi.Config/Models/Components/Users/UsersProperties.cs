using MultiPlug.Base;

namespace MultiPlug.Ext.RasPi.Config.Models.Components.Users
{
    public class UsersProperties : SharedProperties
    {
        public string Message { get; set; } = string.Empty;
        public bool Error { get; set; }
        public bool NotSecure { get; set; }
        public string Username { get; set; }
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
        public string NewPasswordCheck { get; set; }
    }
}
