using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Kenboi.Data.Utils
{
  public  class Helpers
    {
        public static string FormatAsCurrency(dynamic val, bool includeSign = true)
        {
            double.TryParse($"{val}", out double value);
            string sign = includeSign ? "Sh" : "";
            return $"{sign} {value:N}";
        }

        public static bool IsValidPhoneNo(string txtPhoneText)
        {
            if (txtPhoneText.Length > 12 && txtPhoneText.Length < 10)
            {
                return false;
            }
            return int.TryParse(txtPhoneText, out int n);
        }

    }
}
