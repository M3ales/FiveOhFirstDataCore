﻿using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FiveOhFirstDataCore.Core.Account
{
    public class TrooperSignInManager : SignInManager<Trooper>
    {
        public TrooperSignInManager(UserManager<Trooper> userManager, IHttpContextAccessor contextAccessor, IUserClaimsPrincipalFactory<Trooper> claimsFactory, IOptions<IdentityOptions> optionsAccessor, ILogger<SignInManager<Trooper>> logger, IAuthenticationSchemeProvider schemes, IUserConfirmation<Trooper> confirmation)
            : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation) { }

        public override async Task<SignInResult> PasswordSignInAsync(string userName, string password, bool isPersistent, bool lockoutOnFailure)
        {
            var user = await UserManager.FindByNameAsync(userName);

            if(user is not null)
            {
                return await PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
            }
            
            return await base.PasswordSignInAsync(userName, password, isPersistent, lockoutOnFailure);
        }

        public override Task<SignInResult> PasswordSignInAsync(Trooper user, string password, bool isPersistent, bool lockoutOnFailure)
        {
            if (user.DiscordId is null || user.SteamId is null)
            {
                return Task.FromResult<SignInResult>(new TrooperSignInResult()
                {
                    RequiresAccountLinking = true,
                    TrooperId = user.Id
                });
            }

            return base.PasswordSignInAsync(user, password, isPersistent, lockoutOnFailure);
        }
    }

    public class TrooperSignInResult : SignInResult
    {
        public bool RequiresAccountLinking { get; set; }
        public int TrooperId { get; set; }
    }
}