﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using YooAsset;

namespace TEngine
{
    /// <summary>
    /// 远端资源地址查询服务类
    /// </summary>
    internal class RemoteServices : IRemoteServices
    {
        private readonly string _defaultHostServer;
        private readonly string _fallbackHostServer;

        public RemoteServices(string defaultHostServer, string fallbackHostServer)
        {
            _defaultHostServer = defaultHostServer;
            _fallbackHostServer = fallbackHostServer;
        }

        string IRemoteServices.GetRemoteMainURL(string fileName)
        {
            return $"{_defaultHostServer}/{fileName}";
        }

        string IRemoteServices.GetRemoteFallbackURL(string fileName)
        {
            return $"{_fallbackHostServer}/{fileName}";
        }
    }
    
    /// <summary>
    /// 文件流加密方式
    /// </summary>
    public class FileStreamEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            var fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
            for (int i = 0; i < fileData.Length; i++)
            {
                fileData[i] ^= BundleStream.KEY;
            }

            EncryptResult result = new EncryptResult();
            result.Encrypted = true;
            result.EncryptedData = fileData;
            return result;
        }
    }

    /// <summary>
    /// 资源文件流加载解密类
    /// </summary>
    public class FileStreamDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            BundleStream bundleStream =
                new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = bundleStream;
            decryptResult.Result =
                AssetBundle.LoadFromStream(bundleStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return decryptResult;
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            BundleStream bundleStream =
                new BundleStream(fileInfo.FileLoadPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = bundleStream;
            decryptResult.CreateRequest =
                AssetBundle.LoadFromStreamAsync(bundleStream, fileInfo.FileLoadCRC, GetManagedReadBufferSize());
            return decryptResult;
        }

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        private static uint GetManagedReadBufferSize()
        {
            return 1024;
        }
    }

    /// <summary>
    /// 文件偏移加密方式
    /// </summary>
    public class FileOffsetEncryption : IEncryptionServices
    {
        public EncryptResult Encrypt(EncryptFileInfo fileInfo)
        {
            int offset = 32;
            byte[] fileData = File.ReadAllBytes(fileInfo.FileLoadPath);
            var encryptedData = new byte[fileData.Length + offset];
            Buffer.BlockCopy(fileData, 0, encryptedData, offset, fileData.Length);

            EncryptResult result = new EncryptResult();
            result.Encrypted = true;
            result.EncryptedData = encryptedData;
            return result;
        }
    }

    /// <summary>
    /// 资源文件偏移加载解密类
    /// </summary>
    public class FileOffsetDecryption : IDecryptionServices
    {
        /// <summary>
        /// 同步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundle(DecryptFileInfo fileInfo)
        {
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = null;
            decryptResult.Result =
                AssetBundle.LoadFromFile(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, GetFileOffset());
            return decryptResult;
        }

        /// <summary>
        /// 异步方式获取解密的资源包对象
        /// 注意：加载流对象在资源包对象释放的时候会自动释放
        /// </summary>
        DecryptResult IDecryptionServices.LoadAssetBundleAsync(DecryptFileInfo fileInfo)
        {
            DecryptResult decryptResult = new DecryptResult();
            decryptResult.ManagedStream = null;
            decryptResult.CreateRequest =
                AssetBundle.LoadFromFileAsync(fileInfo.FileLoadPath, fileInfo.FileLoadCRC, GetFileOffset());
            return decryptResult;
        }

        /// <summary>
        /// 获取解密的字节数据
        /// </summary>
        byte[] IDecryptionServices.ReadFileData(DecryptFileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// 获取解密的文本数据
        /// </summary>
        string IDecryptionServices.ReadFileText(DecryptFileInfo fileInfo)
        {
            throw new System.NotImplementedException();
        }

        private static ulong GetFileOffset()
        {
            return 32;
        }
    }
}

/// <summary>
/// 资源文件解密流
/// </summary>
public class BundleStream : FileStream
{
    public const byte KEY = 64;

    public BundleStream(string path, FileMode mode, FileAccess access, FileShare share) : base(path, mode, access,
        share)
    {
    }

    public BundleStream(string path, FileMode mode) : base(path, mode)
    {
    }

    public override int Read(byte[] array, int offset, int count)
    {
        var index = base.Read(array, offset, count);
        for (int i = 0; i < array.Length; i++)
        {
            array[i] ^= KEY;
        }

        return index;
    }
}