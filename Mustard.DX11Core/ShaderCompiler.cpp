#include "DX11PCH.h"
#include "ShaderCompiler.h"


void DX11Core::DX11ShaderCompiler::SetDevice(ID3D11Device1* hDevice)
{
	d3d11Device = hDevice;
}

//https://olster1.github.io/direct3d_11_part3.html
int DX11Core::DX11ShaderCompiler::Compile(const char* hlslCode)
{
	ID3DBlob* err;
	HRESULT hResult = D3DCompile(
		hlslCode,
		strlen(hlslCode),
		nullptr,
		nullptr,
		nullptr,
		"vs_main",
		"vs_5_0",
		0,
		0,
		&vsBlob,
		&err);
	if (FAILED(hResult)) {
		return -1;
	}
	hResult = D3DCompile(
		hlslCode,
		strlen(hlslCode),
		nullptr,
		nullptr,
		nullptr,
		"ps_main",
		"ps_5_0",
		0,
		0,
		&psBlob,
		&err);
	if (SUCCEEDED(hResult)) {
		hResult = d3d11Device->CreateVertexShader(
			vsBlob->GetBufferPointer(),
			vsBlob->GetBufferSize(),
			nullptr,
			&vertexShader);
		if (FAILED(hResult)) {
			return -1;
		}
		hResult = d3d11Device->CreatePixelShader(
			psBlob->GetBufferPointer(),
			psBlob->GetBufferSize(),
			nullptr,
			&pixelShader);
		psBlob->Release();
	}
	//https://stackoverflow.com/questions/50155656/creating-dx11-shaders-from-string
	if (FAILED(hResult)) {
		return -1;
	}
	return 0;
}

void DX11Core::DX11ShaderCompiler::CompileFromFile(const char* hlslFilePath)
{
	HRESULT hResult = D3DCompileFromFile(
		L"shader.hlsl",
		nullptr,
		nullptr,
		"vs_main",
		"vs_5_0",
		0,
		0,
		&vsBlob,
		nullptr);
	hResult = d3d11Device->CreateVertexShader(
		vsBlob->GetBufferPointer(),
		vsBlob->GetBufferSize(),
		nullptr,
		&vertexShader);

	hResult = D3DCompileFromFile(
		L"shader.hlsl",
		nullptr,
		nullptr,
		"ps_main",
		"ps_5_0",
		0,
		0,
		&psBlob,
		nullptr);
	hResult = d3d11Device->CreatePixelShader(
		psBlob->GetBufferPointer(),
		psBlob->GetBufferSize(),
		nullptr,
		&pixelShader);
	psBlob->Release();
}

void DX11Core::DX11ShaderCompiler::SetInputElementsDesc(D3D11_INPUT_ELEMENT_DESC* inputElementDesc, int count)
{
	HRESULT hResult = d3d11Device->CreateInputLayout(
		inputElementDesc,
		count,
		vsBlob->GetBufferPointer(),
		vsBlob->GetBufferSize(),
		&inputLayout);
	vsBlob->Release();
}
