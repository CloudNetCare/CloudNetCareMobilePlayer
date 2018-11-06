# CloudNetCareMobilePlayer
APPIUM PLAYER FOR SELENIUM SCRIPTS

CloudNetCareMobilePlayer is a console tool designed to play SELENIUM scripts on mobile devices using APPIUM.


Prerequisite to use the player :

- install node.js https://nodejs.org/en/ 
- install appium : launch node.js command prompt and type : npm install -g appium

Prerequisite to use Android Emulator (for example) :

- install AndroidStudio https://developer.android.com/studio/index.html (Save path Android SDK)
- Create ANDROID_HOME environment variable to be your Android SDK
- install JAVA JDK http://www.oracle.com/technetwork/java/javase/downloads/jdk8-downloads-2133151.html

How to use the player :

1 - First you need to have or create a SELENIUM Script with mobiles commands (Tap, Swipe, ...) 
-- (See example: SeleniumScript.html)

2 - Set appsettings.json for your context :
<pre><code>
{
  "ScriptFile": "SeleniumScript.html",
  "PackageFile": "test.apk",
  "DataDir": "C:\\data",

  "DeviceTarget": {
    "Platform": "Android",
    "IsRealDevice": false,
    "DeviceName": "NEXUS5XAPI24",
    "VersionMajor": "7",
    "VersionMinor": "0",
    "VersionSubminor": ""
  },

  "AppiumHost": "127.0.0.1",
  "AppiumPort": "4723",
  "ANDROID_BUILD_TOOLS_VERSION": "27.0.3"
}
</pre></code>

3 - Connect your device (or launch emulator)

4-  Launch amppium : On command prompt : appium &

5 - Start CloudNetCare.AppiumPilot to play the script on the emulator.
	Let the script runs until it ends.

NB: CloudNetCare.AppiumPilot is build for .netCore (play on Windows/Linux/Mac)
CloudNetCare.CloudNetCare.MobilePlayer.Console is a wrapper build for Windows .net 4.6.1.

