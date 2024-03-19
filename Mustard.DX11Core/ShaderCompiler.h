#pragma once
#include "DX11PCH.h"

namespace DX11Core {
	class DX11ShaderCompiler {
	private:
		ID3D11Device1* d3d11Device = 0;

		// Vertex.
		ID3DBlob* vsBlob = 0;

		// Pixel.
		ID3DBlob* psBlob = 0;

	public:
		ID3D11VertexShader* vertexShader = 0;
		ID3D11PixelShader* pixelShader = 0;
		ID3D11InputLayout* inputLayout = 0;

	public:
		void SetDevice(ID3D11Device1* hDevice);

		int Compile(const char* hlslCode);

		void CompileFromFile(const char* hlslFilePath);

		void SetInputElementsDesc(D3D11_INPUT_ELEMENT_DESC* inputElementDesc, int count);
	};
}