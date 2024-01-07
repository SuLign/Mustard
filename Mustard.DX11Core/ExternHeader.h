#pragma once
#include "DX11Chart.h"

#define API __declspec(dllexport)

#ifdef __cplusplus
extern "C"
#endif
{
	API DX11Chart* CreateHandle();

	API void InitD3D(DX11Chart* chartHandle, HWND handle);

	API void Render(DX11Chart* chartHandle);
}