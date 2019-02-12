using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PhoneNumbers;

namespace Kenboi.Data.Utils
{
    public static class MyValidator
    {

        static readonly Regex ValidEmailRegex = CreateValidEmailRegex();


        private static Regex CreateValidEmailRegex()
        {
            string validEmailPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                       + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                       + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return new Regex(validEmailPattern, RegexOptions.IgnoreCase);
        }

        public static Boolean ValidatePhone(string phone)
        {
            return ValidatePhoneWithCode(phone, "KE");
        }

        public static Boolean ValidatePhone(string phone, String country)
        {
            return ValidatePhoneWithCode(phone, country);
        }

        public static String FormatPhone(String phone)
        {
            return FormatPhoneWithCode(phone, "KE");
        }

        public static String FormatPhone(String phone, String country)
        {
            return FormatPhoneWithCode(phone, country);
        }

        public static String FormatPhoneWithCode(String phone, String country = "KE")
        {
            if (ValidatePhoneWithCode(phone, country.Trim()))
            {
                PhoneNumberUtil phs = PhoneNumberUtil.GetInstance();
                PhoneNumber phn = phs.Parse(phone, country);
                return phs.Format(phn, PhoneNumberFormat.E164).Substring(1);
            }

            return null;
        }

        public static Boolean ValidatePhoneWithCode(String phone, String country = "KE")
        {
            if (!string.IsNullOrEmpty(phone))
            {
                PhoneNumberUtil phs = PhoneNumberUtil.GetInstance();
                try
                {
                    PhoneNumber phn = phs.Parse(phone, country);
                    if (!phs.IsValidNumber(phn))
                    {
                        throw new Exception();
                    }

                    return true;

                }
                catch (Exception)
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        internal static bool EmailIsValid(string emailAddress)
        {
            bool isValid = ValidEmailRegex.IsMatch(emailAddress);

            return isValid;

        }
    }
}
