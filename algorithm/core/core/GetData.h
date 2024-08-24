#pragma once
#include <string>

using std::string;

class GetData
{
public:
	double GetDeviceData(string AccX);
	double GetDeviceData(string AccY);
	double GetDeviceData(string AccZ);
	double GetDeviceData(string AsX);
	double GetDeviceData(string AsY);
	double GetDeviceData(string AsZ);
	double GetDeviceData(string ChipTime);
};

