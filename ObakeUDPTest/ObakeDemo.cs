using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using System.IO;
using Okta.Sdk;

namespace ObakeUDPTest
{
    public class ObakeDemo
    {
        int TESTSIZE = 3;
        string TestUserPass = "TestPass123456!";
        string OktaDomain = "https://gamb.oktapreview.com";
        string OktaAPIToken = "ApiToken";

        IWebDriver driver;

        [SetUp]
        public void startBrowser()
        {
            ChromeOptions option = new ChromeOptions();
            option.AddArgument("--headless");

            driver = new ChromeDriver(Path.Combine(Path.GetDirectoryName(typeof(ObakeDemo).Assembly.Location), "ChromeDriver"), option);
        }

        [Test]
        public void test()
        {
            //Navigagte to Demo
            driver.Url = "https://gambcorp.obake.gambcorp.com/Login/page-login-api";
            //driver.Url = "https://gambcorp.obake.gambcorp.com";

            //Register Users
            for (int i = 0; i < TESTSIZE; i++)
            {
                //******Register******
                
                //Click Registration Page Menu Button
                IWebElement loginPagesMenuButton = driver.FindElement(By.XPath("//*[@id='nav-link-login']"));
                IWebElement apiPagesMenuButton = driver.FindElement(By.XPath("//*[@id='nav-link--pages--api']"));
                IWebElement regMenuButton = driver.FindElement(By.XPath("//*[@id='nav-submenu--pages--api']/li[2]/a"));

                loginPagesMenuButton.Click();
                apiPagesMenuButton.Click();
                regMenuButton.Click();

                //Populate Reg Form			
                IWebElement nameTextBox = driver.FindElement(By.XPath("//*[@id='FullNameInput']"));
                IWebElement emailTextBox = driver.FindElement(By.XPath("//*[@id='EmailInput']"));
                IWebElement passTextBox = driver.FindElement(By.XPath("//*[@id='PasswordInput']"));

                IWebElement signUpButton = driver.FindElement(By.XPath("//*[@id='SignupSubmitButton']"));

                nameTextBox.SendKeys("Test User " + i);
                emailTextBox.SendKeys("testuser" + i + "@gmail.com");
                passTextBox.SendKeys(TestUserPass);

                //Submit
                signUpButton.Click();
                
                //********LOGIN*******

                //Populate Username
                IWebElement emailTextBox_login = driver.FindElement(By.XPath("//*[@id='UserName']"));

                IWebElement nextButton = driver.FindElement(By.XPath("//*[@id='loginSubmitButton']"));

                emailTextBox_login.SendKeys("testuser" + i + "@gmail.com");

                //Click Next
                nextButton.Click();

                //Select password from dropdown
                IWebElement course = driver.FindElement(By.XPath("//*[@id='SelectedFactor']"));

                var selectTest = new SelectElement(course);

                selectTest.SelectByValue("password, password");

                //Populate Password textbox
                IWebElement passTextBox_Login = driver.FindElement(By.XPath("//*[@id='Verify']"));
                passTextBox_Login.SendKeys(TestUserPass);

                //Click Sign In
                IWebElement signInButton = driver.FindElement(By.XPath("//*[@id='loginSubmitButton']"));
                signInButton.Click();

                //Check Profile
                //TODO: Add This test once missing profile bits are properly handled 

                //Log Out
                IWebElement logOutButton = driver.FindElement(By.XPath("//*[@id='js-header']/div/nav/div/div[2]"));
                logOutButton.Click();
            }
        }

        [TearDown]
        public void closeBrowser()
        {
            //TODO: Delete Users From Okta After Test
            driver.Close();
        }
    }
}
