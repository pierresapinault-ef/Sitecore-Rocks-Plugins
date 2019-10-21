using Sitecore.Data.Query;
using Sitecore.Data.Items;
using Sitecore.Data.Fields;
using Sitecore.Diagnostics;
using System.Linq;
using Sitecore.Collections;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Functions
{
    [Function("GetFields", Example = "select GetFields() from /sitecore/content/*",
        LongHelp = "Return all the fields name of an item, except standard fields",
        ShortHelp = "Return the item's fields' name")]

    public class GetFields : IFunction
    {
        public object Invoke(FunctionArgs args)
        {
            Assert.ArgumentNotNull((object)args, "args");
            Item item = args.ContextNode.GetQueryContextItem();
            string ObjStr = string.Empty;
            if (args.Arguments.Length == 1)
            {
                object obj = args.Arguments[0].Evaluate(args.Query, args.ContextNode);
                ObjStr = obj.ToString();
            }
            string print = string.Empty;
            item.Fields.ReadAll();
            FieldCollection fieldCollection = item.Fields;
            if (ObjStr == "query")
            {
                foreach (Field field in fieldCollection.Where(f => !f.Name.StartsWith("__")))
                {
                    print += '@' + field.Name + ",\n";
                }
            }
            else if (ObjStr == "count")
            {
                print = fieldCollection.Where(f => !f.Name.StartsWith("__")).Count().ToString() + " fields";
            }
            else
            foreach (Field field in fieldCollection.Where(f => !f.Name.StartsWith("__")))
            {
                    print+= field.Name + '\n';
            }
            return (object)print.TrimEnd();
        }
    }
}