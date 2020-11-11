# Obake

Obake (お化け) is a class of yōkai, preternatural creatures in Japanese folklore. Literally, the terms mean a thing that     changes, referring to a state of transformation or shapeshifting. The Obake Demo has the ability to transform and change to meet specific needs on demand.

Obake is an asp.net core razor pages applicaiton that is fully customizable usng a completly codeless apporach. This allows the application to be tailored before every demonstration to adhere to specific branding needs.

Obake showcases the following:
1. Authentication
    * [Okta Sign-In Widget](https://github.com/okta/okta-signin-widget).
    * Custom authentication experience purly using API (.net core example using Okta [.Net Auth SDK](https://github.com/okta/okta-auth-dotnet)
2. Okta Universal Directory for User Profiles
3. User management purly using API (.net core example using Okta [.Net Management SDK](https://github.com/okta/okta-sdk-dotnet)
3. ABAC and RBAC

This application will help you demonstrate:

1. Authentication and Authorization
2. Social Auth
3. MFA
4. Universal Directory
5. Centralized Identity Management
6. API Access Management
7. Custom User Expereinces on top of the Okta Platform
8. Okta user management 
9. ABAC / RBAC

## KNOWN LIMITATIONS

* Obake is currently in development and has several limitations

1. Some Okta auth states are not yet supported in the Login API Example. The most prominant example is MFA Factor Enroll state which happens when a new user has an MFA requirement but has not enrolled the factor. To Demo MFA Factor Enrollment, use the Widget Login Pages instead of the API Login Pages.
2. The entire WWWROOT folder for this project is not public because it contains source and file content that cannot be open sourced. To use this project as described below, you will need the WWWROOT folder or the app will not render correctly. If you are an Otka employee, and feel that you should have access to this content, please contact the repo owners. 
3. Please take a moment to review the Projects tab and Issues tab here in github. This will give you a better idea of what is being worked on and what is planned.
4. Widget uses implicit flow instead of Auth Code. Not a production ready example. 

## 0. Configure your Okta Org

1. Configure and OIDC app in your Okta org and note the Client ID and Issuer
2. Configure a new API token in your Okta org and note the token value
3. Configure an MFA Policy if desired
4. Edit the base Okta user profile and add a new string attribute called profilePictureUrl. Make sure to populate it for your users with a url to a profile picture
5. If you want to use self service reg in the widget, you need to enable the beta flag for your org and configure self service in the admin UI. This goes for UDP versions as well! Go to Directory > Self-Service Registration, enable the feature, and customize to your environment and preferances. The redirect URL needs to be set to your Obake environment URL (i.e. domain.obake.gambcorp.com)
6. Obake prefers Factor Sequencing to be enabled. It will function without it, but it is recomended to enable Factor Sequencing in your org. Make sure to set enable_factor_sequencing to true or false in AppSettings.json or in UDP.

## 1. Build the application

Obake can be configured to run in many ways. However, the first step is to compile it. You will first need to install Visual Studio. 

**Notes:**

* This project is built with .net core 2.2 and is fully cross platform.
* 2019 Community was used to create the application so it is the most recomended version. 
* Obake is cross platform so Visual Studio for Windows or Mac is supported. 
* Obake can be published driectly from Visual Studio to file (for use with Linux), to AWS, to Azure, or directly to a Windows Server. 
* Obake can be ran locally (see running locally) from Visual Studio (using IIS Express).
* Obake contains a docker file generated by Visual Studio. The docker file can be used to debug inside of a container locally via Visual Studio (see running locally) or it can be used via docker directly to build a docker image. 

1. Go to the [Visual Studio](https://visualstudio.microsoft.com/) home page, download, and install.
2. Clone this repo to your local machine 
3. In the root dirctory (same folder as the web.config), create a new file called appsettings.json and use the following to populate it.

```javascript{
  "Logging": {
    "LogLevel": {
      "Default": "Warning"
    }
  },
  "AppSettings": {
    "UDP_BaseUrl": "https://udpurl.io",
    "UDP_Subdomain": "udpSubDomain",
    "UDP_AppName": "obake",
    "okta_org_name": "https://org.okta.com",
    "okta_api_token": "oktaAPIToken",
    "client_id": "oktaClientId",
    "issuer": "https://org.okta.com/oauth2/default",
    "redirect_uri": "https://udpsubDomain.obake.gambcorp.com",
    "app_logo": "http://urlOfALogo.com",
    "jumbo_img_uri": "http://urlOfABigImage.com",
    "video_uri": "https://www.youtube.com/embed/youtubeEmbedUrl",
    "intro_blurb_title": "Okta is the identity standard.",
    "intro_blurb": "The most complete access management platform for your workforce and customers, securing all your critical resources from cloud to ground.",
    "body_blurb1_title": "Workforce Identity",
    "body_blurb1": "Protect and enable employees, contractors, and partners.",
    "body_blurb2_title": "Customer Identity",
    "body_blurb2": "Build secure, seamless experiences for your customers.",
    "body_blurb3_title": "Your technology can do more",
    "body_blurb3": "With over 6,000+ integrations, there's a good chance we can connect anyone that touches your organization to any technology they want to use. But what really sets us apart is the depth of our integrations. Explore how SAML, automated provisioning, and security analytics can make the apps you already use even better.",
	"enable_factor_sequencing": "false"
  },
  "AllowedHosts": "*"
}
```

   **Notes:**
   - Obake is coded to first try to get app settings from the Okta UDP. It will first try to parse the host url to see if an okta subdomain and app name are present. If it is not, or if it contains localhost, it will then try to pull a UDP subdomain and app name from the local appsettings.json and then try to reach out to UDP to get the app settings. If Obake is unable to get any app settings from UDP, it will default to what is in appsettings.json. Refer to [GlobalConfiguration.cs](https://github.com/nickgamb/Obake/blob/master/DemoLauncher/Services/GlobalConfiguration.cs) for details on the config logic.


4. Launch APIDemo_Okta.sln
5. Change the configuration from Debug to Release
6. Click Build > Publish to publish to a folder or services supported by Visual Studio

  **Notes:**
  - For Docker or servers other than Windows, publish to a folder. 

## 2. Run with Docker
1. In your Okta org, you must add CORS and a redirect_uri for your application.
    Please add the following to your Okta org:
    * Add redirect_uri = `{Scheme}://{ServiceHost}:{ServicePort}` to your OpenID Connect app
    * Add a CORS entry for ``{Scheme}://{ServiceHost}:{ServicePort}``
    
    **Notes:**
    * By default, your redirect uri and cors uri should be:
    
    ```http://localhost:52261/```
    * Default uri scheme can be cuanged in lauchSettings.json in Visual Studio

5. Run the command:
    ```
    docker build -f "C:\ObakePath\Okta_DemoLauncher\DemoLauncher\Dockerfile" -t apidemodemolauncher:dev --target base  --label "com.microsoft.created-by=visual-studio" --label "com.microsoft.visual-studio.project-name=APIDemo_DemoLauncher" "C:\ObakePath\Okta_DemoLauncher" 
    
    docker run -dt -v "C:\Users\userName\vsdbg\vs2017u5:/remote_debugger:rw" -v "C:\ObakePath\Okta_DemoLauncher\DemoLauncher:/app" -v "C:\Users\user\.nuget\packages\:/root/.nuget/fallbackpackages2" -v "C:\Program Files\dotnet\sdk\NuGetFallbackFolder:/root/.nuget/fallbackpackages" -e "DOTNET_USE_POLLING_FILE_WATCHER=1" -e "ASPNETCORE_ENVIRONMENT=Development" -e "NUGET_PACKAGES=/root/.nuget/fallbackpackages2" -e "NUGET_FALLBACK_PACKAGES=/root/.nuget/fallbackpackages;/root/.nuget/fallbackpackages2" -p 52261:80 --entrypoint tail apidemodemolauncher:dev -f /dev/null 
    ```
    
## 3. Run Locally

1. In Visual Studio, click the green play icon.
2. To run locally within a docker container, click the drop down next to the green play icon and click Docker. 
3. To run locally outside of Visual Studio (or on linux), open cmd, or terminal, cd to the bin/Release directory (or publish folder) and run the command:
  ```
  dotnet APIDemo_DemoLauncher.dll
  ```
