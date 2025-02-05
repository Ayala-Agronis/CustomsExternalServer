using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CustomsExternal.Services
{

    public class IdValidator
    {
        public static bool IsValidId(string id)
        {
            if (string.IsNullOrEmpty(id) || id.Length != 9 || !long.TryParse(id, out _))
            {
                return false;
            }

            int sum = 0;
            bool isEvenPosition = false;

            for (int i = id.Length - 1; i >= 0; i--)
            {
                int digit = id[i] - '0'; 

                if (isEvenPosition)
                {
                    digit *= 2;
                    if (digit > 9)
                    {
                        digit -= 9; 
                    }
                }

                sum += digit;
                isEvenPosition = !isEvenPosition;
            }

            return sum % 10 == 0;
        }
    }
}