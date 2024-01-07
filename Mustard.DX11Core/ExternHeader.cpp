#include "DX11PCH.h"
#include "ExternHeader.h"

DX11Chart* CreateHandle()
{
    return new DX11Chart();
}

void InitD3D(DX11Chart* chartHandle, HWND handle)
{
    chartHandle->SetBindHwnd(handle);
    chartHandle->InitD3D();
}

void Render(DX11Chart* chartHandle)
{
    chartHandle->Render();
}
