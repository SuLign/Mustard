using OpenTK;
using OpenTK.Graphics.OpenGL;

using SharpFont;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace Mustard.UIExtension.PlotControl.GLBase;

internal class TextRender
{
    private Dictionary<uint, Character> characters;
    private int vao;
    private int vbo;
    private Shader textRenderShader;
    private float ph;

    private static Library lib;
    private static Face face;
    private List<float> verticsLst;

    private List<int> chList;
    private Dictionary<int, List<float>> chMap;
    private int chCount;
    private int mti;

    public int CharactorSpanPixel
    {
        get; set;
    } = 2;

    static TextRender()
    {
        lib = new Library();
        //using Stream rsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mustard.UIExtension.PlotControl.GLBase.STSONG.TTF");
        using Stream rsStream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Mustard.UIExtension.PlotControl.GLBase.font.ttf");
        using MemoryStream ms = new MemoryStream();
        rsStream.CopyTo(ms);
        face = new Face(lib, ms.ToArray(), 0);
    }

    public TextRender()
    {
        textRenderShader = new Shader();
        textRenderShader.SetShaderSource(
            "#version 440 core\n" +
            "layout (location = 0) in vec4 vertex;\n" +
            "layout (location = 1) in vec4 color;\n" +
            "uniform mat4 projection;\n" +
            "out vec2 textCoords;\n" +
            "out vec4 textColor;\n" +
            "void main()\n" +
            "{\n" +
            "    gl_Position = vec4(vertex.xy, 0.0, 1.0) * projection;\n" +
            "    textCoords = vertex.zw;\n" +
            "    textColor = color;\n" +
            "}\n",

            "#version 440 core\n" +
            "in vec2 textCoords;\n" +
            "in vec4 textColor;\n" +
            "out vec4 fragColor;\n" +
            "uniform sampler2D u_texture;\n" +
            "void main()\n" +
            "{\n" +
            "    vec4 text = vec4(1.0, 1.0, 1.0, texture(u_texture, textCoords).r);\n" +
            "    fragColor = textColor * text;\n" +
            "}\n");
        characters = new Dictionary<uint, Character>();
        vao = GL.GenVertexArray();
        vbo = GL.GenBuffer();
        GL.BindVertexArray(vao);
        GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
        GL.EnableVertexAttribArray(0);
        GL.VertexAttribPointer(0, 4, VertexAttribPointerType.Float, false, 32, 0);
        GL.EnableVertexAttribArray(1);
        GL.VertexAttribPointer(1, 4, VertexAttribPointerType.Float, false, 32, 16);
        GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
        GL.BindVertexArray(0);
        GL.Enable(EnableCap.Blend);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusDstAlpha);
        verticsLst = new List<float>();
        chList = new List<int>();
        chMap = new Dictionary<int, List<float>>();
    }

    private bool LoadCharactor(char c, uint pixelHeight)
    {
        face.SetCharSize((int)pixelHeight, (int)pixelHeight, 96, 96);
        GL.PixelStore(PixelStoreParameter.UnpackAlignment, 1);
        try
        {
            face.LoadChar(c, LoadFlags.Render, LoadTarget.Normal);
            GlyphSlot glyph = face.Glyph;
            FTBitmap bitmap = glyph.Bitmap;

            // create glyph texture
            int texObj = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, texObj);
            GL.TexImage2D(
                TextureTarget.Texture2D,
                0,
                //(PixelInternalFormat)0x1903,
                PixelInternalFormat.R8,
                bitmap.Width,
                bitmap.Rows,
                0,
                //OpenTK.Graphics.OpenGL.PixelFormat.Alpha,
                OpenTK.Graphics.OpenGL.PixelFormat.Red,
                PixelType.UnsignedByte,
                bitmap.Buffer);

            // set texture parameters
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

            // add character
            Character ch = new Character
            {
                TextureID = texObj,
                Size = new Vector2(bitmap.Width, bitmap.Rows),
                Bearing = new Vector2(glyph.BitmapLeft, glyph.BitmapTop),
                Advance = (int)glyph.Advance.X.Value,
            };
            characters.Add(c, ch);
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }
        finally
        {
            GL.BindTexture(TextureTarget.Texture2D, 0);
        }
        return false;
    }

    public void ClearBuffer()
    {
        chCount = 0;
        chList.Clear();
        chMap.Clear();
    }

    public double MeasureTextWidth(string textToRender, float pixelHeight)
    {
        if (string.IsNullOrEmpty(textToRender)) return 0;
        if (ph != pixelHeight)
        {
            characters.Clear();
            ph = pixelHeight;
        }
        foreach (var c in textToRender)
        {
            if (!characters.ContainsKey(c))
            {
                LoadCharactor(c, (uint)pixelHeight);
                continue;
            }
        }
        return textToRender.Sum(e => (characters[e].Advance >> 6) + CharactorSpanPixel);
    }

    public Size AddCharactor(string textToRender, float pixelHeight, float x, float y, Color color, AlignmentX alignmentX, AlignmentY alignmentY, bool isVertical = false)
    {
        if (string.IsNullOrEmpty(textToRender)) return new Size();
        float char_x = 0.0f;

        if (ph != pixelHeight)
        {
            characters.Clear();
            ph = pixelHeight;
        }
        foreach (var c in textToRender)
        {
            if (!characters.ContainsKey(c))
            {
                LoadCharactor(c, (uint)pixelHeight);
                continue;
            }
        }
        var stringWidth =
            textToRender.Sum(e => (characters[e].Advance >> 6) + CharactorSpanPixel);

        foreach (var c in textToRender)
        {
            var ch = characters[c];
            chCount++;
            if (!chList.Contains(ch.TextureID)) chList.Add(ch.TextureID);
            float xrel = x + char_x + ch.Bearing.X;
            float yrel = -y - (ch.Size.Y - ch.Bearing.Y);
            if (alignmentX == AlignmentX.Center)
            {
                xrel -= (float)stringWidth / 2;
            }
            else if (alignmentX == AlignmentX.Right)
            {
                xrel -= stringWidth;
            }
            if (alignmentY == AlignmentY.Center)
            {
                yrel -= pixelHeight / 2;
            }
            else if (alignmentY == AlignmentY.Top)
            {
                yrel += pixelHeight / 2;
            }
            else
            {
                yrel -= pixelHeight;
            }
            float w = ch.Size.X;
            float h = ch.Size.Y;
            float[] vertics = new float[48];
            if (isVertical)
            {
                vertics = new float[48]
                {
                    (int) xrel     ,     (int)(yrel + h),       0,  0,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int) xrel     ,     (int) yrel     ,       0,  1,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int)(xrel + w),     (int) yrel     ,       1,  1,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int) xrel     ,     (int)(yrel + h),       0,  0,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int)(xrel + w),     (int) yrel     ,       1,  1,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int)(xrel + w),     (int)(yrel + h),       1,  0,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                };
            }
            else
            {
                vertics = new float[48]
                {
                    (int)(xrel + w),     (int) yrel     ,       1,  1,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int) xrel     ,     (int)(yrel + h),       0,  0,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int) xrel     ,     (int) yrel     ,       0,  1,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int) xrel     ,     (int)(yrel + h),       0,  0,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int)(xrel + w),     (int) yrel     ,       1,  1,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                    (int)(xrel + w),     (int)(yrel + h),       1,  0,    color.R / 255.0f, color.G / 255.0f, color.B / 255.0f, color.A / 255.0f,
                };
            }
            if (!chMap.ContainsKey(ch.TextureID)) chMap.Add(ch.TextureID, new List<float>());
            chMap[ch.TextureID].AddRange(vertics);
            char_x += (ch.Advance >> 6) + CharactorSpanPixel;
        }
        return new Size(stringWidth, pixelHeight);
    }

    public void DoDisplayRender(float screenWidth, float screenHeight)
    {
        var projection = Matrix4.CreateOrthographicOffCenter(0, (int)screenWidth, 0, (int)screenHeight, -1, 1);
        projection.Row3.Z = -1;
        projection.Row3.W = 1;

        GL.Enable(EnableCap.Blend);
        GL.Disable(EnableCap.LineSmooth);
        GL.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);
        GL.BindVertexArray(vao);
        textRenderShader.Use();
        textRenderShader.SetMatrix4("projection", projection);

        foreach (var chID in chList)
        {
            GL.BindTexture(TextureTarget.Texture2D, chID);
            GL.BindBuffer(BufferTarget.ArrayBuffer, vbo);
            GL.BufferData(BufferTarget.ArrayBuffer, chMap[chID].Count * 4, chMap[chID].ToArray(), BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            GL.DrawArrays(PrimitiveType.Triangles, 0, chMap[chID].Count / 7);
        }
        GL.Disable(EnableCap.LineSmooth);
        GL.BindVertexArray(0);
        GL.BindTexture(TextureTarget.Texture2D, mti);
    }

    public void SetMainTexture(int mainTextureID)
    {
        mti = mainTextureID;
    }
}
