using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using DemoLauncher.Interfaces;

namespace DemoLauncher.Pages
{
    public class HomeDefaultModel : PageModel
    {
        /**************************
        * Global config settings *
        **************************/

        //Global Config
        private readonly IGlobalConfiguration _globalConfiguration;

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
        public string DemoLauncher_BodyBlurb2_Title { get; set; } //The second body blurb title
        [BindProperty]
        public string DemoLauncher_BodyBlurb2 { get; set; } //The second body blurb
        [BindProperty]
        public string DemoLauncher_BodyBlurb3_Title { get; set; } //The third body blurb title
        [BindProperty]
        public string DemoLauncher_BodyBlurb3 { get; set; } //The third body blurb


        /*********************************
        * Class setup and global config *
        *********************************/

        public HomeDefaultModel(IGlobalConfiguration globalConfiguration)
        {
            _globalConfiguration = globalConfiguration;
        }

        /*************
        * View logic *
        **************/

        public void OnGet()
        {
            //Set our config variables so that our view can use them
            DemoLauncher_LogoUri = _globalConfiguration.DemoLauncher_LogoUri; //The Logo to use through out the demo
            DemoLauncher_JumboImageUri = _globalConfiguration.DemoLauncher_JumboImageUri; //The jumbotron image
            DemoLauncher_VideoUri = _globalConfiguration.DemoLauncher_VideoUri; //The jumbotron video
            DemoLauncher_IntroBlurb_Title = _globalConfiguration.DemoLauncher_IntroBlurb_Title; //The jumbotron blurb title
            DemoLauncher_IntroBlurb = _globalConfiguration.DemoLauncher_IntroBlurb; //The jumbotron blurb
            DemoLauncher_BodyBlurb1_Title = _globalConfiguration.DemoLauncher_BodyBlurb1_Title; //The first body blurb title
            DemoLauncher_BodyBlurb1 = _globalConfiguration.DemoLauncher_BodyBlurb1; //The first body blurb
            DemoLauncher_BodyBlurb2_Title = _globalConfiguration.DemoLauncher_BodyBlurb2_Title; //The second body blurb title
            DemoLauncher_BodyBlurb2 = _globalConfiguration.DemoLauncher_BodyBlurb2; //The second body blurb
            DemoLauncher_BodyBlurb3_Title = _globalConfiguration.DemoLauncher_BodyBlurb3_Title; //The third body blurb title
            DemoLauncher_BodyBlurb3 = _globalConfiguration.DemoLauncher_BodyBlurb3; //The third body blurb
        }
    }
}
