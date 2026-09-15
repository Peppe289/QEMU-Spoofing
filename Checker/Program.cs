using Microsoft.Win32;
using SpoofingChecker;
using System.Management;
using System.Runtime.CompilerServices;
using Registry = SpoofingChecker.Registry;

string MANIPULATED = "000000000000";
string QEMU_MAC_PREFIX = "525400";
string VIRTUALBOX_MAC_PREFIX = "080027";

SystemInfo systemInfo = new SystemInfo(new Registry());
Registry registry = new Registry();

string[] SystemHardware =
        {
            "CIM_Memory",
            "CIM_NumericSensor",
            "CIM_Sensor",
            "CIM_TemperatureSensor",
            "CIM_VoltageSensor",
            "Win32_CacheMemory",
            "Win32_Fan",
            "Win32_VoltageProbe"
        };

string[] DeviceWhitelist =
        {
			// Microsoft Virtual Disk Device
			"PROD_VIRTUAL_DISK",
			// Microsoft Virtual DVD Device
			"PROD_VIRTUAL_DVD"
        };

string[] DeviceBlacklist =
        {
			// Hyper-V
			"PROD_VIRTUAL", "HYPER_V",
			// QEMU
			"qemu", "ven_1af4", "ven_1b36", "subsys_11001af4",
			// VirtualBox
			"VEN_VBOX", "vid_80ee",
			// VMware
			"PROD_VMWARE", "VEN_VMWARE", "VMWARE_IDE"
        };

bool HasNoSystemHardware()
{
    var hasHardware = false;

    try
    {
        foreach (var hardware in SystemHardware)
        {
            using (var searcher = new ManagementObjectSearcher($"SELECT * FROM {hardware}"))
            using (var results = searcher.Get())
            {
                hasHardware |= results.Count > 0;
            }
        }
    }
    catch (Exception e)
    {
        hasHardware = false;
        Console.WriteLine(e.ToString());
    }

    return !hasHardware;
}

bool HasVirtualDevice() {
    var hasVirtualDevice = false;

    foreach (var device in systemInfo.PlugAndPlayDeviceIds)
    {
        var result = DeviceBlacklist.Any(d => device.ToLower().Contains(d.ToLower())) && DeviceWhitelist.All(d => !device.ToLower().Contains(d.ToLower()));
        if(result) Console.WriteLine($"Result about Device ID: {device} is {result}");
        hasVirtualDevice |= result;
    }

    return hasVirtualDevice;
}

bool HasVirtualMacAddress()
{
    var hasVirtualMacAddress = false;
    var macAddress = systemInfo.MacAddress;

    if (macAddress != null && macAddress.Length > 2)
    {
        hasVirtualMacAddress |= macAddress.StartsWith(MANIPULATED);
        hasVirtualMacAddress |= macAddress.StartsWith(QEMU_MAC_PREFIX);
        hasVirtualMacAddress |= macAddress.StartsWith(VIRTUALBOX_MAC_PREFIX);
    }

    return hasVirtualMacAddress;
}

bool IsVirtualCpu()
{
    var isVirtualCpu = false;

    isVirtualCpu |= systemInfo.CpuName.ToLower().Contains(" kvm ");

    return isVirtualCpu;
}


bool IsVirtualRegistry()
{
    var isVirtualRegistry = false;

    isVirtualRegistry |= HasLocalVirtualMachineDeviceCache();

    return isVirtualRegistry;
}

bool IsVirtualSystem(string biosInfo, string manufacturer, string model)
{
    var isVirtualSystem = false;

    biosInfo = biosInfo.ToLower();
    manufacturer = manufacturer.ToLower();
    model = model.ToLower();

    isVirtualSystem |= biosInfo.Contains("hyper-v");
    isVirtualSystem |= biosInfo.Contains("virtualbox");
    isVirtualSystem |= biosInfo.Contains("vmware");
    isVirtualSystem |= biosInfo.Contains("ovmf");
    isVirtualSystem |= biosInfo.Contains("edk ii unknown");
    isVirtualSystem |= manufacturer.Contains("microsoft corporation") && !model.Contains("surface");
    isVirtualSystem |= manufacturer.Contains("parallels software");
    isVirtualSystem |= manufacturer.Contains("qemu");
    isVirtualSystem |= manufacturer.Contains("vmware");
    isVirtualSystem |= model.Contains("virtualbox");
    isVirtualSystem |= model.Contains("Q35 +");

    return isVirtualSystem;
}

bool HasLocalVirtualMachineDeviceCache()
{
    var deviceName = Environment.GetEnvironmentVariable("COMPUTERNAME");
    var hasDeviceCache = false;
    var hasDeviceCacheKeys = registry.TryGetSubKeys(RegistryValue.UserHive.DeviceCache_Key, out var deviceCacheKeys);

    if (deviceName != default && hasDeviceCacheKeys)
    {
        foreach (var cacheId in deviceCacheKeys)
        {
            var cacheIdKey = $@"{RegistryValue.UserHive.DeviceCache_Key}\{cacheId}";
            var didReadKeys = true;

            didReadKeys &= registry.TryRead(cacheIdKey, "DeviceName", out var cacheDeviceName);

            if (didReadKeys && deviceName.ToLower() == ((string)cacheDeviceName).ToLower())
            {
                didReadKeys &= registry.TryRead(cacheIdKey, "DeviceMake", out var cacheDeviceManufacturer);
                didReadKeys &= registry.TryRead(cacheIdKey, "DeviceModel", out var cacheDeviceModel);

                if (didReadKeys)
                {
                    hasDeviceCache |= IsVirtualSystem("", (string)cacheDeviceManufacturer, (string)cacheDeviceModel);
                }
            }
        }
    }

    return hasDeviceCache;
}

bool IsVirtualMachine()
{
    var isVm = false;
    isVm |= HasNoSystemHardware();
    isVm |= HasVirtualDevice();
    isVm |= HasVirtualMacAddress();
    isVm |= IsVirtualCpu();
    isVm |= IsVirtualRegistry();
    isVm |= IsVirtualSystem(systemInfo.BiosInfo, systemInfo.Manufacturer, systemInfo.Model);

    return isVm;
}


Console.Write($"Risultato operazione: {IsVirtualMachine()}");