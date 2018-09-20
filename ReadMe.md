# nRF80001 Windows GATT client
Connecting to BLE UART via GATT notification through Nordic 80001 board.

## Requirements
* Windows 10
* Visual Studio 2017 Community Edition with UWP support
* Bluetooth adapter supporting minimum BT 4.0, better if supports 4.3 or 5.0
* Nordic device programmed with name BLEtest, or change name in code.
* Nordic device must be paired ahead of use.

## Running sample
Once code is compiled a sample can be executed, it has minimalistic UI where connection events will be written along with Notify data changes in Base64 string format.
Main lib source code is located under NewLib.cs
Lib binding is done under MainPage.xaml.cs

