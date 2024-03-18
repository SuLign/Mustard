#pragma once
#include "DX11PCH.h"
#include "Vertex.h"
#include "ShaderCompiler.h"

using namespace DX11Core;

class DX11Chart
{
private:
	P(ID3D11Device)				d3d11Device = 0;
	P(IDXGISwapChain)			d3d11SwapChain = 0;
	P(ID3D11DeviceContext)		d3d11DeviceContext = 0;
	P(ID3D11RenderTargetView)	d3d11RenderTargetView = 0;
	P(DX11ShaderCompiler)		dx11ShaderCompiler = 0;

private:
	HWND						hContainer;


	ID3D11Buffer* vertexBuffer;
	UINT numVerts;
	UINT stride;
	UINT offset;
public:
	void SetBindHwnd(HWND hContainer);

	void InitD3D();

	void Render();
};

