using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.PCL
{
    public class OpenUrlData
    {
        // enum to select a type of page with subtype
        public enum OpenPageType { LoginPageRegistryToken }

        public OpenPageType Target { get; set; }
        public Uri Uri { get; set; }
        private string Url { get; set; }
        public Dictionary<string, string> Parameters { get; set; }

        //void OpenPage()
        //{
        //}

        public OpenUrlData(string url)
        {
            Url = url;
            Uri = new Uri(url, UriKind.RelativeOrAbsolute);
            if (Uri.LocalPath.Contains("LoginPage"))
                Target = OpenPageType.LoginPageRegistryToken;

            Parameters = new Dictionary<string, string>();
            foreach (string item in Uri.Query.Split('&'))
            {
                string[] parts = item.Replace("?", "").Split('=');
                Parameters.Add(parts[0], parts[1]);
            }
        }

        public string GetParameter(string key)
        {
            Parameters.TryGetValue(key, out string username);
            return username;
        }
    }
}
