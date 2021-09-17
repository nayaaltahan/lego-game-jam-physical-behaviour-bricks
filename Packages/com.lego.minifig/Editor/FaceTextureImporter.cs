// Copyright (C) LEGO System A/S - All Rights Reserved
// Unauthorized copying of this file, via any medium is strictly prohibited

using UnityEditor;
using UnityEngine;

namespace LEGOMinifig
{
    public class FaceTextureImporter : AssetPostprocessor
    {
        public override uint GetVersion()
        {
            return base.GetVersion() + 156;
        }

        void OnPreprocessTexture()
        {
            TextureImporter t = (TextureImporter)assetImporter;

            if (assetPath.Contains(MinifigUtility.faceTexturePath))
            {
                t.mipmapEnabled = false;
                t.alphaIsTransparency = true;
                t.wrapMode = TextureWrapMode.Clamp;
            }
        }
    }
}
