# Pet Store Windows 10 IoT Core Background Job

Main logic of the Pet Store client.

## Building
1. Checkout and build [BuildAzure.IoT.Adafruit.BME280](https://github.com/bonitoo-io/BuildAzure.IoT.Adafruit.BME280)
1. Checkout and open PetStoreClient
1. Fix the reference to the `BuildAzure.IoT.Adafruit.BME280.dll`
1. Right Click on project name in Solution Explorer and select `Manage NuGet Packages`
1. In the right upper corner click on the gear (seettings) icon
1. Add ApiTea repo: https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/ 
1. Select ARM and RemoteClient for deployment
1. Build & Deploy