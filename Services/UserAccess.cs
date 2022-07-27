using AuthFirst.Data;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace AuthFirst.Services
{
    public class UserAccess
    {
        private readonly AuthDbContext _context;
        public UserAccess(AuthDbContext context)
        {
            _context = context;
        }

        internal AppUser GetUserByExternalProvider(string provider, string nameIdentifier)
        {
            var appUser = _context.AppUsers
                .Where(e => e.Provider == provider)
                .Where(e => e.NameIdentifier == nameIdentifier).FirstOrDefault();

            return appUser;
        }

        internal AppUser GetUserById(int id)
        {
            var appUser = _context.AppUsers.Find(id);

            return appUser;
        }

        internal AppUser AddUser(List<Claim> claims, string provider)
        {
            var appUser = new AppUser();

            appUser.Provider = provider;
            appUser.NameIdentifier = claims.GetClaims(ClaimTypes.NameIdentifier);
            appUser.Username = claims.GetClaims("username");
            appUser.Firstname = claims.GetClaims(ClaimTypes.GivenName);
            appUser.Lastname = claims.GetClaims(ClaimTypes.Surname);
            if(appUser.Firstname == null)
            {
                appUser.Firstname = claims.GetClaims("name").Split(" ")[0];
                appUser.Lastname = claims.GetClaims("name").Split(" ")[1];
            }
            appUser.Email = claims.GetClaims(ClaimTypes.Email);
            appUser.Mobile = claims.GetClaims(ClaimTypes.MobilePhone);

            var entity = _context.AppUsers.Add(appUser);
            _context.SaveChanges();
            return entity.Entity;
        }

        internal bool TryValidateUser(string username, string password, out List<Claim> claims)
        {
            claims = new List<Claim>();
            var appUser = _context.AppUsers
                .Where(e => e.Username == username)
                .Where(e => e.Password == password).FirstOrDefault();

            if(appUser is null)
            {
                return false;
            }
            else
            {
                claims.Add(new Claim(ClaimTypes.NameIdentifier, username));
                claims.Add(new Claim("username", username));
                claims.Add(new Claim(ClaimTypes.GivenName, appUser.Firstname));
                claims.Add(new Claim(ClaimTypes.Surname, appUser.Lastname));
                claims.Add(new Claim(ClaimTypes.Email, appUser.Email));
                claims.Add(new Claim(ClaimTypes.MobilePhone, appUser.Mobile));

                foreach(var r in appUser.RoleList)
                {
                    claims.Add(new Claim(ClaimTypes.Role, r));
                }
            }

            return true;
        }
    }

    public static class Extensions
    {
        public static string GetClaims(this List<Claim> claims, string name)
        {
            return claims.FirstOrDefault(t => t.Type == name)?.Value;
        }
    }
}
