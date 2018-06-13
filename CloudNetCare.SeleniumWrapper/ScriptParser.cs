using System;
using System.Collections.Generic;
using HtmlAgilityPack;

namespace CloudNetCare.SeleniumWrapper
{
    public enum StepType : int
    {
        Optional = 3,
    }

    /// <summary>
    /// Parsing utility of the Selenium script (html document)
    /// </summary>
    public class ScriptParser
    {
        public static IEnumerable<LocalStep> GetStepListFromScript(string scriptPath)
        {
            var docHtml = new HtmlDocument();
            docHtml.OptionOutputOriginalCase = true;

            docHtml.Load(scriptPath);
            string scenarioName;

            return Parse(docHtml, out scenarioName);
        }

        public static IEnumerable<LocalStep> Parse(HtmlDocument docHtml, out string scenarioName)
        {
            var listAdvanced = new List<LocalStep>();
            scenarioName = "";

            var tables = docHtml.DocumentNode.SelectNodes("//table");
            var rows = tables[0].SelectNodes(".//tr");
            var links = docHtml.DocumentNode.SelectNodes("//link");

            for (var i = 0; i < rows.Count; i++)
            {
                var r = rows[i];
                var cols = r.SelectNodes(".//td");
                var com = string.Empty;
                var tar = string.Empty;
                var val = string.Empty;

                if (i == 0)
                {
                    scenarioName = cols[0].InnerText;
                    continue;
                }

                com = cols[0].InnerText;
                tar = cols[1].InnerHtml.Replace("\n", ""); //behavior detected in case of "target" or "value" containing html, such as <br/> => wich is transformed to \n<br> in the InnerHtml. However the InnetText Poperty doesn't include at all non-encoded html.
                val = cols[2].InnerHtml.Replace("\n", "");

                //Check if optional step
                var optionalStep = false;
                var stepName = "";
                var condition = "";
                var browsers = "";
                int? timeBetweenSteps = null;
                int? timedOutValue = null;

                const string optionalComment = "optional";

                var previousSibling = r.PreviousSibling;
                while (previousSibling != null && previousSibling.Name.ToLower() != "tr")
                {
                    if (previousSibling.Name.ToLower() == "#comment")
                    {
                        var comment = previousSibling.InnerHtml.Replace("<!--", "").Replace("-->", "");
                        if (comment.Contains(optionalComment))
                            optionalStep = true;

                        if (!String.IsNullOrEmpty(comment.Trim()))
                        {
                            try
                            {
                                if (comment.Trim().Contains("timeBetweenSteps=")) timeBetweenSteps = Convert.ToInt32(comment.Trim().Replace("timeBetweenSteps=", "").Trim());
                                if (comment.Trim().Contains("timedOutValue=")) timedOutValue = Convert.ToInt32(comment.Trim().Replace("timedOutValue=", "").Trim());
                                if (comment.Trim().Contains("condition=")) condition = comment.Trim().Replace("condition=", "").Trim();
                                if (comment.Trim().Contains("browsers=")) browsers = comment.Trim().Replace("browsers=", "").Trim();
                            }
                            catch (Exception)
                            {
                            }
                        }
                    }
                    previousSibling = previousSibling.PreviousSibling;
                }

                int? stepType = null;

                if (optionalStep) stepType = (int)StepType.Optional;

                var local = new LocalStep(com, tar, val, stepType, stepName, timeBetweenSteps, timedOutValue, browsers, condition);

                listAdvanced.Add(local);
            }
            return listAdvanced;
        }

    }
}
