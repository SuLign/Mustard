#include "DX11PCH.h"
#include "DX11Chart.h"

void DX11Chart::SetBindHwnd(HWND hContainer)
{
	this->hContainer = hContainer;

	ID3D11Device* baseDevice;
	ID3D11DeviceContext* baseDeviceContext;
	D3D_FEATURE_LEVEL featureLevels[] = { D3D_FEATURE_LEVEL_11_0 }; //we just want d3d 11 features, not below
	UINT creationFlags = D3D11_CREATE_DEVICE_BGRA_SUPPORT;

	HRESULT hResult = D3D11CreateDevice(
		0,
		D3D_DRIVER_TYPE_HARDWARE, //hardware rendering instead of software rendering
		0,
		creationFlags,
		featureLevels,
		ARRAYSIZE(featureLevels),  //feature levels: we want direct11 features - don't want any below
		D3D11_SDK_VERSION,
		&baseDevice,
		0,
		&baseDeviceContext);
	hResult = baseDevice->QueryInterface(__uuidof(ID3D11Device1), (void**)&d3d11Device);
	assert(SUCCEEDED(hResult));
	baseDevice->Release();

	hResult = baseDeviceContext->QueryInterface(__uuidof(ID3D11DeviceContext1), (void**)&d3d11DeviceContext);
	assert(SUCCEEDED(hResult));
	baseDeviceContext->Release();

	IDXGIFactory2* dxgiFactory;
	IDXGIDevice1* dxgiDevice;
	hResult = d3d11Device->QueryInterface(__uuidof(IDXGIDevice1), (void**)&dxgiDevice);
	assert(SUCCEEDED(hResult));

	IDXGIAdapter* dxgiAdapter;
	hResult = dxgiDevice->GetAdapter(&dxgiAdapter);
	assert(SUCCEEDED(hResult));
	dxgiDevice->Release();

	DXGI_ADAPTER_DESC adapterDesc;
	dxgiAdapter->GetDesc(&adapterDesc);

	OutputDebugStringA("Graphics Device: \n");
	OutputDebugStringW(adapterDesc.Description);

	hResult = dxgiAdapter->GetParent(__uuidof(IDXGIFactory2), (void**)&dxgiFactory);
	assert(SUCCEEDED(hResult));
	dxgiAdapter->Release();

	RECT winRect;
	GetClientRect(hContainer, &winRect);

	// Create Swap Chain
	DXGI_SWAP_CHAIN_DESC1 d3d11SwapChainDesc = {};
	d3d11SwapChainDesc.Width = winRect.right - winRect.left; // use window width
	d3d11SwapChainDesc.Height = winRect.bottom - winRect.top; // use window height
	d3d11SwapChainDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM_SRGB;
	d3d11SwapChainDesc.SampleDesc.Count = 1;
	d3d11SwapChainDesc.SampleDesc.Quality = 0;
	d3d11SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
	d3d11SwapChainDesc.BufferCount = 1;
	d3d11SwapChainDesc.Scaling = DXGI_SCALING_STRETCH;
	d3d11SwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
	d3d11SwapChainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;
	d3d11SwapChainDesc.Flags = 0;
	hResult = dxgiFactory->CreateSwapChainForHwnd(d3d11Device, hContainer, &d3d11SwapChainDesc, 0, 0, &d3d11SwapChain);
	assert(SUCCEEDED(hResult));
	dxgiFactory->Release();
}

void DX11Chart::InitD3D()
{
	dx11ShaderCompiler = new DX11ShaderCompiler;
	dx11ShaderCompiler->SetDevice(d3d11Device);
#if 0
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

	auto res = dx11ShaderCompiler->Compile(shader);

#else
	dx11ShaderCompiler->CompileFromFile(nullptr);
#endif
	D3D11_INPUT_ELEMENT_DESC inputElementDesc[] =
	{
		{ "POS", 0, DXGI_FORMAT_R32G32_FLOAT, 0, 0, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		{ "COL", 0, DXGI_FORMAT_R32G32B32A32_FLOAT, 0, D3D11_APPEND_ALIGNED_ELEMENT, D3D11_INPUT_PER_VERTEX_DATA, 0 }
	};
	dx11ShaderCompiler->SetInputElementsDesc(inputElementDesc, 2);
	float vertexData[] = { // x, y, r, g, b, a
		   0.0f,  0.5f, 0.f, 1.f, 0.f, 1.f,
		   0.5f, -0.5f, 1.f, 0.f, 0.f, 1.f,
		   -0.5f, -0.5f, 0.f, 0.f, 1.f, 1.f
	};
	stride = 6 * sizeof(float);
	numVerts = sizeof(vertexData) / stride;
	offset = 0;
	D3D11_BUFFER_DESC vertexBufferDesc = {};
	vertexBufferDesc.ByteWidth = sizeof(vertexData);
	vertexBufferDesc.Usage = D3D11_USAGE_IMMUTABLE;
	vertexBufferDesc.BindFlags = D3D11_BIND_VERTEX_BUFFER;

	D3D11_SUBRESOURCE_DATA vertexSubresourceData = { vertexData };

	auto hResult = d3d11Device->CreateBuffer(&vertexBufferDesc, &vertexSubresourceData, &vertexBuffer);



}

void DX11Chart::Render()
{
	RECT winRect;
	GetClientRect(hContainer, &winRect);
	auto w = winRect.right - winRect.left;
	auto h = winRect.bottom - winRect.top;
	if (w != width || h != height)
	{
		width = w;
		height = h;
		IDXGIFactory2* dxgiFactory;
		IDXGIDevice1* dxgiDevice;
		HRESULT hResult = d3d11Device->QueryInterface(__uuidof(IDXGIDevice1), (void**)&dxgiDevice);
		assert(SUCCEEDED(hResult));

		IDXGIAdapter* dxgiAdapter;
		hResult = dxgiDevice->GetAdapter(&dxgiAdapter);
		assert(SUCCEEDED(hResult));
		dxgiDevice->Release();

		DXGI_ADAPTER_DESC adapterDesc;
		dxgiAdapter->GetDesc(&adapterDesc);

		OutputDebugStringA("Graphics Device: \n");
		OutputDebugStringW(adapterDesc.Description);

		hResult = dxgiAdapter->GetParent(__uuidof(IDXGIFactory2), (void**)&dxgiFactory);
		assert(SUCCEEDED(hResult));
		dxgiAdapter->Release();

		// Create Swap Chain
		DXGI_SWAP_CHAIN_DESC1 d3d11SwapChainDesc = {};
		d3d11SwapChainDesc.Width = width; // use window width
		d3d11SwapChainDesc.Height = height; // use window height
		d3d11SwapChainDesc.Format = DXGI_FORMAT_B8G8R8A8_UNORM_SRGB;
		d3d11SwapChainDesc.SampleDesc.Count = 1;
		d3d11SwapChainDesc.SampleDesc.Quality = 0;
		d3d11SwapChainDesc.BufferUsage = DXGI_USAGE_RENDER_TARGET_OUTPUT;
		d3d11SwapChainDesc.BufferCount = 1;
		d3d11SwapChainDesc.Scaling = DXGI_SCALING_STRETCH;
		d3d11SwapChainDesc.SwapEffect = DXGI_SWAP_EFFECT_DISCARD;
		d3d11SwapChainDesc.AlphaMode = DXGI_ALPHA_MODE_UNSPECIFIED;
		d3d11SwapChainDesc.Flags = 0;
		d3d11SwapChain->Release();
		hResult = dxgiFactory->CreateSwapChainForHwnd(d3d11Device, hContainer, &d3d11SwapChainDesc, 0, 0, &d3d11SwapChain);
		assert(SUCCEEDED(hResult));
		dxgiFactory->Release();

		if (d3d11RenderTargetView != 0)d3d11RenderTargetView->Release();
		ID3D11Texture2D* d3d11FrameBuffer;
		hResult = d3d11SwapChain->GetBuffer(0, __uuidof(ID3D11Texture2D), (void**)&d3d11FrameBuffer);
		assert(SUCCEEDED(hResult));
		d3d11Device->CreateRenderTargetView(d3d11FrameBuffer, NULL, &d3d11RenderTargetView);
		d3d11FrameBuffer->Release();

	}
	float color[4]{ 80.0f / 255,0.0f,0.0f,1.0f };
	d3d11DeviceContext->ClearRenderTargetView(d3d11RenderTargetView, color);

	D3D11_VIEWPORT viewport = { 0.0f, 0.0f, (FLOAT)w, (FLOAT)h, 0.0f, 1.0f };
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
