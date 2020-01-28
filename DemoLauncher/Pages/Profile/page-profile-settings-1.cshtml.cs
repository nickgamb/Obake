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
        ObakeHelpers helpers;

        /**********************
        * Getters and Setters*
        **********************/

        //input fields from page
        [BindProperty]
        public APIDemo_DemoLauncher.Pages.Profile.OktaProfile oktaProfile { get; set; } = new APIDemo_DemoLauncher.Pages.Profile.OktaProfile();
        [BindProperty]
        public APIDemo_DemoLauncher.Pages.Profile.OktaChangePasswordProfile oktaChangePasswordProfile { get; set; } = new APIDemo_DemoLauncher.Pages.Profile.OktaChangePasswordProfile();

        [BindProperty]
        public bool isAdmin { get; set; }
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
            helpers = new ObakeHelpers(_globalConfiguration);
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
                Token = helpers.GetOktaAPIToken()
            }) ;

            oktaProfile.Email = GetUserName();

            var currentUser = await client.Users.GetUserAsync(oktaProfile.Email);

            oktaProfile.FirstName = helpers.TryGetProfileValues(currentUser.Profile, "firstName");
            oktaProfile.LastName = helpers.TryGetProfileValues(currentUser.Profile, "lastName"); 
            oktaProfile.FullName = oktaProfile.FirstName + " " + oktaProfile.LastName;
            oktaProfile.EmployeeNumber = helpers.TryGetProfileValues(currentUser.Profile, "employeeNumber");
            oktaProfile.Organization = helpers.TryGetProfileValues(currentUser.Profile, "organization");
            oktaProfile.Title = helpers.TryGetProfileValues(currentUser.Profile, "title");
            oktaProfile.Email = helpers.TryGetProfileValues(currentUser.Profile, "email");
            oktaProfile.ProfileURL = helpers.TryGetProfileValues(currentUser.Profile, "profileUrl");
            oktaProfile.MobilePhone = helpers.TryGetProfileValues(currentUser.Profile, "mobilePhone");
            oktaProfile.PrimaryPhone = helpers.TryGetProfileValues(currentUser.Profile, "primaryPhone");
            oktaProfile.Address = helpers.TryGetProfileValues(currentUser.Profile, "streetAddress");
            oktaProfile.ProfilePictureURL = helpers.TryGetProfileValues(currentUser.Profile, "profilePictureUrl");
            oktaProfile.SSN = helpers.TryGetProfileValues(currentUser.Profile, "socialSecurityNumber");
            oktaProfile.BusinessNumber = helpers.TryGetProfileValues(currentUser.Profile, "businessNumber");
            oktaProfile.AccountNumber = helpers.TryGetProfileValues(currentUser.Profile, "accountNumber");

            //If there is no profile picture url in the Okta Profile, set to default image
            if (oktaProfile.ProfilePictureURL == "")
            {
                oktaProfile.ProfilePictureURL = "../../assets/img-temp/400x450/img0.jpg";
            }

            //Get users groups and check if in MFA
            oktaProfile.inMFA = false; //Setting to false as default
            isAdmin = false;

            var usersGroups = await currentUser.Groups.ToList();

            foreach (IGroup group in usersGroups)
            {
                //group.Profile.Name
                if (group.Profile.Name == "Obake MFA")
                {
                    oktaProfile.inMFA = true; //Setting to true if in group
                }

                if (group.Profile.Name == "Obake Admin")
                {
                    isAdmin = true; //Setting to true if in group
                }
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
                Token = helpers.GetOktaAPIToken()
            });

            oktaProfile.Email = GetUserName();

            var currentUser = await client.Users.GetUserAsync(oktaProfile.Email);

            currentUser.Profile["firstName"] = oktaProfile.FirstName;
            currentUser.Profile["lastName"] = oktaProfile.LastName;
            currentUser.Profile["employeeNumber"] = oktaProfile.EmployeeNumber; //Only Set by Admin
            currentUser.Profile["organization"] = oktaProfile.Organization;
            currentUser.Profile["title"] = oktaProfile.Title;
            //currentUser.Profile["email"] = oktaProfile.Email; //Not setting this
            currentUser.Profile["profileUrl"] = oktaProfile.ProfileURL;
            currentUser.Profile["mobilePhone"] = oktaProfile.MobilePhone;
            currentUser.Profile["primaryPhone"] = oktaProfile.PrimaryPhone;
            currentUser.Profile["streetAddress"] = oktaProfile.Address;
            currentUser.Profile["profilePictureUrl"] = oktaProfile.ProfilePictureURL; //TODO: This needs to be done via UDP file upload in V2
            currentUser.Profile["socialSecurityNumber"] = oktaProfile.SSN;
            currentUser.Profile["businessNumber"] = oktaProfile.BusinessNumber;
            currentUser.Profile["accountNumber"] = oktaProfile.AccountNumber;

            var updateResult = await currentUser.UpdateAsync();

            //Add/Remove user from MFA group depednding on inMFA checkbox
            try
            {
                if (oktaProfile.inMFA)
                {
                    // find the desired group
                    var group = await client.Groups.FirstOrDefault(x => x.Profile.Name == "Obake MFA");
                    //add user to group
                    await client.Groups.AddUserToGroupAsync(group.Id, currentUser.Id);
                }
                else
                {
                    // find the desired group
                    var group = await client.Groups.FirstOrDefault(x => x.Profile.Name == "Obake MFA");
                    //remove user to group
                    await client.Groups.RemoveGroupUserAsync(group.Id, currentUser.Id);
                }
            }
            catch (Exception ex)
            {
                //TODO: Log Errors
            }

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

        public async Task<ActionResult> OnPostDeleteProfile()
        {
            //Tell Okta to delete profile
            var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
            {
                OktaDomain = _globalConfiguration.Okta_Org,
                Token = helpers.GetOktaAPIToken()
            });

            oktaProfile.Email = GetUserName();

            var currentUser = await client.Users.GetUserAsync(oktaProfile.Email);

            // First, deactivate the user
            await currentUser.DeactivateAsync();

            // Then delete the user
            await currentUser.DeactivateOrDeleteAsync();

            // Then, remove cookies
            foreach (var cookie in Request.Cookies.Keys)
            {
                Response.Cookies.Delete(cookie);
            }

            //Log out
            return Redirect("~/");
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
                    Token = helpers.GetOktaAPIToken()
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
    }
}