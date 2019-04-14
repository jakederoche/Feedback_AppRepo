using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace FeedbackWebApp
{
    public class Response
    {
        public int responseId;
        public string responseText;
        public int questionId;
        public double sentiment;
    }
}