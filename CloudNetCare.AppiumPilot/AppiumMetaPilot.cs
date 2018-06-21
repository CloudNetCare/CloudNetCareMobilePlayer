using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using CloudNetCare.AppiumPilot.Builders;
using OpenQA.Selenium;
using OpenQA.Selenium.Appium.Interfaces;
using OpenQA.Selenium.Appium.MultiTouch;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.UI;

namespace CloudNetCare.AppiumPilot
{
    internal class AppiumMetaPilot
    {
        private readonly DeviceTarget _deviceTarget;
        private readonly string _packagePath;
        private readonly Uri _appiumServerUri;
        private readonly string _aaptExePath;

        public RemoteWebDriver AppiumDriver { get;  }
        private readonly string _installedPackageName;

        private bool UnInstallOnDriverQuit { get; }
        public TimeSpan CommandTimeout { get; set; } = TimeSpan.FromSeconds(300);

        private IInteractsWithApps AppInteraction => (IInteractsWithApps)AppiumDriver;
        private IPerformsTouchActions TouchActions => (IPerformsTouchActions)AppiumDriver;
        private IRotatable Rotatable => (IRotatable)AppiumDriver;


        /// <summary>
        /// 
        /// </summary>
        /// <param name="deviceTarget"></param>
        /// <param name="packagePath">Mobile applpication package path</param>
        /// <param name="appiumHost">Host name/ip address of appium service</param>
        /// <param name="appiumPort">Appium service port</param>
        /// <param name="aaptExePath">Path of aapt.exe file</param>
        public AppiumMetaPilot(DeviceTarget deviceTarget, string packagePath, string appiumHost, int appiumPort, string aaptExePath)
        : this (deviceTarget, packagePath, new Uri($"http://{appiumHost}:{appiumPort}/wd/hub"), aaptExePath)
        {
        }

        public AppiumMetaPilot(DeviceTarget deviceTarget, string packagePath, Uri appiumServerUri, string aaptExePath)
        {
            _deviceTarget = deviceTarget;
            _packagePath = packagePath;
            _appiumServerUri = appiumServerUri;
            _aaptExePath = aaptExePath;
            _installedPackageName = "";
            UnInstallOnDriverQuit = true;
            AppiumDriver = null;

            Console.WriteLine($"Start AppiumPilot deviceTarget:{deviceTarget} package:{packagePath}");

            if (deviceTarget.Platform == DevicePlatform.Other) return;

            try
            {
                if (!IsUp())
                {
                    Console.WriteLine("Appium is not running !");
                    return;
                }


                //TODO ADD IMPLEMENTATION FOR iOS EMULATOR + REAL devices

                if (!deviceTarget.IsRealDevice && deviceTarget.Platform == DevicePlatform.Android)
                {
                    try
                    {
                        var androidDriverBuilder = new AndroidDriverBuilder();

                        AppiumDriver = androidDriverBuilder
                                                .DeviceTarget(deviceTarget)
                                                .AaptExePath(aaptExePath)
                                                .AppiumUri(appiumServerUri)
                                                .CommandTimeout(CommandTimeout)
                                                .PackagePath(packagePath)
                                                .Build();

                        _installedPackageName = androidDriverBuilder.InstalledPackageName;

                        AppiumDriver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
                        Thread.Sleep(10 * 1000);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine("Error launching device : " + e.Message);
                        return;
                    }
                }
                else
                {
                    AppiumDriver = null;
                }
            }

            catch (Exception e)
            {
                Console.WriteLine("An error occured : " + e);
                AppiumDriver = null;
            }
        }



        private static bool IsBrowser(string packagePath)
        {
            return !(packagePath.EndsWith(".app.zip") || packagePath.EndsWith(".ipa") || packagePath.EndsWith(".apk"));
        }

        public bool IsUp()
        {
            Uri requestUri = new Uri(_appiumServerUri, "hub/status");

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUri);
            request.Method = "GET";
            request.ContentType = "application/json";
            HttpWebResponse response;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
            }
            catch (WebException ex)
            {
                Console.WriteLine("Appium Down : " + ex.Message);
                return false;
            }

            string returnValue = "Error:";
            if (response.StatusCode == System.Net.HttpStatusCode.OK && response.ContentLength > 0)
            {
                // Parse the web response.
                Stream responseStream = response.GetResponseStream();
                var reader = new StreamReader(responseStream);
                returnValue = reader.ReadToEnd();
                responseStream.Close();
                reader.Close();
                response.Close();
                if (returnValue.Contains("\"version\"")) { Console.WriteLine("Appium Up!"); return true; }
                else { Console.WriteLine("Appium Down"); return false; }
            }
            else
            {
                Console.WriteLine("Appium Down");
                response.Close();
                return false;
            }
        }

        public void DriverQuit(bool forceNoUninstall = false)
        {
            Thread.Sleep(1000);

            try
            {
                if (_deviceTarget.Platform == DevicePlatform.IOs)
                {
                    if (IsBrowser(_aaptExePath)) AppiumDriver.Close();
                    else AppInteraction.CloseApp();
                    AppiumDriver.Dispose();
                    AppiumDriver.Quit();
                }

                if (_deviceTarget.Platform == DevicePlatform.Android)
                {
                    if (IsBrowser(_packagePath)) AppiumDriver.Close();
                    else
                    {
                        AppInteraction.CloseApp();
                        if (!String.IsNullOrEmpty(_installedPackageName) && UnInstallOnDriverQuit)
                        {
                            Console.WriteLine("Uninstall app : " + _installedPackageName);
                            AppInteraction.RemoveApp(_installedPackageName);
                        }
                        else
                            Console.WriteLine("INFO : app not uninstalled !");
                    }
                    if (!forceNoUninstall) AppiumDriver.Dispose();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }


        public CommandReturn BackGroundApp(string target, string value)
        {
            var returnCommand = new CommandReturn();
            try
            {
                int time = 1;
                bool res = int.TryParse(target, out time);

                if (res)
                {
                    AppInteraction?.BackgroundApp(time);
                    returnCommand.resultValue = ResultValue.COMMAND_OK;
                }
                else
                {
                    returnCommand.resultValue = ResultValue.ERROR;
                    returnCommand.message = "wrong parameters : time in seconds awaited for target";
                }

            }
            catch (Exception ex)
            {
                returnCommand.resultValue = ResultValue.ERROR;
                returnCommand.message = ex.Message;
            }

            return returnCommand;
        }

        public CommandReturn StoreVisible(string target, string value, ref Dictionary<string, string> seleniumVariables)
        {
            var returnValue = new CommandReturn();

            try
            {
                string errorMessage = "";
                var element = _GetElement(target, ref errorMessage);

                if (seleniumVariables.ContainsKey(value)) seleniumVariables.Remove(value);

                if (element != null)
                {
                    if (element.Displayed)
                        seleniumVariables.Add(value, "true");
                    else
                        seleniumVariables.Add(value, "false");
                }
                else
                {
                    seleniumVariables.Add(value, "false");
                }
                returnValue.resultValue = ResultValue.COMMAND_OK;
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }

            return returnValue;
        }

        public CommandReturn Click(string target)
        {
            var returnValue = new CommandReturn();

            try
            {

                string errorMessage = "";
                var element = GetElement(target, ref errorMessage);

                if (element != null)
                {
                    Thread.Sleep(2000);

                    if (element.Displayed)
                    {
                        element.Click();
                    }
                    else
                    {
                        var location = element.Location;
                         Tapxy(TouchActions, 1, location.X, location.Y);
                    }


                    returnValue.resultValue = ResultValue.COMMAND_OK;
                }
                else
                {
                    returnValue.resultValue = ResultValue.ERROR;
                    returnValue.message = errorMessage;
                }
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }
            return returnValue;
        }

        public void Tapxy(IPerformsTouchActions multitouchPerformer, int fingers, int x, int y)
        {
            MultiAction multiTouch = new MultiAction(multitouchPerformer);

            for (int i = 0; i < fingers; i++)
            {
                multiTouch.Add(new TouchAction(multitouchPerformer).Tap(x, y));
            }

            multiTouch.Perform();
        }

        public void Tap(IPerformsTouchActions multitouchPerformer, int fingers, IWebElement webElement)
        {
            MultiAction multiTouch = new MultiAction(multitouchPerformer);

            for (int i = 0; i < fingers; i++)
            {
                multiTouch.Add(new TouchAction(multitouchPerformer).Tap(webElement));
            }

            multiTouch.Perform();
        }

        public void Swipe(IPerformsTouchActions touchActionsPerformer,int startx, int starty, int endx, int endy, int duration)
        {
            TouchAction touchAction = new TouchAction(touchActionsPerformer);

            // appium converts Press-wait-MoveTo-Release to a swipe action
            touchAction.Press(startx, starty).Wait(duration)
                .MoveTo(endx, endy).Release();

            touchAction.Perform();
        }

        public CommandReturn Tap(string target, string value)
        {
            var returnValue = new CommandReturn();

            try
            {
                int nbOfFingers = 1;
                try
                {
                    nbOfFingers = String.IsNullOrEmpty(value) ? 1 : Convert.ToInt32(value);
                }
                catch
                {
                }

                if (target.StartsWith("{x:"))
                {
                    dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(target);

                    int x = jsonResponse.x;
                    int y = jsonResponse.y;

                    Thread.Sleep(2000);

                    Tapxy(TouchActions, nbOfFingers, x, y);

                    returnValue.resultValue = ResultValue.COMMAND_OK;
                }
                else
                {
                    string errorMessage = "";

                    var element = GetElement(target, ref errorMessage);
                    if (element != null)
                    {
                        Thread.Sleep(2000);

                        Tap(TouchActions, nbOfFingers, element);

                        returnValue.resultValue = ResultValue.COMMAND_OK;
                    }
                    else
                    {
                        returnValue.resultValue = ResultValue.ERROR;
                        returnValue.message = errorMessage;
                    }
                }
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }
            return returnValue;
        }

        public CommandReturn SelectAndWait(string target, string value, int timeout)
        {
            var returnValue = new CommandReturn();

            try
            {
                string errorMessage = "";
                var element = GetElement(target, ref errorMessage);
                if (element != null)
                {
                    Thread.Sleep(2000);

                    var selectElementW = new SelectElement(element);
                    SelectOption(selectElementW, value);
                    returnValue.resultValue = ResultValue.COMMAND_OK;
                }
                else
                {
                    returnValue.resultValue = ResultValue.ERROR;
                    returnValue.message = errorMessage;
                }
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }
            return returnValue;
        }

        public void SelectOption(SelectElement selectElement, string locator)
        {
            var optionLocator = GetOptionLocator(ref locator);
            switch (optionLocator)
            {
                case OptionLocator.ByText:
                    selectElement.SelectByText(locator);
                    break;
                case OptionLocator.ByValue:
                    selectElement.SelectByValue(locator);
                    break;
                case OptionLocator.ById:
                case OptionLocator.ByIndex:
                    selectElement.SelectByIndex(Convert.ToInt32(locator));
                    break;
            }
        }

        public CommandReturn WaitForVisible(string target, string value, int timeout)
        {
            var returnValue = new CommandReturn();

            try
            {
                var stopwatch = Stopwatch.StartNew();
                stopwatch.Restart();
                string errorMessage = "";
                var firstLoop = true;
                IWebElement element;

                do
                {
                    element = _GetElement(target, ref errorMessage);

                    if (element == null || !element.Displayed)
                        Thread.Sleep(500);
                    else
                        break;

                    if (firstLoop) firstLoop = false;
                } while (stopwatch.ElapsedMilliseconds < Convert.ToDouble(timeout));

                if (element == null || !element.Displayed)
                {
                    returnValue.resultValue = ResultValue.ERROR;
                    returnValue.message = "Timed out before finding the target element";
                }
                else
                {
                    returnValue.resultValue = ResultValue.COMMAND_OK;
                }
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }

            return returnValue;
        }

        public CommandReturn SendKeys(string target, string value)
        {
            var returnValue = new CommandReturn();

            try
            {
                string errorMessage = "";
                var element = GetElement(target, ref errorMessage);

                if (element != null)
                {
                    Thread.Sleep(2000);


                    if (element.Displayed)
                    {
                        element.SendKeys(value);
                    }
                    else
                    {
                        returnValue.resultValue = ResultValue.ERROR;
                        return returnValue;
                    }
                    returnValue.resultValue = ResultValue.COMMAND_OK;
                }
                else
                {
                    returnValue.resultValue = ResultValue.ERROR;
                    returnValue.message = errorMessage;
                }
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }
            return returnValue;
        }

        public CommandReturn DeviceOrientation(string orientation)
        {
            var returnValue = new CommandReturn();

            try
            {
                Rotatable.Orientation = orientation.ToLower() == "landscape" ? ScreenOrientation.Landscape : ScreenOrientation.Portrait;

                returnValue.resultValue = ResultValue.COMMAND_OK;
                return returnValue;
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }
            return returnValue;
        }

        public CommandReturn VerifyElementPresent(string target, string value)
        {
            var returnValue = new CommandReturn();

            try
            {
                var result = false;
                string errorMessage = "";
                var element = GetElement(target, ref errorMessage);
                result = element != null;

                returnValue.resultValue = result ? ResultValue.VERIFICATION_OK : ResultValue.VERIFICATION_ERROR;
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }
            return returnValue;
        }

        public CommandReturn VerifyText(string target, string value)
        {
            var returnValue = new CommandReturn();
            bool result;

            try
            {
                string errorMessage = "";
                var element = GetElement(target, ref errorMessage);

                if (element == null)
                {
                    returnValue.resultValue = ResultValue.ERROR;
                    returnValue.message = errorMessage;
                    return returnValue;
                }
                var nameAtt = element.GetAttribute("name");

                if (value.Contains("regexp:"))
                {
                    var pattern = @value.Replace("regexp:", "");
                    var regex = new System.Text.RegularExpressions.Regex(pattern);
                    result = regex.IsMatch(element.Text.TrimStart(' ').TrimEnd(' '));
                    var textDisplayed = element.Text;
                    if (!result)
                    {

                        if (nameAtt != null)
                        {
                            var alternativText = element.GetAttribute("name");
                            result = regex.IsMatch(alternativText.TrimStart(' ').TrimEnd(' '));
                            textDisplayed = alternativText;
                        }
                        if (!result) returnValue.message = textDisplayed + " doesn't match with [" + value + "]";
                    }
                }
                else if (value.Contains("regexpi:"))
                {
                    var pattern = @value.Replace("regexpi:", "").ToLowerInvariant();
                    var regex = new System.Text.RegularExpressions.Regex(pattern);
                    result = regex.IsMatch(element.Text.ToLowerInvariant().TrimStart(' ').TrimEnd(' '));
                    var textDisplayed = element.Text;
                    if (!result)
                    {
                        if (nameAtt != null)
                        {
                            var alternativText = element.GetAttribute("name");
                            result = regex.IsMatch(alternativText.TrimStart(' ').TrimEnd(' '));
                            textDisplayed = alternativText;
                        }
                        if (!result) returnValue.message = textDisplayed + " doesn't match with [" + value + "]";
                    }
                }
                else
                {
                    result = String.Equals(element.Text.ToLowerInvariant().TrimStart(' ').TrimEnd(' '), value.ToLowerInvariant().TrimStart(' ').TrimEnd(' '));
                    var textDisplayed = element.Text;
                    if (!result)
                    {
                        if (nameAtt != null)
                        {
                            var alternativText = element.GetAttribute("name");
                            result = String.Equals(alternativText.ToLowerInvariant().TrimStart(' ').TrimEnd(' '), value.ToLowerInvariant().TrimStart(' ').TrimEnd(' '));
                            textDisplayed = alternativText;
                        }
                        if (!result) returnValue.message = textDisplayed + " doesn't match with [" + value + "]";
                    }
                }
                returnValue.isOk = result; 
                returnValue.resultValue = result ? ResultValue.VERIFICATION_OK : ResultValue.VERIFICATION_ERROR;
                return returnValue;
            }
            catch (Exception e)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = e.Message;
            }
            return returnValue;
        }

        public CommandReturn Swipe(string target)
        {
            var returnValue = new CommandReturn();

            try
            {
                dynamic jsonResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(target);

                int startx = jsonResponse.startx;
                int starty = jsonResponse.starty;
                int endx = jsonResponse.endx;
                int endy = jsonResponse.endy;
                int duration = jsonResponse.duration;

                 Swipe(TouchActions, startx, starty, endx, endy, duration);

                returnValue.resultValue = ResultValue.COMMAND_OK;

                Thread.Sleep(500);
            }
            catch (Exception ex)
            {
                returnValue.resultValue = ResultValue.ERROR;
                returnValue.message = "Swipe failed : " + ex.Message;
            }
            return returnValue;
        }

        public CommandReturn PageSource(out string stepValue)
        {
            var returnValue = new CommandReturn();

            string pageSource = "";

            try
            {
                pageSource = AppiumDriver.PageSource;

                stepValue = pageSource;
                returnValue.resultValue = ResultValue.COMMAND_OK;
            }
            catch (Exception)
            {
                stepValue = pageSource;
                returnValue.resultValue = ResultValue.ERROR;
            }
            return returnValue;
        }

        public CommandReturn Echo(string target, string value, out string stepValue)
        {
            var returnValue = new CommandReturn();
            stepValue = target; 
            returnValue.resultValue = ResultValue.COMMAND_OK;

            return returnValue;
        }

        public OptionLocator GetOptionLocator(ref string locator)
        {
            if (locator.ToLower().StartsWith("label="))
            {
                locator = locator.Replace("label=", "");
                return OptionLocator.ByText;
            }
            if (locator.ToLower().StartsWith("value="))
            {
                locator = locator.Replace("value=", "");
                return OptionLocator.ByValue;
            }
            if (locator.ToLower().StartsWith("id="))
            {
                locator = locator.Replace("id=", "");
                return OptionLocator.ById;
            }
            if (locator.ToLower().StartsWith("index="))
            {
                locator = locator.Replace("index=", "");
                return OptionLocator.ByIndex;
            }
            return OptionLocator.ByText; //Default label=
        }

        public enum OptionLocator
        {
            ByIndex,
            ByText,
            ByValue,
            ById
        }


        public IWebElement GetElement(string target, ref string errorMessage)
        {
            var nbOfIterationToWait = 2;

            for (int attempt = 0; attempt < nbOfIterationToWait; attempt++)
            {
                var element = _GetElement(target, ref errorMessage);
                if (element != null)
                    return element;
                else
                    Thread.Sleep(500);
            }
            return null;
        }

        private IWebElement _GetElement(string target, ref string errorMessage)
        {
            try
            {
                var elements = AppiumDriver?.FindElement(GetBy(target));

                return elements;
            }
            catch (Exception e)
            {
                if (errorMessage.Contains(e.Message)) return null;
                errorMessage += (errorMessage.Length > 0 ? " -- " : "") + e.Message;
                return null;
            }
        }

        private static By GetBy(string target)
        {
            By by;

            if (target.StartsWith("css="))
            {
                by = By.CssSelector(target.Substring(4, target.Length - 4));
            }
            else if (target.StartsWith("id="))
            {
                by = By.Id(target.Substring(3, target.Length - 3));
            }
            else if (target.StartsWith("link="))
            {
                by = By.LinkText(target.Substring(5, target.Length - 5));
            }
            else if (target.StartsWith("name="))
            {
                by = By.Name(target.Substring(5, target.Length - 5));
            }
            else if (target.StartsWith("xpath="))
            {
                by = By.XPath(target.Substring(6, target.Length - 6));
            }
            else
            {
                by = By.XPath(target);
            }
            return by;
        }

    }
}
