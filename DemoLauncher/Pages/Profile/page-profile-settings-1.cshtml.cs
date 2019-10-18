using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using Okta.Sdk;
using DemoLauncher.Interfaces;
using DemoLauncher.Services;

namespace DemoLauncher.Pages
{
    public class PageProfileModel : PageModel
    {
        /**************************
         * Global config settings *
         **************************/

        //Global Config
        private readonly IGlobalConfiguration _globalConfiguration;

        /**********************
        * Getters and Setters*
        **********************/

        //input fields from page
        [BindProperty]
        public APIDemo_DemoLauncher.Pages.Profile.OktaProfile oktaProfile { get; set; } = new APIDemo_DemoLauncher.Pages.Profile.OktaProfile();
        [BindProperty]
        public APIDemo_DemoLauncher.Pages.Profile.OktaChangePasswordProfile oktaChangePasswordProfile { get; set; } = new APIDemo_DemoLauncher.Pages.Profile.OktaChangePasswordProfile();

        /**********************
        * Alerts***************
        **********************/

        [BindProperty]
        public bool bShowSuccessMessage { get; set; } = false;
        [BindProperty]
        public string SuccessMessage { get; set; }
        [BindProperty]
        public bool bShowErrorMessage { get; set; } = false;
        [BindProperty]
        public string ErrorMessage { get; set; }

        /*********************************
         * Class setup and global config *
         *********************************/

        public PageProfileModel(IGlobalConfiguration globalConfiguration)
        {
            _globalConfiguration = globalConfiguration;
        }

        /************************
         * On Every Get**********
         ************************/

        public async Task<ActionResult> OnGet()
        {
            //Create a new okta client to get profiule data for the user
            var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
            {
                OktaDomain = _globalConfiguration.Okta_Org,
                Token = _globalConfiguration.Okta_APIToken
            });

            oktaProfile.Email = GetUserName();

            var currentUser = await client.Users.GetUserAsync(oktaProfile.Email);

            oktaProfile.FirstName = TryGetDictValues(currentUser.Profile, "firstName");
            oktaProfile.LastName = TryGetDictValues(currentUser.Profile, "lastName"); 
            oktaProfile.FullName = oktaProfile.FirstName + " " + oktaProfile.LastName;
            oktaProfile.EmployeeNumber = TryGetDictValues(currentUser.Profile, "employeeNumber");
            oktaProfile.Organization = TryGetDictValues(currentUser.Profile, "organization");
            oktaProfile.Title = TryGetDictValues(currentUser.Profile, "title");
            oktaProfile.Email = TryGetDictValues(currentUser.Profile, "email");
            oktaProfile.ProfileURL = TryGetDictValues(currentUser.Profile, "profileUrl");
            oktaProfile.MobilePhone = TryGetDictValues(currentUser.Profile, "mobilePhone");
            oktaProfile.PrimaryPhone = TryGetDictValues(currentUser.Profile, "primaryPhone");
            oktaProfile.Address = TryGetDictValues(currentUser.Profile, "streetAddress");
            oktaProfile.ProfilePictureURL = TryGetDictValues(currentUser.Profile, "profilePictureUrl");

            //If there is no profile picture url in the Okta Profile, set to default image
            if (oktaProfile.ProfilePictureURL == "")
            {
                oktaProfile.ProfilePictureURL = "../../assets/img-temp/400x450/img1.jpg";
            }
            

            //This is how we can get everything from the oauth tokens. This solution presents issues with stagnant data. Leaving here for posterity
            /*
            var stream = Request.Cookies["idToken"].ToString();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = handler.ReadToken(stream) as JwtSecurityToken;

            //Do Error handling for missing claims 
            oktaProfile.FirstName = tokenS.Claims.First(claim => claim.Type == "FirstName").Value;
            oktaProfile.LastName = tokenS.Claims.First(claim => claim.Type == "LastName").Value;
            oktaProfile.FullName = oktaProfile.FirstName + " " + oktaProfile.LastName;
            oktaProfile.EmployeeNumber = tokenS.Claims.First(claim => claim.Type == "EmployeeNumber").Value;
            oktaProfile.Organization = tokenS.Claims.First(claim => claim.Type == "Organization").Value;
            oktaProfile.Title = tokenS.Claims.First(claim => claim.Type == "Title").Value;
            oktaProfile.Email = tokenS.Claims.First(claim => claim.Type == "preferred_username").Value;
            oktaProfile.ProfileURL = tokenS.Claims.First(claim => claim.Type == "ProfileUrl").Value;
            oktaProfile.MobilePhone = tokenS.Claims.First(claim => claim.Type == "MobilePhone").Value;
            oktaProfile.PrimaryPhone = tokenS.Claims.First(claim => claim.Type == "PrimaryPhone").Value;
            oktaProfile.Address = tokenS.Claims.First(claim => claim.Type == "StreetAddress").Value;
            */

            return Page();
        }

        //Save profile changes to Okta
        public async Task<ActionResult> OnPostSave()
        {
            var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
            {
                OktaDomain = _globalConfiguration.Okta_Org,
                Token = _globalConfiguration.Okta_APIToken
            });

            oktaProfile.Email = GetUserName();

            var currentUser = await client.Users.GetUserAsync(oktaProfile.Email);

            currentUser.Profile["firstName"] = oktaProfile.FirstName;
            currentUser.Profile["lastName"] = oktaProfile.LastName;
            //currentUser.Profile["employeeNumber"] = oktaProfile.EmployeeNumber; //Not setting this
            currentUser.Profile["organization"] = oktaProfile.Organization;
            currentUser.Profile["title"] = oktaProfile.Title;
            //currentUser.Profile["email"] = oktaProfile.Email; //Not setting this
            currentUser.Profile["profileUrl"] = oktaProfile.ProfileURL;
            currentUser.Profile["mobilePhone"] = oktaProfile.MobilePhone;
            currentUser.Profile["primaryPhone"] = oktaProfile.PrimaryPhone;
            currentUser.Profile["streetAddress"] = oktaProfile.Address;

            var updateResult = await currentUser.UpdateAsync();

            //TODO: Do some error checking here

            SuccessMessage = "Your Profile Has Been Updated";
            bShowSuccessMessage = true;


            //TODO: Present result / error to user in banner
            return Redirect("page-profile-settings-1");
        }

        public ActionResult OnPostCancel()
        {
            return Redirect("page-profile-settings-1");
        }

        //Save new password to Okta
        public async Task<ActionResult> OnPostSetPassword()
        {
            if (oktaChangePasswordProfile.NewPassword == oktaChangePasswordProfile.VerifyPassword)
            {

                //Post password reset to Okta
                var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
                {
                    OktaDomain = _globalConfiguration.Okta_Org,
                    Token = _globalConfiguration.Okta_APIToken
                });

                oktaProfile.Email = GetUserName();

                var currentUser = await client.Users.GetUserAsync(oktaProfile.Email);

                ChangePasswordOptions options = new ChangePasswordOptions { CurrentPassword = oktaChangePasswordProfile.CurrentPassword, NewPassword = oktaChangePasswordProfile.NewPassword };
                var changePassResult = await currentUser.ChangePasswordAsync(options);

                //TODO: Do some error checking here
                SuccessMessage = "Your Password Has Been Changed";
                bShowSuccessMessage = true;
            }
            else
            {
                ErrorMessage= "The Passwords Dont Match";
                bShowErrorMessage = true;  
            }


            //TODO Present result / error to user in banner
            return Redirect("page-profile-settings-1");
        }

        //Get username from ID Token
        private string GetUserName()
        {
            //TODO: We need some error handling here checking if idtoken is not present. 
            var stream = Request.Cookies["idToken"].ToString();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = handler.ReadToken(stream) as JwtSecurityToken;

            //TODO: We need some error handling here checking if preferred_username is in the claim.
            string userName = tokenS.Claims.First(claim => claim.Type == "preferred_username").Value;

            return userName;
        }

        //Util to get profile values gracefully
        public string TryGetDictValues(dynamic dict, string key)
        {
            if (dict.ContainsKey(key))
            {
                return dict[key].ToString();
            }
            else
            {
                return "";
            }
        }
    }
}