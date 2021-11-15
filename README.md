# Get connected USB

## What does this thing do?

Detects USB peripheral devices inserted or removed after starting this programme,  
printing the information of these devices on the screen.

Example output:

```
Availability:
Caption: Dispositivo di archiviazione di massa USB
ClassCode:
ConfigManagerErrorCode: 0
ConfigManagerUserConfig: False
CreationClassName: Win32_USBHub
CurrentAlternateSettings:
CurrentConfigValue:
Description: Dispositivo di archiviazione di massa USB
DeviceID: USB\VID_23A9&PID_EF18\6&2DEEA478&0&5
ErrorCleared:
ErrorDescription:
GangSwitched:
InstallDate:
LastErrorCode:
Name: Dispositivo di archiviazione di massa USB
NumberOfConfigs:
NumberOfPorts:
PNPDeviceID: USB\VID_23A9&PID_EF18\6&2DEEA478&0&5
PowerManagementCapabilities:
PowerManagementSupported:
ProtocolCode:
Status: OK
StatusInfo:
SubclassCode:
SystemCreationClassName: Win32_ComputerSystem
SystemName: DESKTOP-100E04S
USBVersion:
```

## Versions

```
Microsoft Visual Studio Community 2019
Versione 16.10.1
VisualStudio.16.Release/16.10.1+31402.337
Microsoft .NET Framework
Versione 4.8.04084

Edizione installata: Community

Visual C++ 2019   00435-60000-00000-AA167
Microsoft Visual C++ 2019
```

## TODO

- [X] prints USB key info when it is *inserted* into the PC;

- [X] prints USB key info when it is *removed* from the PC;

- [ ] print the USB keys already inserted in the PC.
