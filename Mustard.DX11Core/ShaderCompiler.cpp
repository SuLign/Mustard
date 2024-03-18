#include "DX11PCH.h"
#include "ShaderCompiler.h"


void DX11Core::DX11ShaderCompiler::SetDevice(ID3D11Device* hDevice)
{
	d3d11Device = hDevice;
}

//https://olster1.github.io/direct3d_11_part3.html
int DX11Core::DX11ShaderCompiler::Compile(const char* hlslCode)
{
	HRESULT hResult = D3DCompile(
		hlslCode,
		strlen(hlslCode),
		nullptr,
		nullptr,
		nullptr,
		"vs_main",
		"vs_5_0",
		D3DCOMPILE_ENABLE_STRICTNESS,
		0,
		&vsBlob,
		nullptr);
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
		D3DCOMPILE_ENABLE_STRICTNESS,
		0,
		&psBlob,
		nullptr);
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
	}
	//https://stackoverflow.com/questions/50155656/creating-dx11-shaders-from-string
	if (FAILED(hResult)) {
		return -1;
	}
	return 0;
}

void DX11Core::DX11ShaderCompiler::SetInputElementsDesc(D3D11_INPUT_ELEMENT_DESC* inputElementDesc, int count)
{
	HRESULT hResult = d3d11Device->CreateInputLayout(inputElementDesc, count, vsBlob->GetBufferPointer(), vsBlob->GetBufferSize(), &inputLayout);
}
