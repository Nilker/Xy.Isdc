﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;

namespace Xy.Isdc.IdentityServer.OAuth
{
    public class ResourceOwnerPasswordValidator : IResourceOwnerPasswordValidator
    {
        //private readonly IUserService _userService;

        //public ResourceOwnerPasswordValidator(IUserService userService)
        //{
        //    _userService = userService;
        //}

        public async Task ValidateAsync(ResourceOwnerPasswordValidationContext context)
        {
            context.Result=new GrantValidationResult();

            //var user = context.  await _userService.Login(context.UserName, context.Password);
            //if (user != null)
            //{
            //    var claims = new List<Claim>() { new Claim("role", "admin") }; //根据 user 对象，设置不同的 role
            //    context.Result = new GrantValidationResult(user.UserId.ToString(), OidcConstants.AuthenticationMethods.Password, claims);
            //}
        }
    }
}
