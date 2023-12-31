﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Effects;
using System.Windows;
using System.Windows.Media;

namespace Mustard.UI.Effects
{
    public class InvertPngEffect : ShaderEffect
    {
        private const string _kshaderAsBase64 =
                @"AAP///7/HwBDVEFCHAAAAE8AAAAAA///AQAAABwAAAAAAQAASAAAADAAAAADAAAAAQACADgAAAAA
            AAAAaW5wdXQAq6sEAAwAAQABAAEAAAAAAAAAcHNfM18wAE1pY3Jvc29mdCAoUikgSExTTCBTaGFk
            ZXIgQ29tcGlsZXIgMTAuMQCrUQAABQAAD6AAAIA/AAAAAAAAAAAAAAAAHwAAAgUAAIAAAAOQHwAA
            AgAAAJAACA+gQgAAAwAAD4AAAOSQAAjkoAIAAAMAAAeAAADkgQAAAKAFAAADAAgHgAAA/4AAAOSA
            AQAAAgAICIAAAP+A//8AAA==";
        private static readonly PixelShader _shader;

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }
        public static readonly DependencyProperty InputProperty = ShaderEffect.RegisterPixelShaderSamplerProperty("Input", typeof(InvertPngEffect), 0);

        static InvertPngEffect()
        {
            _shader = new PixelShader();
            _shader.SetStreamSource(new MemoryStream(Convert.FromBase64String(_kshaderAsBase64)));
        }

        public InvertPngEffect()
        {
            PixelShader = _shader;
            UpdateShaderValue(InputProperty);
        }
    }
}
