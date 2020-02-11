using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DemoLauncher.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using Okta.Sdk;
using DemoLauncher.Services;
using Newtonsoft.Json;

namespace DemoLauncher.Pages
{
    public class HomeDefaultModel : PageModel
    {
        /**************************
        * Global config settings *
        **************************/

        //Global Config
        private readonly IGlobalConfiguration _globalConfiguration;
        ObakeHelpers helpers;

        [BindProperty]
        public string DemoLauncher_LogoUri { get; set; } //The Logo to use through out the demo
        [BindProperty]
        public string DemoLauncher_JumboImageUri { get; set; } //The jumbotron image
        [BindProperty]
        public string DemoLauncher_VideoUri { get; set; } //The jumbotron video
        [BindProperty]
        public string DemoLauncher_IntroBlurb_Title { get; set; } //The jumbotron blurb title
        [BindProperty]
        public string DemoLauncher_IntroBlurb { get; set; } //The jumbotron blurb
        [BindProperty]
        public string DemoLauncher_BodyBlurb1_Title { get; set; } //The first body blurb title
        [BindProperty]
        public string DemoLauncher_BodyBlurb1 { get; set; } //The first body blurb
        [BindProperty]
        public string DemoLauncher_BodyBlurb1_HyperLink_Text { get; set; } //The first body blurb hyperlink text
        [BindProperty]
        public string DemoLauncher_BodyBlurb1_HyperLink_Url { get; set; } //The first body blurb hyperlink url
        [BindProperty]
        public string DemoLauncher_BodyBlurb2_Title { get; set; } //The second body blurb title
        [BindProperty]
        public string DemoLauncher_BodyBlurb2 { get; set; } //The second body blurb
        [BindProperty]
        public string DemoLauncher_BodyBlurb2_HyperLink_Text { get; set; } //The second body blurb hyperlink text
        [BindProperty]
        public string DemoLauncher_BodyBlurb2_HyperLink_Url { get; set; } //The second body blurb hyperlink url
        [BindProperty]
        public string DemoLauncher_BodyBlurb3_Title { get; set; } //The third body blurb title
        [BindProperty]
        public string DemoLauncher_BodyBlurb3 { get; set; } //The third body blurb
        [BindProperty]
        public string DemoLauncher_BodyBlurb3_HyperLink_Text { get; set; } //The third body blurb hyperlink text
        [BindProperty]
        public string DemoLauncher_BodyBlurb3_HyperLink_Url { get; set; } //The third body blurb hyperlink url
        [BindProperty]
        public bool Consent_Required { get; set; } //Is user consent needed
        [BindProperty]
        public string Consent_Version { get; set; } //Is user consent needed


        /*********************************
        * Class setup and global config *
        *********************************/

        public HomeDefaultModel(IGlobalConfiguration globalConfiguration)
        {
            _globalConfiguration = globalConfiguration;
            helpers = new ObakeHelpers(_globalConfiguration);
        }

        /*************
        * View logic *
        **************/

        public async Task<ActionResult> OnGet()
        {
            //Set our config variables so that our view can use them
            DemoLauncher_LogoUri = _globalConfiguration.DemoLauncher_LogoUri; //The Logo to use through out the demo
            DemoLauncher_JumboImageUri = _globalConfiguration.DemoLauncher_JumboImageUri; //The jumbotron image
            DemoLauncher_VideoUri = _globalConfiguration.DemoLauncher_VideoUri; //The jumbotron video
            DemoLauncher_IntroBlurb_Title = _globalConfiguration.DemoLauncher_IntroBlurb_Title; //The jumbotron blurb title
            DemoLauncher_IntroBlurb = _globalConfiguration.DemoLauncher_IntroBlurb; //The jumbotron blurb
            DemoLauncher_BodyBlurb1_Title = _globalConfiguration.DemoLauncher_BodyBlurb1_Title; //The first body blurb title
            DemoLauncher_BodyBlurb1 = _globalConfiguration.DemoLauncher_BodyBlurb1; //The first body blurb
            DemoLauncher_BodyBlurb1_HyperLink_Text = _globalConfiguration.DemoLauncher_BodyBlurb1_HyperLink_Text; //The first body blurb hyperlink text
            DemoLauncher_BodyBlurb1_HyperLink_Url = _globalConfiguration.DemoLauncher_BodyBlurb1_HyperLink_Url; //The first body blurb hyperlink url
            DemoLauncher_BodyBlurb2_Title = _globalConfiguration.DemoLauncher_BodyBlurb2_Title; //The second body blurb title
            DemoLauncher_BodyBlurb2 = _globalConfiguration.DemoLauncher_BodyBlurb2; //The second body blurb
            DemoLauncher_BodyBlurb2_HyperLink_Text = _globalConfiguration.DemoLauncher_BodyBlurb2_HyperLink_Text; //The second body blurb hyperlink text
            DemoLauncher_BodyBlurb2_HyperLink_Url = _globalConfiguration.DemoLauncher_BodyBlurb2_HyperLink_Url; //The second body blurb hyperlink url
            DemoLauncher_BodyBlurb3_Title = _globalConfiguration.DemoLauncher_BodyBlurb3_Title; //The third body blurb title
            DemoLauncher_BodyBlurb3 = _globalConfiguration.DemoLauncher_BodyBlurb3; //The third body blurb
            DemoLauncher_BodyBlurb3_HyperLink_Text = _globalConfiguration.DemoLauncher_BodyBlurb3_HyperLink_Text; //The third body blurb hyperlink text
            DemoLauncher_BodyBlurb3_HyperLink_Url = _globalConfiguration.DemoLauncher_BodyBlurb3_HyperLink_Url; //The third body blurb hyperlink url
            Consent_Version = _globalConfiguration.GDPR_Consent_Version;

            //Check with Okta to see if concent is required
            Consent_Required = false;
            string userEmail = GetUserName();

            //User is logged in. Check on consent
            if (userEmail != "ERROR")
            {
                //TODO: Push this out to a function
                //Create a new okta client to get profile data for the user
                var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
                {
                    OktaDomain = _globalConfiguration.Okta_Org,
                    Token = helpers.GetOktaAPIToken()
                });

                var currentUser = await client.Users.GetUserAsync(userEmail);

                //TODO: We need some error handling here 
                var stream = helpers.TryGetProfileValues(currentUser.Profile, "consent");

                if (stream != "")
                {
                    var consentObject = JsonConvert.DeserializeObject<dynamic>(stream);

                    if (consentObject.Version != Consent_Version)
                    {
                        Consent_Required = true;
                    }

                    //If we get here Consent is not needed.
                }
                else
                {
                    Consent_Required = true;
                }
            }
            else
            {
                //TODO: Evaluate error. This happens when a user is logged out or no id token is present in cookies
            }

            return Page();
        }

        //Get username from ID Token
        private string GetUserName()
        {
            try
            {
                //TODO: We need some error handling here checking if idtoken is not present. 
                var stream = Request.Cookies["idToken"].ToString(); //broken
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(stream);
                var tokenS = handler.ReadToken(stream) as JwtSecurityToken;

                //TODO: We need some error handling here checking if preferred_username is in the claim.
                string userName = tokenS.Claims.First(claim => claim.Type == "preferred_username").Value;

                return userName;
            }
            catch (Exception ex)
            {

                //TODO: Do some error handling
                return "ERROR";
            }
            
        }

        public async Task<ActionResult> OnPostSaveConsent()
        {
            //Write Consent JSON to Okta
            //Check with Okta to see if concent is required
            string userEmail = GetUserName();

            //User is logged in. Check on consent
            if (userEmail != "ERROR")
            {
                //Create a new okta client to get profile data for the user
                var client = new OktaClient(new Okta.Sdk.Configuration.OktaClientConfiguration
                {
                    OktaDomain = _globalConfiguration.Okta_Org,
                    Token = helpers.GetOktaAPIToken()
                });

                var currentUser = await client.Users.GetUserAsync(userEmail);

                //TODO: Clean this crap up
                Consent_Version = _globalConfiguration.GDPR_Consent_Version;
                currentUser.Profile["consent"] = @"{'Version': '" + Consent_Version + "', 'Date': '" + DateTime.Now + "'}";

                var updateResult = await currentUser.UpdateAsync();

                return Redirect("~/");
            }
            else
            {
                return Redirect("~/");
            }
        }
    }
}
