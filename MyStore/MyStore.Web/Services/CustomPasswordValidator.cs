using Microsoft.AspNetCore.Identity;

namespace MyStore.Web.Services
{
    public class CustomPasswordValidator<TUser> : IPasswordValidator<TUser> where TUser : class
    {
        public Task<IdentityResult> ValidateAsync(UserManager<TUser> manager, TUser user, string password)
        {
            var errors = new List<IdentityError>();

            if (password.Length > 64)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordTooLong",
                    Description = "The password must not exceed 64 characters."
                });
            }

            int categories = 0;
            if (password.Any(char.IsLower)) categories++;
            if (password.Any(char.IsUpper)) categories++;
            if (password.Any(char.IsDigit)) categories++;
            if (password.Any(ch => !char.IsLetterOrDigit(ch))) categories++;

            if (categories < 2)
            {
                errors.Add(new IdentityError
                {
                    Code = "PasswordComplexity",
                    Description = "The password must contain at least 2 of the following 4 types: uppercase letters, lowercase letters, numbers, and special characters."
                });
            }

            return Task.FromResult(errors.Any() ? IdentityResult.Failed(errors.ToArray()) : IdentityResult.Success);
        }
    }
}
