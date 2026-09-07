using Microsoft.Win32;
using MyComputerManager.Models;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace MyComputerManager.Helpers
{
    /// <summary>
    /// 单个可被隐藏的"此电脑"系统文件夹项
    /// </summary>
    public class ThisPcFolderItem
    {
        public string Name { get; set; }
        public string Clsid { get; set; }

        /// <summary>当前是否已隐藏</summary>
        public bool IsHidden { get; set; }

        public ThisPcFolderItem(string name, string clsid)
        {
            Name = name;
            Clsid = clsid;
        }
    }

    /// <summary>
    /// 管理"此电脑"中 3D对象/桌面/文档/下载/图片/音乐/视频 7 个系统文件夹的显示与隐藏。
    /// 原理：修改 HKLM\...\Explorer\FolderDescriptions\{CLSID}\PropertyBag 的 ThisPCPolicy 值为 Show/Hide，
    /// 同时处理 64 位系统的 WOW6432Node 视图，并用 SHChangeNotify 通知资源管理器刷新。
    /// 注意：写 HKLM 需要管理员权限。
    /// </summary>
    public static class ThisPcFolderHelper
    {
        private const string PolicyValueName = "ThisPCPolicy";
        private const string HideValue = "Hide";
        private const string ShowValue = "Show";

        // CLSID 与 "此电脑" 中系统文件夹的对应关系（均来自 Windows 官方 FolderDescriptions）
        private static readonly (string Name, string Clsid)[] Definitions =
        {
            ("3D对象",   "{31C0DD25-9439-4F12-BF41-7FF4EDA38722}"),
            ("桌面",     "{B4BFCC3A-DB2C-424C-B029-7FE99A87C641}"),
            ("下载",     "{7D83EE9B-2244-4E70-B1F5-5393042AF1E4}"),
            ("文档",     "{F42EE2D3-909F-4907-8871-4C22FC0BF756}"),
            ("图片",     "{0DDD015D-B06C-45D5-8C4C-F59713854639}"),
            ("音乐",     "{A0C69A99-21C8-4671-8703-7934162FCF1D}"),
            ("视频",     "{35286A68-3C57-41A1-BBB1-0EAE73D76C95}"),
        };

        // 构建 FolderDescriptions 下的完整注册表路径前缀
        private const string RootKeyPath =
            @"SOFTWARE\Microsoft\Windows\CurrentVersion\Explorer\FolderDescriptions";

        /// <summary>
        /// 获取当前可管理的全部文件夹及其隐藏状态
        /// </summary>
        public static List<ThisPcFolderItem> GetItems()
        {
            var list = new List<ThisPcFolderItem>();
            foreach (var def in Definitions)
            {
                list.Add(new ThisPcFolderItem(def.Name, def.Clsid)
                {
                    IsHidden = IsHiddenInternal(def.Clsid)
                });
            }
            return list;
        }

        /// <summary>
        /// 设置指定 CLSID 文件夹的显示/隐藏状态
        /// </summary>
        public static CommonResult SetHidden(string clsid, bool hide)
        {
            try
            {
                string value = hide ? HideValue : ShowValue;
                bool is64Bit = Environment.Is64BitOperatingSystem;

                // 64 位系统需同时处理 WOW6432Node 视图，避免 32 位程序看到不一致
                if (is64Bit)
                {
                    using var key64 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64);
                    SetPolicyCore(key64, clsid, value);

                    using var key32 = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32);
                    SetPolicyCore(key32, clsid, value);
                }
                else
                {
                    using var key = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Default);
                    SetPolicyCore(key, clsid, value);
                }

                RefreshShell();
                return new CommonResult(true, "成功");
            }
            catch (UnauthorizedAccessException)
            {
                return new CommonResult(false, "当前无管理员权限，无法修改系统文件夹设置。请以管理员身份运行本程序。");
            }
            catch (Exception ex)
            {
                return new CommonResult(false, ex.Message);
            }
        }

        private static void SetPolicyCore(RegistryKey root, string clsid, string value)
        {
            var propertyBag = root.CreateSubKey($@"{RootKeyPath}\{clsid}\PropertyBag", true);
            if (propertyBag == null)
                throw new System.IO.IOException($"无法打开注册表键 {RootKeyPath}\\{clsid}\\PropertyBag");

            using (propertyBag)
            {
                propertyBag.SetValue(PolicyValueName, value, RegistryValueKind.String);
            }
        }

        private static bool IsHiddenInternal(string clsid)
        {
            try
            {
                RegistryHive hive = RegistryHive.LocalMachine;
                RegistryView[] views;

                if (Environment.Is64BitOperatingSystem)
                {
                    views = new[] { RegistryView.Registry64 };
                }
                else
                {
                    views = new[] { RegistryView.Default };
                }

                foreach (var view in views)
                {
                    using var root = RegistryKey.OpenBaseKey(hive, view);
                    using var propertyBag = root.OpenSubKey($@"{RootKeyPath}\{clsid}\PropertyBag", false);
                    if (propertyBag != null)
                    {
                        var policy = propertyBag.GetValue(PolicyValueName) as string;
                        if (string.Equals(policy, HideValue, StringComparison.OrdinalIgnoreCase))
                            return true;
                    }
                }
                return false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 通知资源管理器刷新图标与命名空间，使隐藏/显示立即生效
        /// </summary>
        public static void RefreshShell()
        {
            const uint SHCNE_ASSOCCHANGED = 0x08000000;
            const uint SHCNF_IDLIST = 0x0000;
            const uint SHCNF_FLUSH = 0x1000;
            NativeMethods.SHChangeNotify(SHCNE_ASSOCCHANGED, SHCNF_IDLIST | SHCNF_FLUSH, IntPtr.Zero, IntPtr.Zero);
        }

        private static class NativeMethods
        {
            [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            public static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
        }
    }
}