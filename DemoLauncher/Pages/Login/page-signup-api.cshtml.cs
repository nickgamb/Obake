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
        [BindProperty]
        public string sSNInput { get; set; }
        [BindProperty]
        public string businessNumberInput { get; set; }
        [BindProperty]
        public string accountNumberInput { get; set; }
        [BindProperty]
        public bool bShowErrorMessage { get; set; } = false;
        [BindProperty]
        public string ErrorMessage { get; set; }
        [BindProperty]
        public string widgetBackgroundImage { get; set; }

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
            //Get the background image for the signin backgound from the widget image app setting in UDP
            widgetBackgroundImage = _globalConfiguration.Widget_Background_Image;
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
            catch (Exception ex)
            {
                //TODO: Handle Errors
            }

            return Redirect("page-login-api");
        }

        /***********************************************
         * On Signup Button Post With Account Claiming *
         ***********************************************/

        //TODO: Currently no error handling and the user auto activates which means anyone can create as many accounts as desired with no verification.
        //TODO: Currently no factor enrollment is supported in API integrations. Use widget demos for now.
        public async Task<ActionResult> OnPostSignup_ClaimAccount()
        {
            try
            {
                var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
                {
                    OktaDomain = _globalConfiguration.Okta_Org,
                    Token = helpers.GetOktaAPIToken()
                });

                //Determine how we should search for existing account profile
                string profileAttributeForLookup = "";
                string profileAttributeValue = "";

                if (sSNInput != null)
                {
                    profileAttributeForLookup = "socialSecurityNumber";
                    profileAttributeValue = sSNInput;
                }
                else if (businessNumberInput != null)
                {
                    profileAttributeForLookup = "businessNumber";
                    profileAttributeValue = businessNumberInput;
                }
                else
                {
                    if (accountNumberInput != null)
                    {
                        profileAttributeForLookup = "accountNumber";
                        profileAttributeValue = accountNumberInput;
                    }
                    else
                    {
                        //TODO: Handle Error
                    }
                }

                //Search for Account Profile
                var foundUsers = await client.Users
                        .ListUsers(search: $"profile." + profileAttributeForLookup + " eq \"" + profileAttributeValue + "\"")
                        .ToArray();

                if (foundUsers.Length == 1)
                {
                    //User Found. Update Profile
                    foundUsers[0].Profile["firstName"] = FirstNameInput;
                    foundUsers[0].Profile["lastName"] = LastNameInput;
                    foundUsers[0].Profile["email"] = EmailInput;

                    //Update The Profile
                    var updateResult = await foundUsers[0].UpdateAsync();

                    // Activate Profile
                    var result = await foundUsers[0].ActivateAsync();

                    return Redirect("~/");
                }
                else if (foundUsers.Length > 1)
                {
                    //More than one account was found
                    ErrorMessage = "More than one account was found. Please contact support.";
                    bShowErrorMessage = true;
                }
                else
                {
                    //No account was found. Present Error
                    ErrorMessage = "Your account was not found. Please check your information and try again.";
                    bShowErrorMessage = true;
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = ex.Message;
                bShowErrorMessage = true;
            }

            return Page();
        }
    }
}