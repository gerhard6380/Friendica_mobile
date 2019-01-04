using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Friendica_Mobile.UWP.Models
{
    public sealed class DisplayStringAttribute : Attribute
    {
        private readonly string _current;
        public string Current
        {
            get { return _current; }
        }

        private readonly string _english;
        public string English
        {
            get { return _english; }
        }

        private readonly string _german;
        public string German
        {
            get { return _german; }
        }

        private readonly string _spanish;
        public string Spanish
        {
            get { return _spanish; }
        }

        private readonly string _french;
        public string French
        {
            get { return _french; }
        }

        private readonly string _italian;
        public string Italian
        {
            get { return _italian; }
        }

        private readonly string _portuguese;
        public string Portuguese
        {
            get { return _portuguese; }
        }

        public string ResourceKey { get; set; }

        public DisplayStringAttribute()
        {
        }

        public DisplayStringAttribute(string current, string english, string german, string spanish, string italian, string portuguese, string french)
        {
            this._current = current;
            this._english = english;
            this._german = german;
            this._spanish = spanish;
            this._italian = italian;
            this._portuguese = portuguese;
            this._french = french;
        }

        public List<string> ReturnListOfAllLanguages()
        {
            List<string> list = new List<string>();
            list.Add(English);
            list.Add(German);
            list.Add(Spanish);
            list.Add(Italian);
            list.Add(Portuguese);
            list.Add(French);
            return list;
        }
    }

}
