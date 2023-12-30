namespace API.Routes;

public static class AppRoutes
{
    public static class Authentication
    {
        public const string Base = "auth";

        public const string Register = Base + "/register";

        public const string ConfirmEmail = Base + "/confirm-email";

        public const string Login = Base + "/login";

        public const string Logout = Base + "/logout";

        public const string SignInExternal = Base + "/signing/{provider}";

        public const string ExternalLogin = Base + "/login/{provider}";

        public const string ChangePassword = Base + "/change-password";

        public const string RefreshToken = Base + "/refresh-token";
    }

    public static class Profile
    {
        public const string Base = "profile";

        public const string Me = Base + "/me";

        public const string Update = Base + "/update";
    }
}
