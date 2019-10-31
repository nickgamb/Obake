using System;
namespace DemoLauncher.Interfaces
{
    public interface IGlobalConfiguration
    {
        //UDP variables that should never change
        string UDP_BASE_URL { get; set; }
        string UDP_KEY { get; set; }
        string UDP_OKTA_URL { get; set; }
        string UDP_OKTA_CLIENT_ID { get; set; }
        string UDP_OKTA_CLIENT_SECRET { get; set; }


        //UDP variables set depending on host
        string subdomain { get; set; }
        string app_name { get; set; }

        //App config variables. Specific to Okta
        string Okta_Org { get; set; } //base Okta Org Uri
        string Okta_ClientId { get; set; } //Okta OIDC Client ID
        string Okta_Issuer { get; set; } //Okta OIDC Issuer
        string Okta_RedirectUri { get; set; } //Used for OIDC redirect when converting session token to oauth token

        //App Config variables. Specific to the demo
        string DemoLauncher_LogoUri { get; set; } //The Logo to use through out the demo
        string DemoLauncher_JumboImageUri { get; set; } //The jumbotron image
        string DemoLauncher_VideoUri { get; set; } //The jumbotron video
        string DemoLauncher_IntroBlurb_Title { get; set; } //The jumbotron blurb title
        string DemoLauncher_IntroBlurb { get; set; } //The jumbotron blurb
        string DemoLauncher_BodyBlurb1_Title { get; set; } //The first body blurb title
        string DemoLauncher_BodyBlurb1 { get; set; } //The first body blurb
        string DemoLauncher_BodyBlurb2_Title { get; set; } //The second body blurb title
        string DemoLauncher_BodyBlurb2 { get; set; } //The second body blurb
        string DemoLauncher_BodyBlurb3_Title { get; set; } //The third body blurb title
        string DemoLauncher_BodyBlurb3 { get; set; } //The third body blurb
    }
}
