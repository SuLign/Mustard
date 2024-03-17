#include "DX11PCH.h"
#include "DX11Chart.h"

void DX11Chart::SetBindHwnd(HWND hContainer)
{
	DXGI_SWAP_CHAIN_DESC scd;
	ZeroMemory(&scd, sizeof(DXGI_SWAP_CHAIN_DESC));
	scd.BufferCount = 1;
	scd.BufferDesc.Format = DXGI_FORMAT_R8G8B8A8_UNORM;
	scd.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	scd.OutputWindow = hContainer;
	scd.SampleDesc.Count = 4;
	scd.Windowed = TRUE;

	D3D11CreateDeviceAndSwapChain(NULL,
		D3D_DRIVER_TYPE_HARDWARE,
		NULL,
		NULL,
		NULL,
		NULL,
		D3D11_SDK_VERSION,
		&scd,
		&d3d11SwapChain,
		&d3d11Device,
		NULL,
		&d3d11DeviceContext);

}

void DX11Chart::InitD3D()
{
	ID3D11Texture2D* pBackBuffer;
	d3d11SwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (LPVOID*)&pBackBuffer);
	if (pBackBuffer == 0)return;
	d3d11Device->CreateRenderTargetView(pBackBuffer, NULL, &d3d11RenderTargetView);
	pBackBuffer->Release();
	d3d11DeviceContext->OMSetRenderTargets(1, &d3d11RenderTargetView, NULL);
	D3D11_VIEWPORT viewport;
	ZeroMemory(&viewport, sizeof(D3D11_VIEWPORT));

	auto shader = 
		"struct VOut"
		"{"
		"	float4 position : SV_POSITION;"
		"	float4 color : COLOR;"
		"};"
		"VOut VShader(float4 position : POSITION, float4 color : COLOR)"
		"{"
		"	VOut output;"
		"	output.position = position;"
		"	output.color = color;"
		"	return output;"
		"}"
		"float4 PShader(float4 position : SV_POSITION, float4 color : COLOR) : SV_TARGET"
		"{"
		"	return color;"
		"}";
	viewport.TopLeftX = 0;
	viewport.TopLeftY = 0;
	viewport.Width = 800;
	viewport.Height = 600;

	d3d11DeviceContext->RSSetViewports(1, &viewport);
}

void DX11Chart::Render()
{
	float color[4]{ 80.0f / 255,0.0f,0.0f,1.0f };
	d3d11DeviceContext->ClearRenderTargetView(d3d11RenderTargetView, color);

	D3D11_BUFFER_DESC bd = {};
	bd.Usage = D3D11_USAGE_DEFAULT;
	bd.ByteWidth = sizeof(Vertex) * 3; // 一个三角形有3个顶点
	bd.BindFlags = D3D11_BIND_VERTEX_BUFFER;
	bd.CPUAccessFlags = 0;
	D3D11_SUBRESOURCE_DATA InitData = {};
	//InitData.pSysMem = vertices; // vertices是预先定义的包含三角形数据的数组
	ID3D11Buffer* vertexBuffer;
	d3d11Device->CreateBuffer(&bd, &InitData, &vertexBuffer);



	d3d11DeviceContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
	d3d11DeviceContext->Draw(3, 0); // 绘制3个顶点，开始于索引0
	//d3d11DeviceContext->Begin()
	d3d11SwapChain->Present(0, 0);
}
