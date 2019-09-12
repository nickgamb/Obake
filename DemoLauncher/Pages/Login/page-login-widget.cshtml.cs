using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DemoLauncher.Interfaces;

namespace DemoLauncher.Pages
{
    public class PageLoginWidgetModel : PageModel
    {
        /**************************
         * Global config settings *
         **************************/

        //Global Config
        private readonly IGlobalConfiguration _globalConfiguration;

        //okta base url
        [BindProperty]
        public string baseUrl { get; set; }
        //okta client id
        [BindProperty]
        public string clientId { get; set; }
        //okta redirect uri
        [BindProperty]
        public string redirectUri { get; set; }
        //okta logo
        [BindProperty]
        public string logo { get; set; }
        //okta issuer
        [BindProperty]
        public string issuer { get; set; }

        /*********************************
        * Class setup and global config *
        *********************************/

        public PageLoginWidgetModel(IGlobalConfiguration globalConfiguration)
        {
            _globalConfiguration = globalConfiguration;
        }

        public void OnGet()
        {
            //Set our config variables so that our view can use them
            baseUrl = _globalConfiguration.Okta_Org;
            clientId = _globalConfiguration.Okta_ClientId;
            redirectUri = _globalConfiguration.Okta_RedirectUri;
            logo = _globalConfiguration.DemoLauncher_LogoUri;
            issuer = _globalConfiguration.Okta_Issuer;
        }
    }
}