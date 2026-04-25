using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using System.Data;
using System.Drawing;
using System.Linq;

namespace NoScript
{
    public partial class Form1 : Form
    {
        Dictionary<string, string> variables = new Dictionary<string, string>();
        Dictionary<string, string> methods = new Dictionary<string, string>();
        private bool _hasError = false;

        public Form1() { InitializeComponent(); }

        private void btnRun_Click(object sender, EventArgs e)
        {
            txtOutput.Clear();
            variables.Clear();
            methods.Clear();
            _hasError = false;

            string fullCode = rtbCode.Text.Trim();

            if (string.IsNullOrWhiteSpace(fullCode))
            {
                ReportError("Empty code. Nothing to run.", 0);
                return;
            }

            var statements = SplitIntoStatements(fullCode);

            if (statements.Count == 0)
            {
                ReportError("No valid statements found.", 1);
                return;
            }

            foreach (var (stmt, lineNum) in statements)
            {
                if (_hasError) break;
                ExecuteNeoScript(stmt.Trim(), lineNum);
            }

            if (!_hasError)
            {
                LogSuccess("Execution completed successfully.");
            }
        }

        // ===== الحل: Placeholder Approach =====
        private List<(string stmt, int line)> SplitIntoStatements(string code)
        {
            var result = new List<(string, int)>();
            var placeholders = new Dictionary<string, string>();

            string blockPattern = @"(Check\s*\([\s\S]+?\)\s*Do[\s\S]+?Done|Repeat\s*\([\s\S]+?\)\s*Flow[\s\S]+?EndFlow|Action\s+\w+\[.*?\]\s*Result\s*\w+\s*Start[\s\S]*?End)";
            string modified = code;

            // الخطوة 1: استبدل كل block بـ placeholder
            int idx = 0;
            foreach (Match m in Regex.Matches(code, blockPattern, RegexOptions.Singleline))
            {
                string placeholder = $"BLOCK{idx}PLACEHOLDER";
                placeholders[placeholder] = m.Value;
                modified = modified.Replace(m.Value, placeholder);
                idx++;
            }

            // الخطوة 2: split على ;
            int currentLine = 1;
            var parts = modified.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in parts)
            {
                string trimmed = part.Trim();
                if (string.IsNullOrEmpty(trimmed)) continue;

                // حساب رقم السطر
                int partStart = modified.IndexOf(part, StringComparison.Ordinal);
                int lineNum = code.Substring(0, Math.Min(partStart, code.Length))
                                  .Count(c => c == '\n') + 1;

                // الخطوة 3: لو placeholder، رجع الـ block الأصلي
                if (placeholders.ContainsKey(trimmed))
                {
                    result.Add((placeholders[trimmed], lineNum));
                }
                else
                {
                    result.Add((trimmed + ";", lineNum));
                }
            }

            return result;
        }

        private bool ExecuteNeoScript(string line, int lineNumber)
        {
            if (_hasError) return false;
            if (string.IsNullOrWhiteSpace(line) || line == ";") return true;

            try
            {
                string firstWord = line.Split(new[] { ' ', '(', '\r', '\n', '\t' },
                    StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "";

                string[] validKeywords = { "Define", "Set", "Invoke", "Check", "Repeat", "Action" };

                if (!validKeywords.Contains(firstWord))
                {
                    string suggestion = FindClosestKeyword(firstWord, validKeywords);
                    if (suggestion != null)
                        ReportError($"Unknown command '{firstWord}'. Did you mean '{suggestion}'?", lineNumber);
                    else
                        ReportError($"Unknown command '{firstWord}'. Valid commands: Define, Set, Invoke, Check, Repeat, Action.", lineNumber);
                    return false;
                }

                if (firstWord == "Define") return HandleDefine(line, lineNumber);
                if (firstWord == "Set") return HandleAssign(line, lineNumber);
                if (firstWord == "Invoke") return HandleInvoke(line, lineNumber);
                if (firstWord == "Check") return HandleCheck(line, lineNumber);
                if (firstWord == "Repeat") return HandleRepeat(line, lineNumber);
                if (firstWord == "Action") return HandleAction(line, lineNumber);

                return true;
            }
            catch (Exception ex)
            {
                ReportError($"Runtime Error: {ex.Message}", lineNumber);
                return false;
            }
        }

        private string FindClosestKeyword(string input, string[] keywords)
        {
            foreach (var kw in keywords)
                if (LevenshteinDistance(input.ToLower(), kw.ToLower()) <= 2)
                    return kw;
            return null;
        }

        private int LevenshteinDistance(string s, string t)
        {
            int[,] d = new int[s.Length + 1, t.Length + 1];
            for (int i = 0; i <= s.Length; i++) d[i, 0] = i;
            for (int j = 0; j <= t.Length; j++) d[0, j] = j;
            for (int i = 1; i <= s.Length; i++)
                for (int j = 1; j <= t.Length; j++)
                    d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                        d[i - 1, j - 1] + (s[i - 1] == t[j - 1] ? 0 : 1));
            return d[s.Length, t.Length];
        }
        private bool HandleDefine(string line, int ln)
        {
            var match = Regex.Match(line, @"^Define\s+(int|float|string|bool)\s+(\w+)(\s+Init\s+(.+?))?;$");
            if (!match.Success)
            {
                ReportError("Syntax Error in 'Define'. Format: Define [int|float|string|bool] [name] Init [value];", ln);
                return false;
            }

            string type = match.Groups[1].Value;
            string name = match.Groups[2].Value;
            string rawExpr = match.Groups[4].Value.Trim(); // ===== الـ raw قبل EvaluateExpr =====

            if (!string.IsNullOrEmpty(rawExpr))
            {
                // ===== نتحقق من النوع على الـ raw expression الأول =====
                if (!ValidateType(type, rawExpr, ln)) return false;
            }

            string value = string.IsNullOrEmpty(rawExpr) ? GetDefaultValue(type) : EvaluateExpr(rawExpr, ln);
            if (_hasError) return false;

            variables[name] = value;
            LogSuccess($"Variable '{name}' defined as {type} = {value}");
            return true;
        }

        private bool ValidateType(string type, string rawExpr, int ln)
        {
            bool isStringLiteral = rawExpr.StartsWith("\"") && rawExpr.EndsWith("\"");

            switch (type)
            {
                case "string":
                    // string لازم قيمتها تكون جوه ""
                    if (!isStringLiteral)
                    {
                        ReportError($"Type Error: string value must be in quotes. Write: \"{rawExpr}\" instead of {rawExpr}", ln);
                        return false;
                    }
                    break;

                case "int":
                    // int مينفعش يكون string literal
                    if (isStringLiteral)
                    {
                        ReportError($"Type Error: int cannot have a text value. Use a whole number like 5 or 100.", ln);
                        return false;
                    }
                    // ونتحقق إنه فعلاً رقم صحيح
                    if (!int.TryParse(rawExpr, out _))
                    {
                        ReportError($"Type Error: '{rawExpr}' is not a valid int. Expected a whole number like 5 or 100.", ln);
                        return false;
                    }
                    break;

                case "float":
                    if (isStringLiteral)
                    {
                        ReportError($"Type Error: float cannot have a text value. Use a decimal number like 3.14.", ln);
                        return false;
                    }
                    if (!float.TryParse(rawExpr, System.Globalization.NumberStyles.Float,
                        System.Globalization.CultureInfo.InvariantCulture, out _))
                    {
                        ReportError($"Type Error: '{rawExpr}' is not a valid float. Expected a decimal number like 3.14.", ln);
                        return false;
                    }
                    break;

                case "bool":
                    if (isStringLiteral)
                    {
                        ReportError($"Type Error: bool cannot be in quotes. Use 'true' or 'false' without quotes.", ln);
                        return false;
                    }
                    if (rawExpr != "true" && rawExpr != "false")
                    {
                        ReportError($"Type Error: '{rawExpr}' is not a valid bool. Use 'true' or 'false'.", ln);
                        return false;
                    }
                    break;
            }
            return true;
        }

        private string GetDefaultValue(string type)
        {
            switch (type)
            {
                case "int": return "0";
                case "float": return "0.0";
                case "string": return "";
                case "bool": return "false";
                default: return "0";
            }
        }

        private bool HandleAssign(string line, int ln)
        {
            var match = Regex.Match(line, @"^Set\s+(\w+)\s+To\s+(.+?);$");
            if (!match.Success)
            {
                ReportError("Syntax Error in 'Set'. Format: Set [variable] To [value];", ln);
                return false;
            }

            string name = match.Groups[1].Value;
            if (!variables.ContainsKey(name))
            {
                ReportError($"Variable '{name}' is not defined. Use 'Define' first.", ln);
                return false;
            }

            string value = EvaluateExpr(match.Groups[2].Value, ln);
            if (_hasError) return false;
            variables[name] = value;
            return true;
        }

        private bool HandleInvoke(string line, int ln)
        {
            var match = Regex.Match(line, @"^Invoke\s+(\w+)\((.*?)\);$");
            if (!match.Success)
            {
                ReportError("Syntax Error in 'Invoke'. Format: Invoke FunctionName(args);", ln);
                return false;
            }

            string func = match.Groups[1].Value;
            string param = match.Groups[2].Value;

            if (func == "Print")
            {
                string val = EvaluateExpr(param, ln);
                if (_hasError) return false;
                txtOutput.SelectionColor = Color.White;
                txtOutput.AppendText($"> {val}\r\n");
                return true;
            }

            if (methods.ContainsKey(func))
            {
                ExecuteSubCode(methods[func]);
                return !_hasError;
            }

            ReportError($"Function '{func}' is not defined.", ln);
            return false;
        }

        private bool HandleCheck(string line, int ln)
        {
            var match = Regex.Match(line,
                @"Check\s*\((.+?)\)\s*Do\s*([\s\S]+?)(\s*Otherwise\s*Do\s*([\s\S]+?))?\s*Done",
                RegexOptions.Singleline);

            if (!match.Success)
            {
                ReportError("Syntax Error in 'Check'. Format: Check (condition) Do ... [Otherwise Do ...] Done", ln);
                return false;
            }

            bool cond = EvaluateCondition(match.Groups[1].Value, ln);
            if (_hasError) return false;

            if (cond)
                ExecuteSubCode(match.Groups[2].Value);
            else if (!string.IsNullOrEmpty(match.Groups[4].Value))
                ExecuteSubCode(match.Groups[4].Value);

            return !_hasError;
        }

        private bool HandleRepeat(string line, int ln)
        {
            var match = Regex.Match(line,
                @"Repeat\s*\((.+?)\)\s*Flow\s*([\s\S]+?)\s*EndFlow",
                RegexOptions.Singleline);

            if (!match.Success)
            {
                ReportError("Syntax Error in 'Repeat'. Format: Repeat (condition) Flow ... EndFlow", ln);
                return false;
            }

            int safety = 0;
            while (EvaluateCondition(match.Groups[1].Value, ln) && !_hasError)
            {
                if (safety >= 1000)
                {
                    ReportError("Infinite loop detected! Stopped after 1000 iterations.", ln);
                    return false;
                }
                ExecuteSubCode(match.Groups[2].Value);
                safety++;
            }
            return !_hasError;
        }

        private bool HandleAction(string line, int ln)
        {
            var match = Regex.Match(line,
                @"Action\s+(\w+)\[.*?\]\s*Result\s*\w+\s*Start\s+([\s\S]*?)\s+End",
                RegexOptions.Singleline);

            if (!match.Success)
            {
                ReportError("Syntax Error in 'Action'. Format: Action Name[params] Result type Start ... End", ln);
                return false;
            }

            methods[match.Groups[1].Value] = match.Groups[2].Value;
            LogSuccess($"Action '{match.Groups[1].Value}' registered.");
            return true;
        }

        private void ExecuteSubCode(string block)
        {
            if (_hasError) return;
            var stmts = SplitIntoStatements(block);
            foreach (var (stmt, line) in stmts)
            {
                if (_hasError) break;
                ExecuteNeoScript(stmt.Trim(), line);
            }
        }

        private void ReportError(string message, int line)
        {
            _hasError = true;
            txtOutput.SelectionColor = Color.Red;
            string lineInfo = line > 0 ? $"Line {line}" : "Unknown Line";
            txtOutput.AppendText($"\n[ERROR at {lineInfo}]: {message}\n");
            txtOutput.AppendText("=== Execution stopped ===\n");
            txtOutput.ScrollToCaret();
        }

        private void LogSuccess(string message)
        {
            txtOutput.SelectionColor = Color.Lime;
            txtOutput.AppendText($"[OK]: {message}\r\n");
        }

        private string EvaluateExpr(string expr, int ln = 0)
        {
            if (_hasError) return "";
            string temp = expr.Trim();

            if (temp.StartsWith("\"") && temp.EndsWith("\""))
                return temp.Substring(1, temp.Length - 2);

            if (variables.ContainsKey(temp))
                return variables[temp];

            string math = temp;
            foreach (var v in variables)
                math = Regex.Replace(math, $@"\b{v.Key}\b", v.Value);

            try
            {
                return new DataTable().Compute(math, null).ToString();
            }
            catch
            {
                ReportError($"Cannot evaluate expression: '{expr}'", ln);
                return "";
            }
        }

        private bool EvaluateCondition(string cond, int ln = 0)
        {
            if (_hasError) return false;
            string temp = cond;
            foreach (var v in variables)
                temp = Regex.Replace(temp, $@"\b{v.Key}\b", v.Value);

            try
            {
                return Convert.ToBoolean(new DataTable().Compute(temp, null));
            }
            catch
            {
                ReportError($"Cannot evaluate condition: '{cond}'", ln);
                return false;
            }
        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            // بنحدد إن البرنامج يحفظ بصيغة .neo الخاصة بيك
            saveFileDialog.Filter = "NeoScript Files (*.neo)|*.neo|All Files (*.*)|*.*";
            saveFileDialog.Title = "Save your NeoScript Code";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                // السطر ده هو اللي بياخد الكلام من المحرر ويحطه في الملف
                System.IO.File.WriteAllText(saveFileDialog.FileName, rtbCode.Text);
                MessageBox.Show("Saved Successfully! عاش يا مروان", "Success");
            }
        }

        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "NeoScript Files (*.neo)|*.neo";

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                // السطر ده بيقرأ الملف ويرجعه للمحرر عشان تعدل عليه
                string code = System.IO.File.ReadAllText(openFileDialog.FileName);
                rtbCode.Text = code;
                txtOutput.Clear();
                txtOutput.AppendText($"[System]: Loaded file: {openFileDialog.SafeFileName}\r\n");
            }
        }
    }
}