using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FeedbackWebApp
{
    public class KeyWords
    {
        private string keyWord;
        private string responseID;

        public string KeyWord { get => keyWord; set => keyWord = value; }
        public string ResponseID { get => responseID; set => responseID = value; }
    }
}