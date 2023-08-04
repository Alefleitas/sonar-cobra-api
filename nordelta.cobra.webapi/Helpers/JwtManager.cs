using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using nordelta.cobra.webapi.Models;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace nordelta.cobra.webapi.Helpers
{

    public static class JwtManager
    {
        public static ClaimsPrincipal claims;

        public static string GenerateToken(User user, User supportUser = null) //10 años  (10 * 365 * 24 * 60)
        {
            string base64Key = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(Startup.StaticConfig["JwtKey"])
            );
            int expireMinutes = Convert.ToInt32(Startup.StaticConfig["JwtExpireMins"]);

            byte[] symmetricKey = Convert.FromBase64String(base64Key);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            DateTime now = DateTime.UtcNow;
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                        {
                            new Claim("userId",user.Id.ToString()),
                            new Claim("firstname", user.FirstName),
                            new Claim("lastname", user.LastName),
                            new Claim("userEmail", user.Email),
                            new Claim("cuit", user.Cuit.ToString()),
                            new Claim("accountNumber", user.AccountNumber ?? ""),
                            new Claim("isForeignCuit", user.IsForeignCuit.ToString().ToLower()),
                            new Claim("clientReference", user.ClientReference ?? ""),
                            new Claim("aditionalCuits", JsonConvert.SerializeObject(user.AdditionalCuits)),
                            // ToLower so property names doesn't get casted CamelCase from frontend
                            new Claim("userRoles",  JsonConvert.SerializeObject(user.Roles).ToLower()),
                             new Claim("businessUnits",  JsonConvert.SerializeObject(user.BusinessUnits).ToLower()),
                            new Claim("userBirthDate",  user.BirthDate.ToShortDateString()),
                        }),
                NotBefore = now.Subtract(new TimeSpan(0, 5, 0)),

                Expires = now.AddMinutes(Convert.ToInt32(expireMinutes)),

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(symmetricKey), SecurityAlgorithms.HmacSha256Signature)
            };

            //If support user then add claim
            if (supportUser != null)
            {
                tokenDescriptor.Subject.AddClaim(new Claim("supportUserEmail", supportUser.Email));
                tokenDescriptor.Subject.AddClaim(new Claim("supportUserId", supportUser.Id));
                tokenDescriptor.Subject.AddClaim(new Claim("supportUserName", supportUser.FirstName));
            }

            SecurityToken stoken = tokenHandler.CreateToken(tokenDescriptor);
            string token = tokenHandler.WriteToken(stoken);

            return token;
        }

        public static ClaimsPrincipal GetVerifiedPayload(string token)
        {
            try
            {
                string base64Key = Convert.ToBase64String(
                    System.Text.Encoding.UTF8.GetBytes(Startup.StaticConfig["JwtKey"])
                );
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                JwtSecurityToken jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

                if (jwtToken == null)
                {
                    return null;
                }

                byte[] symmetricKey = Convert.FromBase64String(base64Key);

                TokenValidationParameters validationParameters = new TokenValidationParameters()
                {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };

                ClaimsPrincipal payload = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken securityToken);

                claims = payload;
                return payload;
            }

            catch (Exception e)
            {
                Console.WriteLine(e);
                return null;
            }
        }
        public static dynamic GetPayloadWithoutVerification(string token)
        {
            string base64Payload = token.Split(".")[1];
            int length = base64Payload.Length;
            int multipleOf4Length = (length + 3) & ~0x3; //length + (length % 4);

            // .NET needs Base64 to be multiple of 4, and JWT omits those padding characters.
            byte[] payloadBytes = Convert.FromBase64String(base64Payload.PadRight(multipleOf4Length, '='));
            string payloadString = Encoding.UTF8.GetString(payloadBytes);

            return JsonConvert.DeserializeObject(payloadString);
        }
    }
}
