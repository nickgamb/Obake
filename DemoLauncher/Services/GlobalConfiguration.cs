using System;
using Microsoft.Extensions.Configuration;
using DemoLauncher.Interfaces;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;

namespace DemoLauncher.Services
{
    public class GlobalConfiguration : IGlobalConfiguration
    {
        /*************************************
         * Global Config Getters and Setters *
         *************************************/

        //UDP variables that should never change
        public string UDP_BASE_URL { get; set; }
        public string UDP_KEY { get; set; }
        public string UDP_OKTA_URL { get; set; }
        public string UDP_OKTA_CLIENT_ID { get; set; }
        public string UDP_OKTA_CLIENT_SECRET { get; set; }

        //UDP variables set depending on host
        public string subdomain { get; set; }
        public string app_name { get; set; }

        //App config variables. Specific to Okta
        public string Okta_Org { get; set; } //base Okta Org Uri
        public string Okta_ClientId { get; set; } //Okta OIDC Client ID
        public string Okta_Issuer { get; set; } //Okta OIDC Issuer
        public string Okta_RedirectUri { get; set; } //Used for OIDC redirect when converting session token to oauth token
        public string Okta_Enable_FactorSequencing { get; set; } //Enable/Disable Factor sequencing

        //App Config variables. Specific to the demo. App Settings
        public string DemoLauncher_LogoUri { get; set; } //The Logo to use through out the demo
        public string DemoLauncher_JumboImageUri { get; set; } //The jumbotron image
        public string DemoLauncher_VideoUri { get; set; } //The jumbotron video
        public string DemoLauncher_IntroBlurb_Title { get; set; } //The jumbotron blurb title
        public string DemoLauncher_IntroBlurb { get; set; } //The jumbotron blurb
        public string DemoLauncher_BodyBlurb1_Title { get; set; } //The first body blurb title
        public string DemoLauncher_BodyBlurb1 { get; set; } //The first body blurb
        public string DemoLauncher_BodyBlurb2_Title { get; set; } //The second body blurb title
        public string DemoLauncher_BodyBlurb2 { get; set; } //The second body blurb
        public string DemoLauncher_BodyBlurb3_Title { get; set; } //The third body blurb title
        public string DemoLauncher_BodyBlurb3 { get; set; } //The third body blurb
        public string GDPR_Consent_Version { get; set; } //The current version of terms

        private readonly IConfiguration _configuration;

        public GlobalConfiguration(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _configuration = configuration;

            /**********************
            * Global Config Setup *
            **********************/

            //Defaults
            UDP_BASE_URL = _configuration["AppSettings:UDP_BaseUrl"];
            UDP_KEY = _configuration["AppSettings:DefaultKey"];
            UDP_OKTA_URL = _configuration["AppSettings:UDP_Okta_Url"];
            UDP_OKTA_CLIENT_ID = _configuration["AppSettings:UDP_Okta_Client_Id"];
            UDP_OKTA_CLIENT_SECRET = _configuration["AppSettings:UDP_Okta_Client_Secret"];

            var context = httpContextAccessor.HttpContext;

            //Request.Host should be formatted as subdomain.appName.unidemo.online or, in the case of debugging, lcoalhost:port
            string host = context.Request.Host.ToString();

            //Ensure that we have a host string
            if (host != null)
            {
                //If the host is localhost, pull debug subdomain and app name
                if (host.Contains("localhost"))
                {
                    subdomain = _configuration["AppSettings:UDP_Subdomain"];
                    app_name = _configuration["AppSettings:UDP_AppName"];
                }
                //This should be a real udp environment and we should use the domain name and app name from the host string
                else
                {
                    try
                    {
                        string[] hostParts = host.Split(".");
                        subdomain = hostParts[0];
                        app_name = hostParts[1];
                    }
                    catch (Exception ex)
                    {
                        //TODO: Handle Errors
                    }
                }
            }
            else
            {
                //TODO: Handle error getting host
            }

            //Pull the config json and put it in AppConfig
            string udpConfigUrl = String.Format("{0}/api/configs/{1}/{2}", UDP_BASE_URL, subdomain, app_name);
            dynamic AppConfig = GetUDPAppConfig(udpConfigUrl);

            //Make sure AppConfig is populated and that we were able to reach UDP
            if (AppConfig != null)
            {
                //Parse AppConfig into Global Config
                try
                {
                    //Okta Settings
                    Okta_Org = AppConfig["okta_org_name"];
                    Okta_ClientId = AppConfig["client_id"];
                    Okta_Issuer = AppConfig["issuer"];
                    Okta_RedirectUri = AppConfig["redirect_uri"];
                    //Okta_RedirectUri = "http://localhost:52260";

                    //App Settings
                    DemoLauncher_LogoUri = AppConfig["settings"]["app_logo"]; //The Logo to use through out the demo
                    DemoLauncher_JumboImageUri = AppConfig["settings"]["jumbo_img_uri"]; //The jumbotron image
                    DemoLauncher_VideoUri = AppConfig["settings"]["video_uri"]; //The jumbotron video
                    DemoLauncher_IntroBlurb_Title = AppConfig["settings"]["intro_blurb_title"]; //The jumbotron blurb title
                    DemoLauncher_IntroBlurb = AppConfig["settings"]["intro_blurb"]; //The jumbotron blurb
                    DemoLauncher_BodyBlurb1_Title = AppConfig["settings"]["body_blurb1_title"]; //The first body blurb title
                    DemoLauncher_BodyBlurb1 = AppConfig["settings"]["body_blurb1"]; //The first body blurb
                    DemoLauncher_BodyBlurb2_Title = AppConfig["settings"]["body_blurb2_title"];//The second body blurb title
                    DemoLauncher_BodyBlurb2 = AppConfig["settings"]["body_blurb2"]; //The second body blurb
                    DemoLauncher_BodyBlurb3_Title = AppConfig["settings"]["body_blurb3_title"]; //The third body blurb title
                    DemoLauncher_BodyBlurb3 = AppConfig["settings"]["body_blurb3"]; //The third body blurb
                    Okta_Enable_FactorSequencing = AppConfig["settings"]["enable_factor_sequencing"];
                    GDPR_Consent_Version = AppConfig["settings"]["gdpr_consent_version"]; //The third body blurb
                }
                catch (Exception exception)
                {
                    //TODO: Log that we cant cant parse app config
                }
            }
            else
            {
                //TODO: Log that we could not reach UDP
            }
            
            //Use a local config if we couldnt reach UDP or value is null

            //Okta Settings
            if (Okta_Org == null)
            {
                Okta_Org = _configuration["AppSettings:okta_org_name"];
            }
            if (Okta_ClientId == null)
            {
                Okta_ClientId = _configuration["AppSettings:client_id"];
            }
            if (Okta_Issuer == null)
            {
                Okta_Issuer = _configuration["AppSettings:issuer"];
            }
            if (Okta_RedirectUri == null)
            {
                Okta_RedirectUri = _configuration["AppSettings:redirect_uri"];
            }

            //App Settings
            if (DemoLauncher_LogoUri == null)
            {
                DemoLauncher_LogoUri = _configuration["AppSettings:app_logo"]; //The Logo to use through out the demo
            }
            if (DemoLauncher_JumboImageUri == null)
            {
                DemoLauncher_JumboImageUri = _configuration["AppSettings:jumbo_img_uri"]; //The jumbotron image
            }
            if (DemoLauncher_VideoUri == null)
            {
                DemoLauncher_VideoUri = _configuration["AppSettings:video_uri"]; //The jumbotron video
            }
            if (DemoLauncher_IntroBlurb_Title == null)
            {
                DemoLauncher_IntroBlurb_Title = _configuration["AppSettings:intro_blurb_title"]; //The jumbotron blurb title
            }
            if (DemoLauncher_IntroBlurb == null)
            {
                DemoLauncher_IntroBlurb = _configuration["AppSettings:intro_blurb"]; //The jumbotron blurb
            }
            if (DemoLauncher_BodyBlurb1_Title == null)
            {
                DemoLauncher_BodyBlurb1_Title = _configuration["AppSettings:body_blurb1_title"]; //The first body blurb title
            }
            if (DemoLauncher_BodyBlurb1 == null)
            {
                DemoLauncher_BodyBlurb1 = _configuration["AppSettings:body_blurb1"]; //The first body blurb
            }
            if (DemoLauncher_BodyBlurb2_Title == null)
            {
                DemoLauncher_BodyBlurb2_Title = _configuration["AppSettings:body_blurb2_title"]; //The second body blurb title
            }
            if (DemoLauncher_BodyBlurb2 == null)
            {
                DemoLauncher_BodyBlurb2 = _configuration["AppSettings:body_blurb2"]; //The second body blurb
            }
            if (DemoLauncher_BodyBlurb3_Title == null)
            {
                DemoLauncher_BodyBlurb3_Title = _configuration["AppSettings:body_blurb3_title"]; //The third body blurb title
            }
            if(DemoLauncher_BodyBlurb3 == null)
            { 
                DemoLauncher_BodyBlurb3 = _configuration["AppSettings:body_blurb3_title"]; //The third body blurb
            }
            if (Okta_Enable_FactorSequencing == null)
            {
                Okta_Enable_FactorSequencing = _configuration["AppSettings:enable_factor_sequencing"];
            }
            if (GDPR_Consent_Version == null)
            {
                GDPR_Consent_Version = _configuration["AppSettings:gdpr_consent_version"];
            }
        }

        //Get UDP App Config
        public JObject GetUDPAppConfig(string udpConfigUrl)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var response = client.GetAsync(udpConfigUrl).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        var responseContent = response.Content;

                        string responseString = responseContent.ReadAsStringAsync().Result;

                        return JObject.Parse(responseString); 
                    }
                    else
                    {
                        //TODO: Log error 
                    }
                }
            }
            catch (Exception exception)
            {
                //TODO: Log that we cant reach UDP
                return null;
            }

            //TODO: Log unknown state
            return null;
        }
    }
}
