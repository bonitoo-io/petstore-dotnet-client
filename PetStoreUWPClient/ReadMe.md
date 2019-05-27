# Petstore Windows 10 IoT core demo client

## Build
1. Checkout and build `BuildAzure.IoT.Adafruit.BME280`
1. Checkout and open PetStoreClient
1. Right Click on project name in Solution Explorer and select `Manage NuGet Packages`
1. In the right upper corner click on the gear (seettings) icon
1. Add ApiTea repo: https://apitea.com/nexus/service/local/nuget/bonitoo-nuget/ 
1. Change ULR for PetStore server in MainPage.xaml.cs:316 in this project
1. Select ARM and RemoteClient for deployment
1. Build & Deploy

## Sensors
**BME280** (also BMP180) is I2C slave, so connect I2C of RPI:
 - GND c (GND)
 - SDA to RPI GPIO pin 3 (I2C SDA) 
 - SDC to RPI GPIO pin 5 (I2C SDC) 
 - VIN to RPI GPIO pin 1 (VCC)
 ![rpi_3_pinout](rp3_pinout.png)

**DHT 22**
 - GND to RPI GPIO 20 (GND)
 - VCC to RPI GPIO pin 1 (VCC)
 - DATA to RPI GPIO 17
 - ![DHT 22 pinout](DHT22-PinOut.png)