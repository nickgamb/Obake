using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Okta.Auth.Sdk;
using Okta.Sdk.Abstractions.Configuration;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using DemoLauncher.Interfaces;
using DemoLauncher.Services;

namespace DemoLauncher.Pages
{
    public class PageLoginApiModel : PageModel
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
        public string UserName { get; set; }
        public string Password { get; set; }

        [ViewData]
        public string StateToken { get; set; } = "none";

        //Factor dropdown list and storage to bind selected value
        [BindProperty]
        public string SelectedFactor { get; set; }
        public SelectList FactorList { get; set; }

        //verify text box
        [BindProperty]
        public string Verify { get; set; }

        /***************************
         * View element visibility *
         ***************************/

        public bool bShowFactors { get; set; } = false;
        public bool bShowVerify { get; set; } = false;
        public bool bShowUsername { get; set; } = false;
        public bool bShowPassword { get; set; } = false;

        /*********************************
         * Class setup and global config *
         *********************************/

        public PageLoginApiModel(IGlobalConfiguration globalConfiguration)
        {
            _globalConfiguration = globalConfiguration;
        }

        /************************
         * On Every Get**********
         ************************/
        public void OnGet()
        {
            var sessionName = new Byte[20];
            if (!HttpContext.Session.TryGetValue("StateToken", out sessionName))
            {
                bShowUsername = true;
            }
        }
        public void OnPost()
        {
        }

        /************************
         * On Login Button Post *
         ************************/
        //TODO: Currently no factor enrollment is supported in API integrations. Use widget demos for now.
        //Login button post action
        public ActionResult OnPostLogin()
        {
            try
            {
                //Store our auth result response
                AuthenticationResponse authResponse = null;

                //Do a submit and then check if continued polling is required. 
                do
                {
                    authResponse = SubmitChallenge();
                    string test = "";
                }
                while (authResponse.AuthenticationStatus == "MFA_CHALLENGE");

                //Handle all result auth states
                switch (authResponse.AuthenticationStatus)
                {
                    //Auth process started. Present factors
                    case "UNAUTHENTICATED":
                        PopulateFactors(authResponse.Embedded.GetProperty<dynamic>("factors"));//Send factor list to populate dropdown
                        SetStateToken(authResponse.StateToken);
                        return Page();
                    case "PASSWORD_WARN":
                        //TODO: Warn the user if their password is close to expiration 
                        //Getting close to expire, ask user to reset
                        return Page();
                    case "PASSWORD_EXPIRED":
                        //TODO: Support expired passwords
                        //Password is expired, force change password
                        return Page();
                    case "RECOVERY":
                        //TODO: Support account recovery
                        //Ask user password recovery question
                        return Page();
                    case "RECOVERY_CHALLENGE":
                        //Submit answer to recovery question
                        return Page();
                    case "PASSWORD_RESET":
                        //TODO: Support passowrd reset
                        //Submit new password
                        return Page();
                    case "LOCKED_OUT":
                        //TODO: Support locked out accounts
                        //Show error or support self service reecovery
                        return Page();
                    case "MFA_ENROLL":
                        //TODO: Support Enrollment of new factos
                        //Tell user tht they need to enroll the mfa factor
                        return Page();
                    case "MFA_ENROLL_ACTIVATE":
                        //Ask user to complete activation challenge for factor
                        return Page();
                    case "MFA_REQUIRED":
                        PopulateFactors(authResponse.Embedded.GetProperty<dynamic>("factors")); //Send factor list to populate dropdown
                        SetStateToken(authResponse.StateToken);
                        return Page();
                    case "MFA_CHALLENGE":
                        //This state is handled above in the do/while. We should not get here
                        break;
                    case "SUCCESS":
                        string url = String.Format("{0}/oauth2/default/v1/authorize?client_id={1}&response_type=id_token%20token&scope=openid%20profile&prompt=none&redirect_uri={2}&state=Af0ifjslDkj&nonce=n-0S6_WzA2Mj&sessionToken={3}", _globalConfiguration.Okta_Org, _globalConfiguration.Okta_ClientId, _globalConfiguration.Okta_RedirectUri, authResponse.SessionToken);
                        DeleteStateToken();
                        return Redirect(url);
                }
            }
            catch (Exception ex)
            {
                //TODO: This is a vey ugly way of handling factor sequecing. We need to do a check of UDP config and proactivly change the UI depending on if factor sequencing is enabled 
                //check if this happened because factor sequencing is disabled. 
                if (ex.InnerException.Message == "Api validation failed: authRequest (400, E0000001): The 'username' and 'password' attributes are required in this context.")
                {
                    List<dynamic> basicPasswordFactorList = new List<dynamic>()
                    {
                        new Dictionary<string,object>()
                            {
                                {"id","manualGenPass"},
                                {"factorType","password"}
                            }
                    };

                    PopulateFactors(basicPasswordFactorList); //Send factor list to populate dropdown

                    UserName = Request.Form[nameof(UserName)].ToString();

                }

                //TODO: Do more error checking here
            }
            return Page();
        }

        /**********************
         * Helper Methods *****
         **********************/

        private void SetStateToken(string stateToken)
        {
            //Save StateToken in session for later use
            HttpContext.Session.SetString("StateToken", stateToken);

            //Save StateToken in cookies for later use
            Response.Cookies.Append(
            "StateToken",
            stateToken,
            new CookieOptions()
            {
                Expires = DateTime.Now.AddMinutes(10),
                // Marking the cookie as essential
                IsEssential = true
            });
        }

        private void DeleteStateToken()
        {
            //Delete State Token from Session and Cookies
            HttpContext.Session.Remove("StateToken");
            Response.Cookies.Append("StateToken", "", new CookieOptions()
            {
                Expires = DateTime.Now.AddDays(-1)
            });
        }

        //populates a dropdown on the view page based on available factors
        private void PopulateFactors(List<object> factors)
        {
            //declare a list to hold all of the factors
            List<SelectListItem> factorList = new List<SelectListItem>();

            //parse the factors from the embeded object and put each factor in a select list item list
            foreach (Dictionary<string, dynamic> factor in factors)
            {
                string factorName = GetFriendlyFactorName(factor);
                factorList.Add(new SelectListItem() { Text = factorName, Value = factor["id"] + ", " + factor["factorType"] });
            }

            //populate the dropdown with the factors
            FactorList = new SelectList(factorList, "Value", "Text");

            //show the dropdown
            bShowFactors = true;

        }

        //Returns a friendly display name based on factorType and provider
        //Factor Type Docs https://developer.okta.com/docs/reference/api/factors/#factor-type
        private string GetFriendlyFactorName(Dictionary<string, dynamic> factor)
        {
            string friendlyFactorName = "";

            switch (factor["factorType"])
            {
                case "password":
                    friendlyFactorName = "Password";
                    break;
                case "push":
                    friendlyFactorName = "Okta Verify Push";
                    break;
                case "email":
                    friendlyFactorName = String.Format("Email: {0}", factor["profile"]["email"]);
                    break;
                case "sms":
                    friendlyFactorName = String.Format("Text: {0}", factor["profile"]["phoneNumber"]);
                    break;
                case "call":
                    friendlyFactorName = String.Format("Call: {0}", factor["profile"]["phoneNumber"]);
                    break;
                case "token":
                    friendlyFactorName = String.Format("Token ID: {0}", factor["profile"]["credentialId"]);
                    break;
                case "token:software:totp":
                    if (factor["provider"] == "OKTA")
                    {
                        friendlyFactorName = "Okta Verify";
                    }
                    else if (factor["provider"] == "GOOGLE")
                    {
                        friendlyFactorName = "Google Authenticator";
                    }
                    break;
                case "token:hardware":
                    friendlyFactorName = "Hardware Token";
                    break;
                case "question":
                    friendlyFactorName = String.Format("Secret Question: {0}", factor["profile"]["questionText"]);
                    break;
                case "web":
                    friendlyFactorName = String.Format("Web ID: {0}", factor["profile"]["credentialId"]);
                    break;
            }

            return friendlyFactorName;
        }

        private AuthenticationResponse SubmitChallenge(bool sendCode = false)
        {
            //Set up Okta client
            AuthenticationClient authClient = new AuthenticationClient(new OktaClientConfiguration
            {
                OktaDomain = _globalConfiguration.Okta_Org
            });

            //split up the dropdown value
            string[] selectedFactorArr = SelectedFactor.Split(", ");

            //Creating a dynamic JSON object because its easier to control vs Okta SDK classes
            dynamic payload = new JObject();
            payload.stateToken = HttpContext.Session.GetString("StateToken");

            //uri will change depending on state
            string submitUri;

            //There is no existing auth state, start new auth process
            var sessionName = new Byte[20];
            if (!HttpContext.Session.TryGetValue("StateToken", out sessionName))
            {
                payload.username = Request.Form[nameof(UserName)].ToString();

                payload.Add(new JProperty("options", new JObject()));

                payload.options.warnBeforePasswordExpired = false;
                payload.options.multiOptionalFactorEnroll = false;

                //This can only happen if factor squencing is disabled and password is selected before first submit to okta
                if (selectedFactorArr[1] == "password")
                {
                    payload.password = Request.Form[nameof(Verify)].ToString();
                }

                submitUri = "/api/v1/authn";
            }
            //Session state exists, continue existing auth process
            else
            {
                //All submit factor types
                if (selectedFactorArr[1] != "push")
                {
                    //password type has a different payload than code
                    if (selectedFactorArr[1] == "password")
                    {
                        payload.password = Request.Form[nameof(Verify)].ToString();
                    }
                    else
                    {
                        payload.passCode = Request.Form[nameof(Verify)].ToString();
                    }

                    submitUri = String.Format("/api/v1/authn/factors/{0}/verify", selectedFactorArr[0]);
                }
                //All push factor types
                else
                {
                    submitUri = String.Format("/api/v1/authn/factors/{0}/verify?autoPush={1}&rememberDevice={2}", selectedFactorArr[0], false, false);
                }
            }

            //Using raw PostAsync method because specific methods in the SDK are outdated 
            var authResponse = authClient.PostAsync<AuthenticationResponse>(new Okta.Sdk.Abstractions.HttpRequest()
            {
                Uri = submitUri,
                Payload = payload
            }).Result;

            return authResponse;
        }
    }
}
