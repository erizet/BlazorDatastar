using Microsoft.AspNetCore.Http;
using System;

namespace BlazorDatastar.Utils
{
    public static class CookieHelper
    {
        private const string UserIdCookieName = "UserId";

        public static string GetOrCreateUserId(HttpContext context)
        {
            string userId;
            if (string.IsNullOrEmpty(context.Session.GetString(UserIdCookieName)))
            {
                userId = Guid.NewGuid().ToString();
                context.Session.SetString(UserIdCookieName, userId);
            }
            else
            {
                userId = context.Session.GetString(UserIdCookieName) ?? string.Empty;
            }
            return userId;
        }
    }
}
