C:
cd \
MD "C:\ServiceRoot\KofaxTimerService"
Cd "C:\ServiceRoot\KofaxTimerService"

net stop CLUK.KofaxTimerService

"C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe" -u "C:\ServiceRoot\KofaxTimerService\KofaxTimerService.exe"


CD "C:\Users\BIO187\Source\Repos\KnowMe\BasicOnes\KofaxTimerService\bin\Debug\" 
xcopy /e /c /q /y *.*  "C:\ServiceRoot\KofaxTimerService\" 

CD "C:\Program Files (x86)\Microsoft Visual Studio 12.0" 
"C:\Windows\Microsoft.NET\Framework\v4.0.30319\installutil.exe" -i "C:\ServiceRoot\KofaxTimerService\KofaxTimerService.exe"

net start CLUK.KofaxTimerService

PAUSE