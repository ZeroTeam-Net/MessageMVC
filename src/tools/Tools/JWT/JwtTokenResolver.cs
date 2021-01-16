using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using ZeroTeam.MessageMVC.Context;

namespace ZeroTeam.MessageMVC.Tools
{
    /// <summary>
    /// API扩展功能
    /// </summary>
    public class JwtTokenResolver : ITokenResolver
    {
        IUser ITokenResolver.TokenToUser(string token)
        {
            var (success, claims) = CheckJwt(token);
            if (!success)
                return null;

            var user = DependencyHelper.GetService<IUser>();
            foreach (var item in claims)
            {
                user[item.Type]= item.Value;
            }
            return user;
        }

        /// <summary>
        /// 校验参数
        /// </summary>
        static readonly TokenValidationParameters tokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(ToolsOption.Instance.JwtAppSecretByte),

            ValidateIssuer = false,
            //ValidIssuer = PermissionOption.Instance.JwtIssue,

            ValidateAudience = false,
            //ValidAudience = PermissionOption.Instance.JwtIssue,

            ValidateLifetime = false,
            //ClockSkew = TimeSpan.FromMinutes(5)
        };

        /// <summary>
        /// JWT校验
        /// </summary>
        /// <param name="jwt"></param>
        static (bool state, IEnumerable<Claim> claims) CheckJwt(string jwt)
        {
            if (string.IsNullOrEmpty(jwt))
                return (false, null);
            try
            {
                jwt = jwt.Split(new char[] { ' ', '&' },StringSplitOptions.RemoveEmptyEntries).Last();
                var handler = new JwtSecurityTokenHandler();

                handler.ValidateToken(jwt, tokenValidationParameters, out SecurityToken tk);
                var sk = (JwtSecurityToken)tk;
                var type = sk.Claims.FirstOrDefault(p => p.Type == ZeroTeamJwtClaim.TokenType);
                if (type == null)
                    return (false, null);
                return (true, sk.Claims);
            }
            catch(Exception ex)
            {
                DependencyScope.Logger.Exception(ex);
                return (false, null);
            }
        }

    }
}