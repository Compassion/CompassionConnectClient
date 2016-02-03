@echo Off
set config=%1
if "%config%" == "" (
   set config=Release
)

set version=
if not "%PackageVersion%" == "" (
   set version=-Version %PackageVersion%
)

if "%NuGet%" == "" (
	set nuget=nuget.exe
)

cd src

REM Package restore
call %NuGet% restore CompassionConnectClient\packages.config -OutputDirectory %cd%\packages -NonInteractive
call %NuGet% restore CompassionConnectClient.Tests\packages.config -OutputDirectory %cd%\packages -NonInteractive

REM Build
%WINDIR%\Microsoft.NET\Framework\v4.0.30319\msbuild CompassionConnectClient.sln /p:Configuration="%config%" /m /v:M /fl /flp:LogFile=msbuild.log;Verbosity=Normal /nr:false
if not "%errorlevel%"=="0" goto failure

REM Unit tests
call %nuget% install NUnit.Runners -Version 3.0.1 -OutputDirectory packages
packages\NUnit.Console.3.0.1\tools\nunit3-console.exe /config:%config% /framework:net-4.5 CompassionConnectClient.Tests\bin\%config%\CompassionConnectClient.Tests.dll
if not "%errorlevel%"=="0" goto failure

REM Package
mkdir Build
call %nuget% pack "nuspec\CompassionConnectClient.nuspec" -symbols -o Build -p Configuration=%config% %version%
call %nuget% pack "nuspec\CompassionConnectModels.nuspec" -symbols -o Build -p Configuration=%config% %version%
if not "%errorlevel%"=="0" goto failure

:success
exit 0

:failure
exit -1
