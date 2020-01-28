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
    public class ObakeHelpers
    {
        //Global Config
        private readonly IGlobalConfiguration _globalConfiguration;

        public ObakeHelpers(IGlobalConfiguration globalConfiguration)
        {
            _globalConfiguration = globalConfiguration;
        }

        //Gets the API token for the okta org from UDP
        public string GetOktaAPIToken()
        {
            //Get Access token for UDP org via OAuth Client Creds
            string OktaOAuthUrl = String.Format("{0}/oauth2/default/v1/token", _globalConfiguration.UDP_OKTA_URL);
            dynamic OktaAuth = OktaClientCredentials(OktaOAuthUrl, _globalConfiguration.UDP_OKTA_CLIENT_ID, _globalConfiguration.UDP_OKTA_CLIENT_SECRET);

            if (OktaAuth != null)
            {
                //Get Okta API Token From protected UDP Subdomain API
                string udpSubUrl = String.Format("{0}/api/subdomains/{1}", _globalConfiguration.UDP_BASE_URL, _globalConfiguration.subdomain);
                dynamic SubConfig = GetUDPSubdomainSecret(udpSubUrl, OktaAuth.SelectToken("access_token").ToString());

                if (SubConfig != null)
                {
                    //Parse subdomain config to get token
                    try
                    {
                        //Get the Okta API Token
                        return SubConfig["okta_api_token"];
                    }
                    catch (Exception)
                    {
                        //TODO: Log error parsing UDP subdomain config
                        return null;
                    }
                }
                else
                {
                    //TODO: log error getting sub config
                    return null;
                }
            }
            else
            {
                //TODO: Log error getting access token
                return null;
            }
        }

        //Get access token from Okta via client creds grant
        public JObject OktaClientCredentials(string udpConfigUrl, string clientId, string clientSecret)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    var byteArray = Encoding.ASCII.GetBytes(String.Format("{0}:{1}", clientId, clientSecret));
                    client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));

                    //Prepare Request Body
                    List<KeyValuePair<string, string>> requestData = new List<KeyValuePair<string, string>>();
                    requestData.Add(new KeyValuePair<string, string>("grant_type", "client_credentials"));
                    requestData.Add(new KeyValuePair<string, string>("scope", "secrets:read"));
                    FormUrlEncodedContent requestBody = new FormUrlEncodedContent(requestData);

                    var response = client.PostAsync(udpConfigUrl, requestBody).Result;

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
            catch (Exception)
            {
                //TODO: Log that we could not get Access Token
                return null;
            }

            //TODO: Log unkmnown state
            return null;
        }

        //Get UDP subdomain config
        public JObject GetUDPSubdomainSecret(string udpConfigUrl, string accessToken)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + accessToken);
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
            catch (Exception ex)
            {
                //Log that we could nto get secret
                return null;
            }

            //TODO: Log unknown state
            return null;
        }

        //Util to get profile values gracefully
        public string TryGetProfileValues(dynamic profile, string key)
        {
            try
            {
                string value = profile[key].ToString();
                return value;
            }
            catch (Exception)
            {
                return "";
            }

        }
    }
}
