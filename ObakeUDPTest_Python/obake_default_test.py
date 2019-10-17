from tests.base_udp_test import BaseUdpTest

from parameterized import parameterized_class
import logging
import time
import unittest
import os
import shutil
import sys
from selenium import webdriver
from selenium.common.exceptions import TimeoutException
from selenium.webdriver.chrome.options import Options  
from selenium.webdriver.common.by import By
from selenium.webdriver.common.keys import Keys
from selenium.webdriver.common.proxy import Proxy, ProxyType
from selenium.webdriver.support import expected_conditions as EC
from selenium.webdriver.support.ui import WebDriverWait as wait

logger = logging.getLogger('tests.obake_test')
SCREENSHOTS_DIR = 'screenshots'

def set_field_value(driver, field_name, field_value):
    field = driver.find_element_by_id(field_name)
    field.click()
    field.send_keys(field_value)

# All Users 
# Needs setup beyond terraform to set the user passwords
@parameterized_class(
    ('email', 'firstName', 'lastName'), [
        #("perf1.test@gambcorp.com", "Perf1", "Test")#,
        #("perf2.test@gambcorp.com", "Perf2", "Test"),
        ("perf3.test@gambcorp.com", "Perf3", "Test")
    ]
)
class ObakeTest(BaseUdpTest):
    PWD = 'Tra!nme123#'
    URL = 'https://gambcorp.obake.gambcorp.com/'

    def test_register_user(self):
        logger.info("test_register_user")
        driver = self.driver
        driver.get(self.URL)

        try:
            wait(driver, 5).until(EC.visibility_of_element_located((By.ID, 'SignInAccountBtn')))
            self.assertIn("Okta GambCorp Demo", driver.title)

            loginPagesMenuButton = driver.find_element_by_xpath("//*[@id='nav-link-login']").click()
            apiPagesMenuButtonButton = driver.find_element_by_xpath("//*[@id='nav-link--pages--api']").click()
            regMenuButtonButton = driver.find_element_by_xpath("//*[@id='nav-submenu--pages--api']/li[2]/a").click()

            set_field_value(driver, "FullNameInput", self.firstName + ' ' + self.lastName)
            set_field_value(driver, "EmailInput", self.email)
            set_field_value(driver, "PasswordInput", self.PWD)

            super().save_screenshot(__name__, "00", self.email, "reg-pre-submission")
            regButton = driver.find_element_by_id("SignupSubmitButton").click()
            wait(driver, 5).until(EC.visibility_of_element_located((By.ID, 'SignupSubmitButton')))

            super().save_screenshot(__name__, "01", self.email, "reg-post-submission")

            set_field_value(driver, "//*[@id='UserName']", self.email)

            super().save_screenshot(__name__, "02", self.email, "login-username-submission")
            nextButton = driver.find_element_by_xpath("//*[@id='loginSubmitButton']").click()

            driver.find_element_by_xpath("//*[@id='SelectedFactor']/option[@value='password, password']").click()
            set_field_value(driver, "//*[@id='Verify']", self.PWD)

            super().save_screenshot(__name__, "03", self.email, "login-password-submission")
            driver.find_element_by_xpath("//*[@id='loginSubmitButton']").click()

            super().save_screenshot(__name__, "04", self.email, "login-post-submission")
            signOutButton = driver.find_element_by_xpath("//*[@id='js-header']/div/nav/div/div[2]").click()

            super().save_screenshot(__name__, "05", self.email, "logout-post-submission")
        except TimeoutException as exc:
            logger.error("Timed out: " + __name__, exc_info=True)
            super().save_screenshot(__name__, "01", self.email, "error")
        driver.switch_to.default_content()
    
    def tearDown(self):
        super().tearDown()
        logger.info("tearDown()")

        self.deleteOktaTestUser(self.email)

if __name__ == "__main__":
    unittest.main() 