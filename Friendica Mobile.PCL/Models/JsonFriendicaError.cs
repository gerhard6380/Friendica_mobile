using Newtonsoft.Json;

namespace Friendica_Mobile.PCL.Models
{
    public class JsonFriendicaError
    {
        private string _errorResult;
        private string _errorMessage;


        [JsonProperty("result")]
        public string ErrorResult
        {
            get { return _errorResult; }
            set { _errorResult = value; }
        }

        [JsonProperty("message")]
        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

    }
}
