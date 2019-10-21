using System.Data;
using System.IO;
using Excel;
using Sitecore.Data.Query;
using Sitecore.Diagnostics;
using System.Text;

namespace Sitecore.Rocks.Server.QueryAnalyzers.Opcodes
{
    public class ExcelToRocks : Opcode
    {
        public string Filestr { get; set; }

        public ExcelToRocks(string file)
        {
            Assert.ArgumentNotNull((object)file, "Excel file");
            this.Filestr = file;
        }

        public override object Evaluate(Query query, QueryContext contextNode)
        {
            var print = new StringBuilder();
            FileStream stream = System.IO.File.Open(Filestr, FileMode.Open, FileAccess.Read);
            using (IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream))
            {
                excelReader.IsFirstRowAsColumnNames = true;
                DataSet result = excelReader.AsDataSet();

                var dataTable = result.Tables[0];

                var language = dataTable.Columns[3];

                print.Append("set language='" + language + "';\r\n");

                foreach (DataRow row in dataTable.Rows)
                {
                    var fieldName = row[1].ToString();
                    var fieldValue = row[3].ToString().Replace("'", "\'");
                    var fieldPath = row[0].ToString();
                    if (string.IsNullOrEmpty(fieldName) || string.IsNullOrEmpty(fieldValue))
                        continue;
                    print.Append(string.Format("update set @#{0}#='{1}' from /sitecore/Content{2}#;\r\n", fieldName, fieldValue, fieldPath));
                }
                print.Replace("/", "#/#");
                print.Replace("from #", "from ");
            }
            return (object)print;
        }
    }
}