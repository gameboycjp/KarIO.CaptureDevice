﻿using System;
using System.Linq;
using System.IO;
using Elements.Assets;
using Elements.Core;
using FrooxEngine;
using FlashCap;
using BitMiracle.LibJpeg;

namespace KarIO.CaptureDevice;

[Category("KarIO/Textures")]



public class CaptureDeviceTexture : ProceduralTextureBase
{
    static string ByteArrayToString(byte[] ba)
    {
        return BitConverter.ToString(ba).Replace("-", "");
    }


    public readonly Sync<int2> Size;
    [DefaultValue(true)]
    public readonly Sync<bool> Mipmaps;
    [DefaultValue(Elements.Assets.TextureFormat.RGBA32)]
    public readonly Sync<TextureFormat> TextureFormat;

    public readonly Sync<int> CaptureDeviceIndex;
    private int cachedCaptureDeviceIndex;
    public readonly RawOutput<string> CaptureDeviceName;
    private string LocalCaptureDeviceName;

    public readonly Sync<int> CaptureDeviceModeIndex;
    private int cachedCaptureDeviceModeIndex;
    public readonly RawOutput<string> CaptureDeviceModeName;
    private string LocalCaptureDeviceModeName;

    private bool LocalCaptureDeviceActive;
    public readonly RawOutput<bool> CaptureDeviceActiveOutput;

    private FlashCap.CaptureDevice FlashCapDevice;
    private FlashCap.CaptureDeviceDescriptor FlashCapDeviceDescriptor;
    private byte[] CaptureData;

    private Bitmap2D CaptureImage;

    protected override void GenerateErrorIndication() => TextureDecoder.FillErrorTexture(tex2D);

    protected override void OnAwake()
    {
        Size.Value = new int2(1920, 1080);
        UniLog.Log("awake");
        UpdateCaptureDevice();
    }

    protected override void ClearTextureData()
    {
        UniLog.Log("CLEAR?");
    }

    protected override void OnCommonUpdate()
    {
        if ((cachedCaptureDeviceIndex != CaptureDeviceIndex.Value) || (cachedCaptureDeviceModeIndex != CaptureDeviceModeIndex.Value))
        {
            UpdateCaptureDevice();
            cachedCaptureDeviceIndex = CaptureDeviceIndex.Value;
            cachedCaptureDeviceModeIndex = CaptureDeviceModeIndex.Value;
        }
        if (LocalCaptureDeviceActive != CaptureDeviceActiveOutput.Value) { CaptureDeviceActiveOutput.Value = LocalCaptureDeviceActive; }
        if (LocalCaptureDeviceName != CaptureDeviceName.Value) { CaptureDeviceName.Value = LocalCaptureDeviceName; }
        if (LocalCaptureDeviceModeName != CaptureDeviceModeName.Value) { CaptureDeviceModeName.Value =  LocalCaptureDeviceModeName; }
    }

    protected override void UpdateTextureData(Bitmap2D tex2D)
    {
        try
        {
            if (!LocalCaptureDeviceActive) { goto err; }

            CaptureData.CopyTo(tex2D.RawData, 0);
            //tex2D.CopyFrom(CaptureImage, 0, 0, 0, 0, Size.Value.X, Size.Value.Y);
            return;
        }
        catch (Exception exception)
        {
            UniLog.Error(exception.ToString());
            goto err;
        }

    err:
        GenerateErrorIndication();
        return;
    }

    protected override void OnDestroying()
    {
        KillAsyncTask();
        base.OnDestroying();
    }

    protected async void UpdateCaptureDevice()
    {
        UniLog.Log("Initializing capture device");
        KillAsyncTask();
        var devices = new CaptureDevices();
        try
        {
            FlashCapDeviceDescriptor = devices.EnumerateDescriptors().ElementAt(CaptureDeviceIndex);
            FlashCapDevice = await FlashCapDeviceDescriptor.OpenAsync(
                FlashCapDeviceDescriptor.Characteristics[CaptureDeviceModeIndex],
                async bufferScope =>
                {
                    try
                    {
                        byte[] image = bufferScope.Buffer.ExtractImage();

                        var JPEGScanStart = 0;
                        var JPEGScanStartFound = false;
                        var i = 0;
                        while (!JPEGScanStartFound)
                        {
                            if (image[i] == 0xFF && image[i + 1] == 0xDA) { JPEGScanStart = i; JPEGScanStartFound = true; }
                            i++;
                            if (i > image.Length) { JPEGScanStartFound = true; };
                        }

                        if (JPEGScanStart == 0 ) { throw new Exception("Scan start not detected MAYDAY"); }

                        var partA = new byte[JPEGScanStart]; //JFIF Data
                        var partB = new byte[] { //Standard Huffman Tables, courtesy of libjpeg-turbo

                            0xFF, 0xC4, 0x00, 0x1F, 0x00, // Marker, length, and ID

                            0, 1, 5, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, //bits_dc_luminance

                            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, //val_dc_luminance


                            0xFF, 0xC4, 0x00, 0xB5, 0x10, // Marker, length, and ID

                            0, 2, 1, 3, 3, 2, 4, 3, 5, 5, 4, 4, 0, 0, 1, 0x7d, //bits_ac_luminance

                            0x01, 0x02, 0x03, 0x00, 0x04, 0x11, 0x05, 0x12, //val_ac_luminance
                            0x21, 0x31, 0x41, 0x06, 0x13, 0x51, 0x61, 0x07,
                            0x22, 0x71, 0x14, 0x32, 0x81, 0x91, 0xa1, 0x08,
                            0x23, 0x42, 0xb1, 0xc1, 0x15, 0x52, 0xd1, 0xf0,
                            0x24, 0x33, 0x62, 0x72, 0x82, 0x09, 0x0a, 0x16,
                            0x17, 0x18, 0x19, 0x1a, 0x25, 0x26, 0x27, 0x28,
                            0x29, 0x2a, 0x34, 0x35, 0x36, 0x37, 0x38, 0x39,
                            0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48, 0x49,
                            0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58, 0x59,
                            0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68, 0x69,
                            0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78, 0x79,
                            0x7a, 0x83, 0x84, 0x85, 0x86, 0x87, 0x88, 0x89,
                            0x8a, 0x92, 0x93, 0x94, 0x95, 0x96, 0x97, 0x98,
                            0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5, 0xa6, 0xa7,
                            0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4, 0xb5, 0xb6,
                            0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3, 0xc4, 0xc5,
                            0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2, 0xd3, 0xd4,
                            0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda, 0xe1, 0xe2,
                            0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9, 0xea,
                            0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
                            0xf9, 0xfa,


                            0xFF, 0xC4, 0x00, 0x1F, 0x01, // Marker, length, and ID

                            0, 3, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, //bits_dc_chrominance

                            0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, // val_dc_chrominance


                            0xFF, 0xC4, 0x00, 0xB5, 0x11, // Marker, length, and ID

                            0, 2, 1, 2, 4, 4, 3, 4, 7, 5, 4, 4, 0, 1, 2, 0x77, //bits_ac_chrominance

                            0x00, 0x01, 0x02, 0x03, 0x11, 0x04, 0x05, 0x21, //val_ac_chrominance
                            0x31, 0x06, 0x12, 0x41, 0x51, 0x07, 0x61, 0x71,
                            0x13, 0x22, 0x32, 0x81, 0x08, 0x14, 0x42, 0x91,
                            0xa1, 0xb1, 0xc1, 0x09, 0x23, 0x33, 0x52, 0xf0,
                            0x15, 0x62, 0x72, 0xd1, 0x0a, 0x16, 0x24, 0x34,
                            0xe1, 0x25, 0xf1, 0x17, 0x18, 0x19, 0x1a, 0x26,
                            0x27, 0x28, 0x29, 0x2a, 0x35, 0x36, 0x37, 0x38,
                            0x39, 0x3a, 0x43, 0x44, 0x45, 0x46, 0x47, 0x48,
                            0x49, 0x4a, 0x53, 0x54, 0x55, 0x56, 0x57, 0x58,
                            0x59, 0x5a, 0x63, 0x64, 0x65, 0x66, 0x67, 0x68,
                            0x69, 0x6a, 0x73, 0x74, 0x75, 0x76, 0x77, 0x78,
                            0x79, 0x7a, 0x82, 0x83, 0x84, 0x85, 0x86, 0x87,
                            0x88, 0x89, 0x8a, 0x92, 0x93, 0x94, 0x95, 0x96,
                            0x97, 0x98, 0x99, 0x9a, 0xa2, 0xa3, 0xa4, 0xa5,
                            0xa6, 0xa7, 0xa8, 0xa9, 0xaa, 0xb2, 0xb3, 0xb4,
                            0xb5, 0xb6, 0xb7, 0xb8, 0xb9, 0xba, 0xc2, 0xc3,
                            0xc4, 0xc5, 0xc6, 0xc7, 0xc8, 0xc9, 0xca, 0xd2,
                            0xd3, 0xd4, 0xd5, 0xd6, 0xd7, 0xd8, 0xd9, 0xda,
                            0xe2, 0xe3, 0xe4, 0xe5, 0xe6, 0xe7, 0xe8, 0xe9,
                            0xea, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6, 0xf7, 0xf8,
                            0xf9, 0xfa
                        };
                        var partC = new byte[image.Length - partA.Length]; //The actual scan

                        Buffer.BlockCopy(image, 0, partA, 0, partA.Length);
                        Buffer.BlockCopy(image, partA.Length, partC, 0, partC.Length);

                        var bytes = new byte[partA.Length + partB.Length + partC.Length];

                        partA.CopyTo(bytes, 0);
                        partB.CopyTo(bytes, partA.Length);
                        partC.CopyTo(bytes, partA.Length + partB.Length);

                        var ms = new MemoryStream(bytes);

                        CaptureImage = Bitmap2D.Load(ms, "jpg", true);

                        OnChanges();
                    }
                    catch (Exception exception)
                    {
                        UniLog.Error(exception.ToString());
                    }
                });
            await FlashCapDevice.StartAsync();
            LocalCaptureDeviceActive = true;
            LocalCaptureDeviceName = FlashCapDeviceDescriptor.Name;
            LocalCaptureDeviceModeName = FlashCapDeviceDescriptor.Characteristics[CaptureDeviceModeIndex].ToString();
        }
        catch (Exception exception)
        {
            UniLog.Error("Couldn't start Async task");
            UniLog.Error(exception.ToString());
        }
    }

    protected async void KillAsyncTask()
    {
        if (LocalCaptureDeviceActive)
        {
            try
            {
                await FlashCapDevice.StopAsync();
                UniLog.Log("Killed capture device (Allegedly! Seems to be broken.)");
            }
            catch (Exception exception)
            {
                UniLog.Error(exception.ToString());
            }
        }
    }

    protected override int2 GenerateSize => Size.Value;
    protected override bool GenerateMipmaps => Mipmaps.Value;
    protected override TextureFormat GenerateFormat => TextureFormat.Value;
}
