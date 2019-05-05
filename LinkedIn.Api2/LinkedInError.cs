using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinkedIn.Api2
{
    public class LinkedInError : Exception
    {
        public LinkedInError(string message) : base(message)
        {
        }
    }
}
