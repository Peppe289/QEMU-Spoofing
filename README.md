# QEMU VM Spoofing 

This patch refers to the well-known Linux virtual machine [QEMU](https://github.com/qemu/qemu). It was created to be used in combination with a boot parameter, with the purpose of spoofing SafeExamBrowser.

_Disclaimer: The patch was not developed for cheating purposes, but to prevent people from using Windows from external SSDs during exams._

## Args:

```sh
-enable-kvm -m 8G -smp 8 -machine q35 -cpu host,kvm=off,+aes,+sse4.2,-hypervisor -smbios type=0,vendor="Dell Inc.",version="2.7.0" -smbios type=1,manufacturer="Dell Inc.",product="Precision T7910" -smbios type=2,manufacturer="Dell Inc.",product="0KWVT8" -smbios type=3,manufacturer="Dell Inc.",version="2.7.0" -drive file=<path_del_tuo_disk.img>,id=disk0,if=none,index=0,format=raw,index=1 -device ide-hd,drive=disk0,bus=ide.0,model=VIRTUAL_DISK,serial="VIRTUAL_DISK_001" -netdev user,id=net0 -device e1000e,netdev=net0,mac=00:11:22:33:44:55,subsys_ven=348,subsys=6173 -global ICH9-LPC.acpi-pci-hotplug-with-bridge-support=off -global scsi-hd.product="VIRTUAL_DISK_HD_002",device_id="VIRTUAL_DISK_HD_007" -global scsi-block.product="VIRTUAL_DISK_HD_003",device_id="VIRTUAL_DISK_HD_006"   -global scsi-generic.product="VIRTUAL_DISK_HD_004",device_id="VIRTUAL_DISK_HD_005" -rtc base=localtime
```
