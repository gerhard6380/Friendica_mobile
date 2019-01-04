using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Web.Http;

namespace Friendica_Mobile.UWP.HttpRequests
{
    class AuthenticationTest : clsHttpRequests
    {
        public event EventHandler UserAuthenticated;
        protected virtual void OnUserAuthenticated()
        {
            if (UserAuthenticated != null)
                UserAuthenticated(this, EventArgs.Empty);
        }

        private bool _serverAnswered;
        public bool ServerAnswered
        {
            get { return _serverAnswered; }
        }

        private bool _authenticationPassed;
        public bool AuthenticationPassed
        {
            get { return _authenticationPassed; }
        }

        private bool _unexpectedError;
        public bool UnexpectedError
        {
            get { return _unexpectedError; }
        }

        public AuthenticationTest()
        {
            this.RequestFinished += GetFriendicaConfig_RequestFinished;
        }

        private void GetFriendicaConfig_RequestFinished(object sender, EventArgs e)
        {
            _serverAnswered = false;
            _authenticationPassed = false;
            _unexpectedError = false;

            if (this.IsSuccessStateCode)
            {
                _serverAnswered = true;
                _authenticationPassed = true;
            }
            else
            {
                if (this.StatusCode == HttpStatusCode.NotFound)
                {
                }
                else if (StatusCode == HttpStatusCode.Unauthorized)
                {
                    _serverAnswered = true;
                }
                else
                {
                    _serverAnswered = true;
                    _unexpectedError = true;
                }
            }
            OnUserAuthenticated();
        }
    }
}
