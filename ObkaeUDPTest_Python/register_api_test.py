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

logger = logging.getLogger(__name__)
SCREENSHOTS_DIR = 'screenshots'

def set_field_value(driver, field_xpath, field_value):
    field = driver.find_element_by_xpath(field_name)
    field.click()
    field.send_keys(field_value)


# All Users 
@parameterized_class(
    ('email', 'firstName', 'lastName'), [
        ("perf1.test@gambcorp.com", "Perf1", "Test"),
        ("perf2.test@gambcorp.com", "Perf2", "Test"),
        ("perf3.test@gambcorp.com", "Perf3", "Test")
    ]
)
class TemplateTest(BaseUdpTest):
    PWD = 'Tra!nme123#'
    URL = 'https://gambcorp.obake.gambcorp.com/'

    def test_login_user(self):
        logger.info("test_register_api")
        driver = self.driver
        driver.get(self.URL)
        self.assertIn("Obake Demo", driver.title)

        try:
            loginPagesMenuButton = driver.find_element_by_xpath("//*[@id='nav-link-login']").click()
            apiPagesMenuButtonButton = driver.find_element_by_xpath("//*[@id='nav-link--pages--api']").click()
            regMenuButtonButton = driver.find_element_by_xpath("//*[@id='nav-submenu--pages--api']/li[2]/a").click()

            set_field_value(driver, "//*[@id='FullNameInput']", self.firstName + ' ' + self.lastName)
            set_field_value(driver, "//*[@id='EmailInput']", self.email)
            set_field_value(driver, "//*[@id='PasswordInput']", self.PWD)

            regButton = driver.find_element_by_xpath("//*[@id='SignupSubmitButton']").click()

            super().save_screenshot(__name__, "00", self.email, "info")
        except TimeoutException as exc:
            logger.error("Timed out: " + __name__, exc_info=True)
            super().save_screenshot(__name__, "00", self.email, "error")
        driver.switch_to.default_content()
    
    def tearDown(self):
        super().tearDown()
        logger.info("tearDown()")

if __name__ == "__main__":
    unittest.main()    