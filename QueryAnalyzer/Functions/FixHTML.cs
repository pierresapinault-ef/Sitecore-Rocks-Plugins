using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Exceptions;
using System.Text.RegularExpressions;
using System.Net;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("FixHTML", Example = "update set @field = FixHTML(@field) from /sitecore/content/home", 
        LongHelp = "this method tries to fix the bad html\nIt will:\n1)Decode encoded characters\n2)remove inherited style from word or excel copy/pasting\n3)remove consecutive duplicate html tags\n4)remove empty tags\n5)try to add the missing ul tags if it detects li tags", 
        ShortHelp = "fix bad html in a text field")]
    public class FixHTML : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");

            if (args.Arguments.Length != 1)
            {
                throw new QueryException("You need to specify a field or a value in ()");
            }

            //plugin written by a rookie, probably needs some refactoring and optimization

            object obj = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
            string ObjStr = obj.ToString();

            //decoding encoded characters to make it more readable and for the Regex to work properly
            string Obj = WebUtility.HtmlDecode(ObjStr);
            var htmlArr = new[] { "p", "ul", "ol", "em", "li", "h1", "h2", "h3", "h4", "h5", "h6", "strong", "div", "span", "b", "small" };

            if (!string.IsNullOrEmpty(Obj))
            {

                //removing inherited style, class or id from excel or word
                Obj = Regex.Replace(Obj, @"(\s(class|style|id|dir)=""[^"">]+"")", string.Empty);

                ////replacing the likes of < < with a single <
                Obj = Regex.Replace(Obj, @"(<(\s*<)+)", "<");

                ////replacing the likes of > > with a single >
                Obj = Regex.Replace(Obj, @"(>(\s*>)+)", ">");

                Regex htmlRegex = new Regex(@"(<[\sdpulivemhstrongab/123456]+>)", RegexOptions.IgnoreCase);
                MatchCollection matches = htmlRegex.Matches(Obj);

                foreach (Match match in matches)
                {
                    string matchString = match.ToString();
                    string matchString2 = Regex.Replace(matchString, @"(\s)", string.Empty).ToLower();
                    //removing spaces in the tags
                    Obj = Obj.Replace(matchString, matchString2);
                    //removing consecutive duplicate tags
                    Regex matchDuplicates = new Regex(matchString2 + @"(\s*)" + matchString2);
                    Obj = matchDuplicates.Replace(Obj, matchString2);
                }

                //removing tags with no content < >               
                Obj = Regex.Replace(Obj, @"(<\s*>)", string.Empty);

                //removing second <p> tag if the first one was not closed like <p>blablabla<p></p>
                Regex pRegex = new Regex(@"(<p>[^/]+<p>)");
                MatchCollection pMatches = pRegex.Matches(Obj);
                foreach (Match match in pMatches)
                {
                    string matchStr = match.ToString();
                    string matchStr2 = matchStr.Remove(matchStr.LastIndexOf("<p>"));
                    Obj = Obj.Replace(matchStr, matchStr2);
                }

                //removing that weird div and all other divs
                string divEditor = "<div _rdEditor_temp=\"1\">";
                Obj = Obj.Replace(divEditor, string.Empty);
                Obj = Obj.Replace("<div>", string.Empty).Replace("</div>", string.Empty);

                //fixing li tags
                //first let's see if there is a <li> with no preceding <ul>, if true then we add it
                var liWithoutUl = new Regex(@"(.+?<li>)",RegexOptions.Singleline);
                Match liWithoutUlMatch = liWithoutUl.Match(Obj);
                if (!string.IsNullOrEmpty(liWithoutUlMatch.ToString()) && !liWithoutUlMatch.Value.Contains("<ul>"))
                {
                    string str = liWithoutUlMatch.ToString();
                    string str2 = str.Replace("<li>", "<ul>\r\n<li>");
                    Obj = Obj.Replace(str, str2);
                }
                
                // then we look at the case where some tool (person or real tool) tried to fix the html and closed all <ul> tags
                Regex ul = new Regex(@"(<ul>\s*<\/ul>.+?<ul>\s*<\/ul>)",RegexOptions.Singleline);
                MatchCollection closedUl = ul.Matches(Obj);
                foreach (Match match in closedUl)
                {
                    string matchStr = match.ToString();
                    //when we found a string like <ul></ul><li>content</li><ul></ul> we remove the extras
                    Regex insideClosedUl = new Regex(@"(<\/ul>.+?<ul>)",RegexOptions.Singleline);
                    Match insideClosedUlMatch = insideClosedUl.Match(matchStr);
                    string insideClosedUlMatchStr = insideClosedUlMatch.ToString();
                    string insideClosedUlMatchStr2 = insideClosedUlMatchStr.Replace("</ul>", string.Empty).Replace("<ul>", string.Empty);
                    string matchStr2 = matchStr.Replace(insideClosedUlMatchStr, insideClosedUlMatchStr2);
                    Obj = Obj.Replace(matchStr, matchStr2);
                }

                //formatting the <li></li>>
                Obj = Obj.Replace("</li>", string.Empty);
                Obj = Obj.Replace("<li>", "</li>\r\n<li>");

                //cleaning between <ul> and first </li>
                Regex contentBetweenUlLi = new Regex(@"(<ul>(.+?)<\/li>)", RegexOptions.Singleline);
                MatchCollection contentBetweenUlLiCollection = contentBetweenUlLi.Matches(Obj);
                foreach (Match match in contentBetweenUlLiCollection)
                {
                    string matchString = match.ToString();
                    string replaceUlWithText = new Regex(@"(>[\w\s]+<)").ToString();
                    MatchCollection replaceUlWithTextCollection = Regex.Matches(matchString, replaceUlWithText);
                    foreach (Match match2 in replaceUlWithTextCollection)
                    {
                        string matchStr = match2.ToString();
                        string matchStr2 = "<ul>\r\n<li" + matchStr + "/li>";
                        Obj = Obj.Replace(matchString, matchStr2);
                    }
                }

                //if there is a <ul> </li>, remove the </li>
                Regex replaceUl = new Regex(@"(<ul>\s*<\/li>)");
                Obj = replaceUl.Replace(Obj, "<ul>");

                //since we removed all the </li> before, we put the last one back
                Obj = Obj.Replace("</ul>", "</li></ul>");

                //removing <p> just before <ul>
                Regex pUl = new Regex(@"(<p>\s*<ul>)");
                Obj = pUl.Replace(Obj, "<ul>");

                //if between <p> and <ul> there is text then add </p> before <ul>
                var pUlWithText = new Regex(@"(<p>[\w\s]*<ul>)").ToString();
                MatchCollection pUlMatches = Regex.Matches(Obj, pUlWithText);
                foreach (Match match in pUlMatches)
                {
                    string matchStr = match.ToString();
                    string matchStr2 = matchStr.Replace("<ul>", "</p>\r\n<ul>");
                    Obj = Obj.Replace(matchStr, matchStr2);
                }

                //removing </p> after </ul>
                Regex ulP = new Regex(@"(<\/ul>\s*<\/p>)");
                Obj = ulP.Replace(Obj, "</ul>");

                //adding <p> after </ul> if there is some text between them
                var ulPWithText = new Regex(@"(<\/ul>[\w\s]*<\/p>)").ToString();
                MatchCollection ulPMatches = Regex.Matches(Obj, ulPWithText);
                foreach (Match match in ulPMatches)
                {
                    string matchStr = match.ToString();
                    string matchStr2 = matchStr.Replace("</ul>", "</ul>\r\n<p>");
                    Obj = Obj.Replace(matchStr, matchStr2);
                }

                //removing <p> and </p> in between <ul></ul>
                string pInUl = new Regex(@"(<ul>.+?<\/ul>)", RegexOptions.Singleline).ToString();
                MatchCollection pInUlMatches = Regex.Matches(Obj, pInUl, RegexOptions.Singleline);
                foreach (Match match in pInUlMatches)
                {
                    string matchStr = match.ToString();
                    string matchStr2 = matchStr.Replace("<p>", string.Empty).Replace("</p>", string.Empty);
                    Obj = Obj.Replace(matchStr, matchStr2);
                }

                //removing carriage return before /p tags for better visualization in Text Editor
                //Regex carriageReturn = new Regex(@"((\r|\n|\r\n)<\/p>)");
                //Obj = carriageReturn.Replace(Obj, "</p>");

                //looping to remove empty tags
                foreach (string str in htmlArr)
                {
                    Regex pattern = new Regex("(<" + str + @">([&nbsp;\s]*)<\/" + str + ">)");
                    Obj = pattern.Replace(Obj, string.Empty);
                }
                //and looping again
                foreach (string str in htmlArr)
                {
                    Regex pattern = new Regex("(<" + str + @">([&nbsp;\s]*)</" + str + ">)");
                    Obj = pattern.Replace(Obj, string.Empty);
                }


                //removing white space before closing tags
                Regex whiteSpace = new Regex(@"(\s*</)");
                Obj = whiteSpace.Replace(Obj, "</");

                //removing span or p tags with line breaks
                Regex breaks = new Regex(@"([<span>|<p>]+\s*<br\/>\s*[<\/span>|<\/p>]+)");
                Obj = breaks.Replace(Obj, "<br/>");

                //adding carriage return between p tags for better visualization in Text Editor
                Obj = Obj.Replace("</p><p>", "</p>\r\n<p>");
                Obj = Obj.Replace("</li></ul>", "</li>\r\n</ul>");
                Obj = Obj.Replace("</p><ul>", "</p>\r\n<ul>");
                Obj = Obj.Replace("</ul><p>", "</ul>\r\n<p>");

                //making sure we don't add too many \r\n
                Regex carriage = new Regex(@"(\r|\n|\r\n){2,}");
                Obj = carriage.Replace(Obj, "\r\n");
                if (Obj.EndsWith("\r\n"))
                {
                    Obj = Obj.Remove(Obj.LastIndexOf("\r\n"));
                }

                return (object)Obj;
            }
            return (object)obj;
        }
    }
}