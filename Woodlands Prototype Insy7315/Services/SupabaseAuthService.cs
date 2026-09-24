using Supabase;
using Supabase.Gotrue;
using SupabaseClient = Supabase.Client;
using Woodlands_Prototype_Insy7315.Models;

namespace Woodlands_Prototype_Insy7315.Services
{
    public class SupabaseAuthService
    {
        private readonly SupabaseClient _supabase;

        public SupabaseAuthService(SupabaseClient supabase)
        {
            _supabase = supabase;
        }

        public async Task<(bool Success, string? Error, SupabaseAppUser? User)> LoginAsync(
            string email,
            string password)
        {
            try
            {
                var session = await _supabase.Auth.SignInWithPassword(
                    email,
                    password);

                if (session?.User == null)
                {
                    return (false, "Invalid email or password.", null);
                }

                var userId = session!.User!.Id.ToString();

                var response = await _supabase
                    .From<SupabaseAppUser>()
                    .Where(u => u.Id == userId)
                    .Single();

                if (response == null)
                {   
                    return (
                        false,
                        "Your account profile could not be found.",
                        null);
                }

                if (!response.Active)
                {
                    return (
                        false,
                        "This account is currently inactive.",
                        null);
                }

                return (true, null, response);
            }
            catch
            {
                return (
                    false,
                    "Invalid email or password.",
                    null);
            }
        }

        public async Task<(bool Success, string? Error)> RegisterAsync(
            string fullName,
            string email,
            string password,
            string? phone)
        {
            try
            {
                var session = await _supabase.Auth.SignUp(
                    email: email,
                    password: password);

                if (session?.User == null)
                {
                    return (
                        false,
                        "Registration failed. Please try again.");
                }

                var appUser = new SupabaseAppUser
                {
                    Id = session!.User!.Id.ToString(),
                    FullName = fullName,
                    Email = email,
                    Phone = phone ?? "",
                    Role = "Customer",
                    Branch = null,
                    Active = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _supabase
                    .From<SupabaseAppUser>()
                    .Insert(appUser);

                return (true, null);
            }
            catch (Exception ex)
            {
                return (false, GetFriendlyError(ex));
            }
        }

        private static string GetFriendlyError(Exception ex)
        {
            var message = ex.Message.ToLowerInvariant();

            if (message.Contains("already registered") ||
                message.Contains("already exists"))
            {
                return "An account with this email address already exists.";
            }

            if (message.Contains("password"))
            {
                return "The password does not meet the required requirements.";
            }

            if (message.Contains("email"))
            {
                return "Please enter a valid email address.";
            }

            return "The account could not be created. Please try again.";
        }
    }
}