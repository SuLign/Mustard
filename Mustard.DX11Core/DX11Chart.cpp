#include "DX11PCH.h"
#include "DX11Chart.h"

void DX11Chart::SetBindHwnd(HWND hContainer)
{
	this->hContainer = hContainer;
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
	auto shader =
		"struct VS_Input"
		"{"
		"	float2 pos : POS;"
		"	float4 color : COL;"
		"};"
		""
		"struct VS_Output"
		"{"
		"	float4 position : SV_POSITION;"
		"	float4 color : COL;"
		"};"
		""
		"VS_Output vs_main(VS_Input input)"
		"{"
		"	VS_Output output;"
		"	output.position = float4(input.pos, 0.0f, 1.0f);"
		"	output.color = input.color;"
		"	return output;"
		"}"
		""
		"float4 ps_main(VS_Output input) : SV_TARGET"
		"{"
		"	return input.color;"
		"}";

	dx11ShaderCompiler = new DX11ShaderCompiler;
	dx11ShaderCompiler->SetDevice(d3d11Device);
	auto res = dx11ShaderCompiler->Compile(shader);

	D3D11_INPUT_ELEMENT_DESC inputElementDesc[] =
	{
		{ "POS", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		{ "COL", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT, D3D11_INPUT_PER_VERTEX_DATA, 0 }
	};
	dx11ShaderCompiler->SetInputElementsDesc(inputElementDesc, 2);


	float vertexData[] = { // x, y, u, v
		-0.5f, 0.5f, 0.f, 0.f,
		0.5f, -0.5f, 1.f, 1.f,
		-0.5f, -0.5f, 0.f, 1.f,
		-0.5f, 0.5f, 0.f, 0.f,
		0.5f, 0.5f, 1.f, 0.f,
		0.5f, -0.5f, 1.f, 1.f
	};
	stride = 4 * sizeof(float);
	numVerts = sizeof(vertexData) / stride;
	offset = 0;
	D3D11_BUFFER_DESC vertexBufferDesc = {};
	vertexBufferDesc.ByteWidth = sizeof(vertexData);
	vertexBufferDesc.Usage = D3D11_USAGE_IMMUTABLE;
	vertexBufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;

	D3D11_SUBRESOURCE_DATA vertexSubresourceData = { vertexData };

	HRESULT hResult = d3d11Device->CreateBuffer(&vertexBufferDesc, &vertexSubresourceData, &vertexBuffer);
}

void DX11Chart::Render()
{
	float color[4]{ 80.0f / 255,0.0f,0.0f,1.0f };
	d3d11DeviceContext->ClearRenderTargetView(d3d11RenderTargetView, color);

	RECT winRect;
	GetClientRect(hContainer, &winRect);
	D3D11_VIEWPORT viewport = { 0.0f, 0.0f, (FLOAT)(winRect.right - winRect.left), (FLOAT)(winRect.bottom - winRect.top), 0.0f, 1.0f };
	d3d11DeviceContext->RSSetViewports(1, &viewport);
	d3d11DeviceContext->OMSetRenderTargets(1, &d3d11RenderTargetView, NULL);

	d3d11DeviceContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);
	d3d11DeviceContext->IASetInputLayout(dx11ShaderCompiler->inputLayout);

	d3d11DeviceContext->VSSetShader(dx11ShaderCompiler->vertexShader, nullptr, 0);
	d3d11DeviceContext->PSSetShader(dx11ShaderCompiler->pixelShader, nullptr, 0);


	d3d11DeviceContext->IASetVertexBuffers(0, 1, &vertexBuffer, &stride, &offset);

	d3d11DeviceContext->Draw(numVerts, 0);
	d3d11SwapChain->Present(1, 0);
}
