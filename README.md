# QEMU VM Spoofing for SafeExamBrowser (SEB) on Linux

This project aims to provide a way to run SafeExamBrowser (SEB) on Linux by using QEMU to spoof the system into thinking it is running on a physical machine. The goal is to bypass virtual machine detection in SEB, which is used in exam environments, without resorting to traditional cheating methods.

Disclaimer: This patch was not developed for cheating purposes, but to prevent people from running Windows from external SSDs during exams. It helps in making SEB less prone to detecting virtual machines, ensuring that users can take exams under controlled conditions.

## Why This Repository Exists

SafeExamBrowser (SEB) is designed to secure exam environments by preventing users from accessing other applications or browsers during an exam. One of its features is the ability to detect if the exam is being taken in a virtual machine. This repository aims to help overcome this detection mechanism by spoofing the virtual machine as a physical machine using QEMU. This allows SEB to run on a Linux host without detection and ensures a smoother exam-taking experience on a Linux machine.

After applying this patch, you will be able to install Windows and take your exam as if you were using a physical machine.

## Build QEMU

Download QEMU source from https://github.com/qemu/qemu with:

```sh
git clone https://github.com/qemu/qemu --depth=1
```

If you want to reduce build time (and build only for x86_64):

```sh
./configure --target-list=x86_64-softmmu
ninja -C build qemu-system-x86_64
```

## Run QEMU for SEB:

Once QEMU is built, you will use it to run a Windows virtual machine that SEB can run inside. The process consists of two steps:
1) **Install Windows** (first run with the Windows ISO).
2) **Re-run the VM** (after installation) without the ISO to boot into the installed Windows.

### Step 1: Create a Virtual Disk

To begin, create a directory for the virtual machines and create a virtual disk for Windows:

```sh
mkdir -p /home/$USER/VMs
qemu-img create -f qcow2 /home/$USER/VMs/win10.qcow2 80G
```

### Step 2: Boot Windows Installation

For the first boot, use the Windows ISO to install Windows on the virtual machine:

```sh
./qemu-system-x86_64 -enable-kvm -m 8G -smp 8 -machine q35 -cpu host,kvm=off,+aes,+sse4.2,-hypervisor \
                    -smbios type=0,vendor="Dell Inc.",version="2.7.0" \
                    -smbios type=1,manufacturer="Dell Inc.",product="Precision T7910" \
                    -smbios type=2,manufacturer="Dell Inc.",product="0KWVT8" \
                    -smbios type=3,manufacturer="Dell Inc.",version="2.7.0" \
                    -drive file=<Windows_ISO>,media=cdrom,id=disk1,if=none,index=0,format=raw \
                    -device ide-cd,drive=disk1,bus=ide.1 \
                    -drive file=/home/$USER/VMs/win10.qcow2,id=disk0,if=none,index=0,format=qcow2,index=1 \
                    -device ide-hd,drive=disk0,bus=ide.0,model=VIRTUAL_DISK,serial="VIRTUAL_DISK_001" \
                    -netdev user,id=net0 \
                    -device e1000e,netdev=net0,mac=00:11:22:33:44:55,subsys_ven=348,subsys=6173 \
                    -global ICH9-LPC.acpi-pci-hotplug-with-bridge-support=off \
                    -global scsi-hd.product="VIRTUAL_DISK_HD_002",device_id="VIRTUAL_DISK_HD_007" \
                    -global scsi-block.product="VIRTUAL_DISK_HD_003",device_id="VIRTUAL_DISK_HD_006" \
                    -global scsi-generic.product="VIRTUAL_DISK_HD_004",device_id="VIRTUAL_DISK_HD_005" \
                    -rtc base=localtime
```

### Step 3: Run Windows After Installation

Once Windows is installed, shut down the virtual machine and remove the Windows ISO. You can now boot into the installed system without the ISO:

```sh
./qemu-system-x86_64 -enable-kvm -m 8G -smp 8 -machine q35 -cpu host,kvm=off,+aes,+sse4.2,-hypervisor \
                    -smbios type=0,vendor="Dell Inc.",version="2.7.0" \
                    -smbios type=1,manufacturer="Dell Inc.",product="Precision T7910" \
                    -smbios type=2,manufacturer="Dell Inc.",product="0KWVT8" \
                    -smbios type=3,manufacturer="Dell Inc.",version="2.7.0" \
                    -drive file=/home/$USER/VMs/win10.qcow2,id=disk0,if=none,index=0,format=qcow2,index=1 \
                    -device ide-hd,drive=disk0,bus=ide.0,model=VIRTUAL_DISK,serial="VIRTUAL_DISK_001" \
                    -netdev user,id=net0 -device e1000e,netdev=net0,mac=00:11:22:33:44:55,subsys_ven=348,subsys=6173 \
                    -global ICH9-LPC.acpi-pci-hotplug-with-bridge-support=off \
                    -global scsi-hd.product="VIRTUAL_DISK_HD_002",device_id="VIRTUAL_DISK_HD_007" \
                    -global scsi-block.product="VIRTUAL_DISK_HD_003",device_id="VIRTUAL_DISK_HD_006" \
                    -global scsi-generic.product="VIRTUAL_DISK_HD_004",device_id="VIRTUAL_DISK_HD_005" \
                    -rtc base=localtime
```

### Reference

- [SEB VMDetector code](https://github.com/SafeExamBrowser/seb-win-refactoring/blob/master/SafeExamBrowser.Monitoring/VirtualMachineDetector.cs)
