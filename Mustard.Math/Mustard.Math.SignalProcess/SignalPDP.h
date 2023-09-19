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
	/// ��ʼ���㷨
	/// </summary>
	/// <param name="dataLength">�������ݳ���</param>
	/// <param name="dataMode">�����������ͣ�0��Ƶ�ף�1��IQ����<��δ֧��> </param>
	/// <returns>0����ʼ���ɹ���-1���ݲ�֧�֣�-2����������</returns>
	int Init(int dataLength, int dataMode);

	void InputData(double* datas);

	void InputData(float* datas);

	void InputData(short* datas);
};