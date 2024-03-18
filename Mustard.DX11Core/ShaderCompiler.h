#pragma once
#include "DX11PCH.h"

namespace DX11Core {
	class DX11ShaderCompiler {
	private:
		ID3D11Device* d3d11Device;

		// Vertex.
		ID3DBlob* vsBlob;

		// Pixel.
		ID3DBlob* psBlob;

	public:
		ID3D11VertexShader* vertexShader;
		ID3D11PixelShader* pixelShader; 
		ID3D11InputLayout* inputLayout;

	public :
		void SetDevice(ID3D11Device* hDevice);

		int Compile(const char* hlslCode);

		void SetInputElementsDesc(D3D11_INPUT_ELEMENT_DESC* inputElementDesc, int count);
	};
}