using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AssetStudio
{
    public sealed class Texture2D : Texture
    {
        public int m_Width;
        public int m_Height;
        public uint m_CompleteImageSize;
        public TextureFormat m_TextureFormat;
        public bool m_MipMap;
        public int m_MipCount;
        public GLTextureSettings m_TextureSettings;
        public int m_ImageCount;
        public byte[] m_PlatformBlob;
        [JsonPropertyName("image data")]
        public ResourceReader image_data;
        public StreamingInfo m_StreamData;
        public StreamingInfo m_DataStreamData;

        public Texture2D() { }

        public Texture2D(Texture2DArray m_Texture2DArray, int layer)
        {
            reader = m_Texture2DArray.reader;
            assetsFile = m_Texture2DArray.assetsFile;
            version = m_Texture2DArray.version;
            platform = m_Texture2DArray.platform;

            m_Name = $"{m_Texture2DArray.m_Name}_{layer + 1}";
            type = ClassIDType.Texture2DArrayImage;
            m_PathID = m_Texture2DArray.m_PathID;

            m_Width = m_Texture2DArray.m_Width;
            m_Height = m_Texture2DArray.m_Height;
            m_TextureFormat = m_Texture2DArray.m_Format.ToTextureFormat();
            m_MipCount = m_Texture2DArray.m_MipCount;
            m_TextureSettings = m_Texture2DArray.m_TextureSettings;
            m_StreamData = m_Texture2DArray.m_StreamData;
            m_PlatformBlob = Array.Empty<byte>();
            m_MipMap = m_MipCount > 1;
            m_ImageCount = 1;

            m_CompleteImageSize = (uint)(m_Texture2DArray.m_DataSize / m_Texture2DArray.m_Depth);
            var offset = layer * m_CompleteImageSize + m_Texture2DArray.image_data.Offset;
            
            image_data = !string.IsNullOrEmpty(m_StreamData?.path) 
                ? new ResourceReader(m_StreamData.path, assetsFile, offset, m_CompleteImageSize) 
                : new ResourceReader(reader, offset, m_CompleteImageSize);

            byteSize = (uint)(m_Width * m_Height) * 4;
        }

        public Texture2D(ObjectReader reader, byte[] type, JsonSerializerOptions jsonOptions) : base(reader)
        {
            var parsedTex2d = JsonSerializer.Deserialize<Texture2D>(type, jsonOptions);
            m_Width = parsedTex2d.m_Width;
            m_Height = parsedTex2d.m_Height;
            m_CompleteImageSize = parsedTex2d.m_CompleteImageSize;
            m_TextureFormat = parsedTex2d.m_TextureFormat;
            m_MipMap = parsedTex2d.m_MipMap;
            m_MipCount = parsedTex2d.m_MipCount;
            m_ImageCount = parsedTex2d.m_ImageCount;
            m_TextureSettings = parsedTex2d.m_TextureSettings;
            m_StreamData = parsedTex2d.m_StreamData;
            m_PlatformBlob = parsedTex2d.m_PlatformBlob ?? Array.Empty<byte>();

            image_data = !string.IsNullOrEmpty(m_StreamData?.path)
                ? new ResourceReader(m_StreamData.path, assetsFile, m_StreamData.offset, m_StreamData.size)
                : new ResourceReader(reader, parsedTex2d.image_data.Offset, parsedTex2d.image_data.Size);
        }

        public Texture2D(ObjectReader reader) : base(reader)
        {
            m_Width = reader.ReadInt32();
            m_Height = reader.ReadInt32();
            m_CompleteImageSize = reader.ReadUInt32();
            if (version >= 2020)
            {
                var m_MipsStripped = reader.ReadInt32();
            }
            if (version.IsTuanjie && (version > (2022, 3, 2) || version.Build >= 8))
            {
                var m_WebStreaming = reader.ReadBoolean();
                reader.AlignStream();
                var m_PriorityLevel = reader.ReadInt32();
                var m_UploadedMode = reader.ReadInt32();
                m_DataStreamData = new StreamingInfo
                {
                    offset = 0,
                    size = reader.ReadUInt32(),
                    path = reader.ReadAlignedString()
                };
            }
            m_TextureFormat = (TextureFormat)reader.ReadInt32();
            if (version.IsTuanjie && version >= (2022, 3, 62))
            {
                var m_TextureManagerMultiFormatSettingSize = reader.ReadInt32();
                reader.Position += m_TextureManagerMultiFormatSettingSize;
                reader.AlignStream();
            }
            if (version < (5, 2))
            {
                m_MipMap = reader.ReadBoolean();
            }
            else
            {
                m_MipCount = reader.ReadInt32();
            }

            if ((int)m_TextureFormat == 0 && m_MipCount > 10)
            {
                m_TextureFormat = (TextureFormat)m_MipCount;
                m_MipCount = 1;
            }

            if (version >= (2, 6))
            {
                var m_IsReadable = reader.ReadBoolean();
            }
            if (version >= 2020)
            {
                var m_IsPreProcessed = reader.ReadBoolean();
            }
            if (version >= (2019, 3))
            {
                if (version >= (2022, 2))
                {
                    var m_IgnoreMipmapLimit = reader.ReadBoolean();
                    reader.AlignStream();
                }
                else
                {
                    var m_IgnoreMasterTextureLimit = reader.ReadBoolean();
                }
            }
            if (version.IsInRange(3, (5, 5)))
            {
                var m_ReadAllowed = reader.ReadBoolean();
            }
            if (version >= (2022, 2))
            {
                var m_MipmapLimitGroupName = reader.ReadAlignedString();
            }
            if (version >= (2018, 2))
            {
                var m_StreamingMipmaps = reader.ReadBoolean();
            }
            reader.AlignStream();
            if (version >= (2018, 2))
            {
                var m_StreamingMipmapsPriority = reader.ReadInt32();
            }
            m_ImageCount = reader.ReadInt32();
            var m_TextureDimension = reader.ReadInt32();
            m_TextureSettings = new GLTextureSettings(reader);
            if (version >= 3)
            {
                var m_LightmapFormat = reader.ReadInt32();
            }
            if (version >= (3, 5))
            {
                var m_ColorSpace = reader.ReadInt32();
            }
            if (version >= (2020, 2))
            {
                m_PlatformBlob = reader.ReadUInt8Array();
                reader.AlignStream();
            }
            else
            {
                m_PlatformBlob = Array.Empty<byte>();
            }
            
            var image_data_size = reader.ReadInt32();
            if (image_data_size == 0 && version >= (5, 3))
            {
                m_StreamData = new StreamingInfo(reader);
            }

            if (m_StreamData != null && !string.IsNullOrEmpty(m_StreamData.path))
            {
                if (m_StreamData.size > m_CompleteImageSize && m_CompleteImageSize > 0)
                {
                    uint fakeHeaderSize = (uint)(m_StreamData.size - m_CompleteImageSize);
                    m_StreamData.offset += fakeHeaderSize;
                    m_StreamData.size = m_CompleteImageSize;
                }
            }
            else
            {
                if (image_data_size > m_CompleteImageSize && m_CompleteImageSize > 0)
                {
                    int fakeHeaderSize = image_data_size - (int)m_CompleteImageSize;
                    reader.BaseStream.Position += fakeHeaderSize;
                    image_data_size = (int)m_CompleteImageSize;
                }
            }

            image_data = !string.IsNullOrEmpty(m_StreamData?.path)
                ? new ResourceReader(m_StreamData.path, assetsFile, m_StreamData.offset, m_StreamData.size)
                : new ResourceReader(reader, reader.BaseStream.Position, image_data_size);
        }

        private int GetImageDataSize(TextureFormat textureFormat)
        {
            var imgDataSize = m_Width * m_Height;
            switch (textureFormat)
            {
                case TextureFormat.ASTC_RGBA_5x5:
                    imgDataSize = (int)(MathF.Floor((m_Width + 4) / 5f) * MathF.Floor((m_Height + 4) / 5f) * 16);
                    break;
                case TextureFormat.ASTC_RGBA_6x6:
                    imgDataSize = (int)(MathF.Floor((m_Width + 5) / 6f) * MathF.Floor((m_Height + 5) / 6f) * 16);
                    break;
                case TextureFormat.ASTC_RGBA_8x8:
                    imgDataSize = (int)(MathF.Floor((m_Width + 7) / 8f) * MathF.Floor((m_Height + 7) / 8f) * 16);
                    break;
                case TextureFormat.ASTC_RGBA_10x10:
                    imgDataSize = (int)(MathF.Floor((m_Width + 9) / 10f) * MathF.Floor((m_Height + 9) / 10f) * 16);
                    break;
                case TextureFormat.ASTC_RGBA_12x12:
                    imgDataSize = (int)(MathF.Floor((m_Width + 11) / 12f) * MathF.Floor((m_Height + 11) / 12f) * 16);
                    break;
                case TextureFormat.DXT1:
                case TextureFormat.EAC_R:
                case TextureFormat.EAC_R_SIGNED:
                case TextureFormat.ATC_RGB4:
                case TextureFormat.ETC_RGB4:
                case TextureFormat.ETC2_RGB:
                case TextureFormat.ETC2_RGBA1:
                case TextureFormat.PVRTC_RGBA4:
                    imgDataSize /= 2;
                    break;
                case TextureFormat.PVRTC_RGBA2:
                    imgDataSize /= 4;
                    break;
                case TextureFormat.R16:
                case TextureFormat.RGB565:
                    imgDataSize *= 2;
                    break;
                case TextureFormat.RGB24:
                    imgDataSize *= 3;
                    break;
                case TextureFormat.RG32:
                case TextureFormat.RGBA32:
                case TextureFormat.ARGB32:
                case TextureFormat.BGRA32:
                case TextureFormat.RGB9e5Float:
                    imgDataSize *= 4;
                    break;
                case TextureFormat.RGB48:
                    imgDataSize *= 6;
                    break;
                case TextureFormat.RGBAHalf:
                case TextureFormat.RGBA64:
                    imgDataSize *= 8;
                    break;
            }
            return imgDataSize;
        }
    }
}
