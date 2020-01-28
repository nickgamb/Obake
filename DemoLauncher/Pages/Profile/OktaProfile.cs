using System;
namespace APIDemo_DemoLauncher.Pages.Profile
{
    public class OktaProfile
    {
		public string FullName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Title { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Country { get; set; }
        public string MobilePhone { get; set; }
        public string PrimaryPhone { get; set; }
        public string Email { get; set; }
        public string Organization { get; set; }
        public string Department { get; set; }
        public string EmployeeNumber { get; set; }
        public string ProfileURL { get; set; }
        public string ProfilePictureURL { get; set; }
        public string SSN { get; set; }
        public string BusinessNumber { get; set; }
        public string AccountNumber { get; set; }
        public bool inMFA { get; set; }
    }

    public class OktaChangePasswordProfile
    {
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }
        public string VerifyPassword { get; set; }
    }
}
