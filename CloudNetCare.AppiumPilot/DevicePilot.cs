using System;
using System.Collections.Generic;
using System.Threading;
using CloudNetCare.SeleniumWrapper.ExpressionEvaluator;

namespace CloudNetCare.AppiumPilot
{
    public class DevicePilot : IDevicePilot
    {
        private static AppiumMetaPilot _appiumMetaPilot;

        public bool StartDevice (DeviceTarget deviceTarget, string packagePath, string appiumHost, int appiumPort, string aaptPath)
        {
            _appiumMetaPilot = new AppiumMetaPilot(deviceTarget, packagePath, appiumHost, appiumPort, aaptPath);
            return !(_appiumMetaPilot == null|| (_appiumMetaPilot!=null && _appiumMetaPilot.AppiumDriver==null));
        }


        private void DriverQuit()
        {
            _appiumMetaPilot?.DriverQuit();
            _appiumMetaPilot = null;
        }

        public CommandReturn ExecuteCommand(string command, string target, string value, string condition, int? timeout, ref Dictionary<string, string> persistantVariables, out string stepValue)
        {
            stepValue = "";
            CommandReturn ret;

            bool playStep = true;

            // Option : Conditional execution (Selenium condition comment <-- condition=${variable1} == 5 --> )
            if (!String.IsNullOrEmpty(condition))
            {
                // internal variables
                foreach(var variable in persistantVariables)
                {
                    condition = condition.Replace("${" + variable.Key + "}", variable.Value);
                }

                // eval condition
                try
                {
                    var expression2 = condition.Replace("\"", "'").ToLower();
                    var expression = new CompiledExpression() { StringToParse = expression2 };
                    var result = expression.Eval();
                    bool? boolResult = result as bool?;
                    playStep = (boolResult ?? true) != false;
                }
                catch (Exception ex)
                {
                    var commandReturn = new CommandReturn();
                    commandReturn.resultValue = ResultValue.ERROR;
                    commandReturn.isOk = false;
                    commandReturn.message = "Expression evaluator error: " + ex.Message;
                    return commandReturn;
                }

            }

            if(!playStep)
            {
                var commandReturn = new CommandReturn();
                commandReturn.resultValue = ResultValue.COMMAND_OK;
                commandReturn.isOk = true;
                commandReturn.message = "Step not played by condition";
                return commandReturn;
            }

            try
            {
                stepValue = "";
                string sPattern = @"\${\w*?}";

                if (System.Text.RegularExpressions.Regex.IsMatch(target, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    if (persistantVariables != null)
                    {
                        var rgx = new System.Text.RegularExpressions.Regex(sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        foreach (System.Text.RegularExpressions.Match m in rgx.Matches(target))
                        {
                            if (!string.IsNullOrWhiteSpace(m.Value))
                            {
                                string varFound = persistantVariables[m.Value.Replace("${", "").Replace("}", "")];
                                target = target.Replace(m.Value, varFound);
                            }
                        }
                    }

                }

                if (System.Text.RegularExpressions.Regex.IsMatch(value, sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                {
                    if (persistantVariables != null)
                    {
                        var rgx = new System.Text.RegularExpressions.Regex(sPattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                        foreach (System.Text.RegularExpressions.Match m in rgx.Matches(value))
                        {
                            if (!string.IsNullOrWhiteSpace(m.Value))
                            {
                                string varFound = persistantVariables[m.Value.Replace("${", "").Replace("}", "")];
                                value = value.Replace(m.Value, varFound);
                            }
                        }
                    }

                }

                if (_appiumMetaPilot == null)
                {
                    var commandReturn = new CommandReturn();
                    commandReturn.resultValue = ResultValue.ERROR;
                    commandReturn.isOk = false;
                    commandReturn.message = "appiumPilot is null";

                    return commandReturn;
                }

                if (_appiumMetaPilot.AppiumDriver == null)
                {
                    var commandReturn = new CommandReturn();
                    commandReturn.resultValue = ResultValue.ERROR;
                    commandReturn.message = "iosDriver & androidDriver are null";
                    return commandReturn;
                }


                switch (command)
                {
                    case "backGroundApp":
                        ret = _appiumMetaPilot.BackGroundApp(target, value);
                        break;
                    case "click":
                    case "clickAndWait":
                        ret = _appiumMetaPilot.Click(target);
                        break;
                    case "sendkeys":
                    case "sendKeys":
                    case "type":
                        ret = _appiumMetaPilot.SendKeys(target, value);
                        break;
                    case "verifyText":
                        ret = _appiumMetaPilot.VerifyText(target, value);
                        break;
                    case "tap":
                        ret = _appiumMetaPilot.Tap(target, value);
                        break;
                    case "waitForElementPresent":
                    case "waitforvisible":
                        ret = _appiumMetaPilot.WaitForVisible(target, value, timeout??30);
                        break;
                    case "deviceOrientation":
                        ret = _appiumMetaPilot.DeviceOrientation(target);
                        break;
                    case "echo":
                        ret = _appiumMetaPilot.Echo(target, value, out stepValue);
                        break;
                    case "storeVisible":
                        ret = _appiumMetaPilot.StoreVisible(target, value, ref persistantVariables);
                        break;
                    case "swipe":
                        ret = _appiumMetaPilot.Swipe(target);
                        break;
                    case "selectAndWait":
                        ret = _appiumMetaPilot.SelectAndWait(target, value, timeout??30);
                        break;
                    case "assertElementPresent":
                    case "verifyElementPresent":
                        ret = _appiumMetaPilot.VerifyElementPresent(target, value);
                        break;
                    case "pause":
                    case "Pause":
                        var returnValue = new CommandReturn();
                        if (!int.TryParse(target, out var secs))
                        {
                            secs = 0;
                        }
                        Thread.Sleep(secs);
                        returnValue.resultValue = ResultValue.COMMAND_OK;
                        ret = returnValue;
                        break;
                    case "pageSource":
                        ret = _appiumMetaPilot.PageSource(out stepValue);
                        break;
                    default:
                        ret = new CommandReturn();
                        ret.resultValue = ResultValue.INVALID_COMMAND;
                        break;
                }
                if (ret.resultValue == ResultValue.COMMAND_OK || ret.resultValue == ResultValue.VERIFICATION_OK) ret.isOk = true;
                else ret.isOk = false;
            }
            catch (Exception ex)
            {
                ret = new CommandReturn();
                ret.resultValue = ResultValue.ERROR;
                ret.message = "Error in ExecuteCommand()";
                ret.isOk = false;
            }
            return ret;
        }

        #region IDisposable Support
        private bool disposedValue = false; // Pour détecter les appels redondants

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    DriverQuit();
                }

                // TODO: libérer les ressources non managées (objets non managés) et remplacer un finaliseur ci-dessous.
                // TODO: définir les champs de grande taille avec la valeur Null.

                disposedValue = true;
            }
        }

        // Ce code est ajouté pour implémenter correctement le modèle supprimable.
        public void Dispose()
        {
            // Ne modifiez pas ce code. Placez le code de nettoyage dans Dispose(bool disposing) ci-dessus.
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion

    }
}
