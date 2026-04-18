using System;
using System.Web;

namespace Hyperledger.Aries.Utils
{
    public static class UriUtils
    { 
        public static string? GetQueryParam(this Uri uri, string param)
        {
            if (uri.ToString().Contains("%"))
                uri = new Uri(Uri.UnescapeDataString(uri.ToString()));

            var query = HttpUtility.ParseQueryString(uri.Query);

            return query[param];
        }
    }
    
}
