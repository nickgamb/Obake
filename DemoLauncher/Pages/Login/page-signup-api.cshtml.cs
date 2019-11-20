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
        public string UserNameInput { get; set; }
        [BindProperty]
        public string EmailInput { get; set; }
        [BindProperty]
        public string FirstNameInput { get; set; }
        [BindProperty]
        public string LastNameInput { get; set; }
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
            try
            {
                var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
                {
                    OktaDomain = _globalConfiguration.Okta_Org,
                    Token = helpers.GetOktaAPIToken()
                });

                // Create a user with the specified password
                //TODO: Add capcha to protect from spam. Add more profile bits like phone for mfa.
                //TODO: Do a check to make sure that the username is unique or there will be a crash
                var createUserResponse = client.Users.CreateUserAsync(new CreateUserWithPasswordOptions
                {
                    // User profile object
                    Profile = new UserProfile
                    {
                        FirstName = FirstNameInput,
                        LastName = LastNameInput,
                        Email = EmailInput,
                        Login = UserNameInput
                    },
                    Password = PasswordInput,
                    Activate = true,
                }).Result;
            }
            catch (Exception)
            {
                //TODO: Handle Errors
            }

            return Redirect("page-login-api");
        }
    }
}