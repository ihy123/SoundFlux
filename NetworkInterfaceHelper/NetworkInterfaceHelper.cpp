#define WIN32_LEAN_AND_MEAN
#define NOATOM
#define NOKERNEL
#define NOMEMMGR
#define NOSERVICE
#define NORPC
#include <Windows.h>
#include <WinSock2.h>
#include <ws2ipdef.h>
#include <iphlpapi.h>

#pragma comment(lib, "Iphlpapi.lib")

extern "C" __declspec(dllexport)
bool IsHardwareNetworkInterface(unsigned long interfaceIndex) {
	MIB_IF_ROW2 row;
	row.InterfaceLuid.Value = 0;
	row.InterfaceIndex = interfaceIndex;
	if (NO_ERROR == GetIfEntry2(&row))
		return row.InterfaceAndOperStatusFlags.HardwareInterface;
	return false;
}
