using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Okta.Sdk;
using System.IdentityModel.Tokens.Jwt;

namespace DemoLauncher.Pages
{
    public class PageProfileMain1Model : PageModel
    {
        /**********************
         * Getters and Setters*
         **********************/

        //input fields from page
        [BindProperty]
        public APIDemo_DemoLauncher.Pages.Profile.OktaProfile oktaProfile { get; set; } = new APIDemo_DemoLauncher.Pages.Profile.OktaProfile();

        /************************
         * On Every Get**********
         ************************/

        public void OnGet()
        {
            var stream = Request.Cookies["idToken"].ToString();
            var handler = new JwtSecurityTokenHandler();
            var jsonToken = handler.ReadToken(stream);
            var tokenS = handler.ReadToken(stream) as JwtSecurityToken;

            //Do Error handling for missing claims 
            oktaProfile.FirstName = tokenS.Claims.First(claim => claim.Type == "FirstName").Value;
            oktaProfile.LastName = tokenS.Claims.First(claim => claim.Type == "LastName").Value;
            oktaProfile.FullName = oktaProfile.FirstName + " " + oktaProfile.LastName;
            oktaProfile.Title = tokenS.Claims.First(claim => claim.Type == "Title").Value;

            //See if there is a profile url in Okta
            try
            {
                oktaProfile.ProfilePictureURL = tokenS.Claims.First(claim => claim.Type == "ProfilePictureUrl").Value;
            }
            //Use a placeholder if no image is in Okta
            catch (Exception ex)
            {
                oktaProfile.ProfilePictureURL = "../../assets/img-temp/400x450/img1.jpg";
            }
        }
    }
}