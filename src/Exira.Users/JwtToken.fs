namespace Exira.Users

module JwtToken =
    open System
    open System.IdentityModel.Tokens
    open System.Security.Claims

    let private webConfig = WebConfig()

    let private secret = Convert.FromBase64String webConfig.Web.JWT.TokenSigningKey
    let private tokenLifeTime = TimeSpan.FromMinutes (float webConfig.Web.JWT.TokenLifetimeInMinutes)
    let private tokenHandler = JwtSecurityTokenHandler()

    let createToken (claims: Claim list) =
        let token =
            tokenHandler.CreateToken(
                issuer = webConfig.Web.JWT.Issuer,
                audience = webConfig.Web.JWT.Audiences.[0], // TODO: Send along a valid audience (probably have to check incoming and check if we have it configured)
                subject = ClaimsIdentity(claims),
                expires = Nullable (DateTime.UtcNow.Add tokenLifeTime),
                signingCredentials = SigningCredentials(
                        InMemorySymmetricSecurityKey(secret),
                        "http://www.w3.org/2001/04/xmldsig-more#hmac-sha256",
                        "http://www.w3.org/2001/04/xmlenc#sha256"))

        tokenHandler.WriteToken token
