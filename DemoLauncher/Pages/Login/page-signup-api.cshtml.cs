using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Okta.Sdk;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DemoLauncher.Interfaces;
using DemoLauncher.Services;

namespace DemoLauncher.Pages
{
    public class PageSignupApiModel : PageModel
    {
        /**************************
         * Global config settings *
         **************************/

        //Global Config
        private readonly IGlobalConfiguration _globalConfiguration;
        ObakeHelpers helpers;

        /**********************
         * Getters and Setters*
         **********************/

        //input fields from page
        [BindProperty]
        public string FullNameInput { get; set; }
        [BindProperty]
        public string EmailInput { get; set; }
        [BindProperty]
        public string PasswordInput { get; set; }

        /*********************************
        * Class setup and global config *
        *********************************/

        public PageSignupApiModel(IGlobalConfiguration globalConfiguration)
        {
            _globalConfiguration = globalConfiguration;
            helpers = new ObakeHelpers(_globalConfiguration);
        }

        /************************
         * On Every Get**********
         ************************/

        public void OnGet()
        {

        }

        /************************
         * On Signup Button Post *
         ************************/

        //TODO: Currently no error handling and the user auto activates which means anyone can create as many accounts as desired with no verification.
        //TODO: Currently no factor enrollment is supported in API integrations. Use widget demos for now.
        public ActionResult OnPostSignup()
        {
            var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
            {
                OktaDomain = _globalConfiguration.Okta_Org,
                Token = helpers.GetOktaAPIToken()
            });

            string[] fullName = FullNameInput.Split(" ");

            string firstName = fullName[0];
            string lastName = "";

            if (fullName.Length > 0)
            {
                lastName = fullName[1];
            }

            // Create a user with the specified password
            //TODO: Add capcha to protect from spam. Add more profile bits like phone for mfa.
            var createUserResponse = client.Users.CreateUserAsync(new CreateUserWithPasswordOptions
            {
                // User profile object
                Profile = new UserProfile
                {
                    FirstName = firstName,
                    LastName = lastName,
                    Email = EmailInput,
                    Login = EmailInput
                },
                Password = PasswordInput,
                Activate = true,
            }).Result;

            return Redirect("page-login-api");

        }
    }
}