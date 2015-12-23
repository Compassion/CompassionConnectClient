# CompassionConnectClient
.Net NuGet packages that allow connecting to GMC's Compassion Connect REST web services. 

## Installation
1. Add Compassion Australia's [MyGet](https://www.myget.org/) feed as a NuGet package source in Visual Studio.
  - Source URL: https://www.myget.org/F/compassion
2. Install the package "Compassion Connect Client". This will also install a dependent package "Compassion Connect Models" which defines the CommunicationKit class.
3. Configure settings in App.config. The NuGet package will try to add six appSettings configuration keys. It may fail, though, in which case these will need to be added manually. The settings are:
  - "CompassionConnectServiceUrl" - This is the URL of the Marshery Compassion Connect web service. Defaults to the test environment ("https://api2.compassion.com/test/ci/v2/").
  - "CompassionConnectServiceTestUrl" - This is the URL of the Mashery test web service, used to check we can access an OAuth protected service. Defaults to "https://api2.compassion.com/TEST/CI/1/".
  - "CompassionConnectServiceTokenUrl" - This is the URL of the Marshery web service for obtaining an OAuth token. Defaults to "https://api2.compassion.com/core/connect/".
  - "CompassionConnectServiceApiKey" - The Mashery API key to access the web service. Default is empty.
  - "CompassionConnectServiceOAuthClientId" - The OAuth Client Id. Default is empty.
  - "CompassionConnectServiceOAuthClientSecret" - The OAuth Client Secret. Default is empty.

## Usage

To use, simply instantiate the CompassionConnectService class. It can be used as a singleton object and should be thread-safe.  

## Issues

 - Testing is yet to be complete. This is not yet production ready. 
