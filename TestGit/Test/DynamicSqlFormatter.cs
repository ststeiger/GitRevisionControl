
namespace TestGit
{


    class DynamicSqlFormatter
    {


        // https://stackoverflow.com/questions/1138402/easy-way-to-convert-exec-sp-executesql-to-a-normal-query
        // https://github.com/mattwoberts/execsqlformat
        // https://github.com/wangzq/convert-sp_executesql/blob/master/convert-sp_executesql.cs
        // https://github.com/wangzq?tab=repositories
        public static void Test()
        {

            ExecSqlFormat();
            FormatSpExecuteSql();

            string lol = ConvertSql();
            System.Console.WriteLine(lol);
        }


        public static string fileName
        {
            get
            {
                string dir = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
                dir = System.IO.Path.Combine(dir, "..");
                dir = System.IO.Path.Combine(dir, "..");
                dir = System.IO.Path.GetFullPath(dir);
                dir = System.IO.Path.Combine(dir, "Test");
                dir = System.IO.Path.Combine(dir, "SqlTest.sql");

                return dir;
            }
        }


        public static void FormatSpExecuteSql()
        {
            string text = System.IO.File.ReadAllText(fileName);
            FormatSpExecuteSql(text);
        }


        public static void FormatSpExecuteSql(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                System.Console.WriteLine("File is empty; try saving it before using the hillbilly sproc decoder");
            }

            System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(
                @"exec sp_executesql N'(?<query>.*)',N'(?<decls>.*)',(?<sets>.*)",
                System.Text.RegularExpressions.RegexOptions.Singleline
            );

            System.Text.RegularExpressions.Match match = regex.Match(text);

            if (!match.Success || match.Groups.Count != 4)
            {
                System.Console.WriteLine("Didn't capture that one.");
                System.Console.Read();
                return;
            }

            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            // declares go on top
            sb.Append("DECLARE ").AppendLine(match.Groups["decls"].Value);
            // split out our sets, add them one line at a time
            foreach (var set in match.Groups["sets"]
                               .Value.Split(new char[] { ',' },
                               System.StringSplitOptions.RemoveEmptyEntries))
                sb.Append("SET ").AppendLine(set);
            // Add our query, removing double quotes
            sb.AppendLine(match.Groups["query"].Value.Replace("''", "'"));
            System.IO.File.WriteAllText("output.sql", sb.ToString());
        }



        // ConvertSql
        public static void ExecSqlFormat()
        {
            string text = System.IO.File.ReadAllText(fileName);
            // string input = System.Console.In.ReadToEnd();

            text = text.Trim(new char[] { ' ', '\t', '\r', '\n', '\v' });
            if (text.EndsWith("GO", System.StringComparison.InvariantCultureIgnoreCase))
                text = text.Substring(0, text.Length - 2);

            ExecSqlFormat(text);
        }


        static void ExecSqlFormat(string input)
        {
            System.Text.RegularExpressions.Regex re = new System.Text.RegularExpressions.Regex(@"exec*\s*sp_executesql\s+N'([\s\S]*)',\s*N'(@[\s\S]*?)',\s*([\s\S]*)"
                , System.Text.RegularExpressions.RegexOptions.IgnoreCase); // 1: the sql, 2: the declare, 3: the setting

            System.Text.RegularExpressions.Match match = re.Match(input);
            if (match.Success)
            {
                string sql = match.Groups[1].Value.Replace("''", "'");
                string declare = match.Groups[2].Value;
                string setting = match.Groups[3].Value + ',';

                // to deal with comma or single quote in variable values, we can use the variable name to split
                System.Text.RegularExpressions.Regex re2 = new System.Text.RegularExpressions.Regex(@"@[\s\S]*?\s*=");

                System.Collections.Generic.List<System.Text.RegularExpressions.Match> variables = new System.Collections.Generic.List<System.Text.RegularExpressions.Match>();
                foreach (System.Text.RegularExpressions.Match thisVariable in re2.Matches(setting))
                {
                    variables.Add(thisVariable);
                }

                System.Collections.Generic.List<string> values = new System.Collections.Generic.List<string>();
                values.AddRange(re2.Split(setting));

                for (int i = values.Count - 1; i > -1; --i)
                {
                    if (string.IsNullOrWhiteSpace(values[i]))
                        values.RemoveAt(i);
                }




                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                // System.Console.WriteLine("BEGIN\nDECLARE {0};", declare);
                sb.Append("BEGIN\r\n");
                sb.Append("DECLARE ");
                sb.Append(declare);
                sb.Append(";\r\n");


                for (int i = 0; i < variables.Count; i++)
                {
                    // System.Console.WriteLine("SET {0}{1};", variables[i], values[i].Substring(0, values[i].Length - 1));

                    sb.Append("SET ");
                    sb.Append(variables[i]);
                    sb.Append(values[i].Substring(0, values[i].Length - 1));
                    sb.Append(";\r\n");
                }

                // System.Console.WriteLine("{0}\nEND", sql);
                sb.Append(sql);
                sb.Append("\r\nEND ");

                string strSQL = sb.ToString();
                System.Console.WriteLine(strSQL);
            }
        }


        // ConvertSql
        public static string ConvertSql()
        {
            string text = System.IO.File.ReadAllText(fileName);
            return ConvertSql(text);
        }


        private static string ConvertSql(string origSql)
        {
            string tmp = origSql.Replace("''", "~~");
            string baseSql;
            string paramTypes;
            string paramData = "";
            int i0 = tmp.IndexOf("'") + 1;
            int i1 = tmp.IndexOf("'", i0);
            if (i1 > 0)
            {
                baseSql = tmp.Substring(i0, i1 - i0);
                i0 = tmp.IndexOf("'", i1 + 1);
                i1 = tmp.IndexOf("'", i0 + 1);
                if (i0 > 0 && i1 > 0)
                {
                    paramTypes = tmp.Substring(i0 + 1, i1 - i0 - 1);
                    paramData = tmp.Substring(i1 + 1);
                }
            }
            else
            {
                throw new System.Exception("Cannot identify SQL statement in first parameter");
            }

            baseSql = baseSql.Replace("~~", "'");
            if (!string.IsNullOrEmpty(paramData))
            {
                string[] paramList = paramData.Split(",".ToCharArray());
                foreach (string paramValue in paramList)
                {
                    int iEq = paramValue.IndexOf("=");
                    if (iEq < 0)
                        continue;
                    string pName = paramValue.Substring(0, iEq).Trim();
                    string pVal = paramValue.Substring(iEq + 1).Trim();
                    baseSql = baseSql.ReplaceWholeWord(pName, pVal);
                }
            }

            return baseSql;
        }


    } // End Class DynamicSqlFormatter


} // End Namespace TestGit 
