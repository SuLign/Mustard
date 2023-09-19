#pragma once
#include <ipps.h>
class SignalPDP {
private:
	Ipp8u* hightPassFilterBuf = nullptr;
	Ipp8u* lowPassFilterBuf = nullptr;
	Ipp64f* pHTaps = nullptr;
	IppsFIRSpec_64f* pHSpec = nullptr;
	Ipp64f* pLTaps = nullptr;
	IppsFIRSpec_64f* pLSpec = nullptr;

	int dataLength;

public:
	/// <summary>
	/// 初始化算法
	/// </summary>
	/// <param name="dataLength">输入数据长度</param>
	/// <param name="dataMode">输入数据类型，0：频谱，1：IQ数据<暂未支持> </param>
	/// <returns>0：初始化成功，-1：暂不支持，-2：其他错误</returns>
	int Init(int dataLength, int dataMode);

	void InputData(double* datas);

	void InputData(float* datas);

	void InputData(short* datas);
};