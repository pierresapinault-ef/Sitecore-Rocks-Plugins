using Sitecore.Configuration;
using Sitecore.Data.Managers;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using Sitecore.Globalization;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class AllLanguagesQuery : Opcode
    {
        public string Query { get; set; }

        public AllLanguagesQuery(string queryStr)
        {
            Assert.ArgumentNotNull((object)queryStr, "query");
            this.Query = queryStr;
        }
        public override object Evaluate(Query query, QueryContext contextNode)
        {
            Sitecore.Data.Database database = Factory.GetDatabase("master");
            Language[] languages = LanguageManager.GetLanguages(database).ToArray();
            var print = string.Empty;
            foreach (Language language in languages)
            {
                print = print + "set language='" + language.Name + "';\r\n" + Query + ";\r\n";
            }
            return (object)print;
        }
    }
}