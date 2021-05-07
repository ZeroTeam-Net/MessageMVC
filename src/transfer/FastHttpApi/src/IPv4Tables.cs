﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BeetleX.FastHttpApi
{
    public class IPv4Tables
    {
        const string IPTABLE_FILE = "iptables.json";

        public VerifyType Type { get; set; } = VerifyType.None;

        private readonly List<IPv4Match> mWhiteList = new();

        private readonly List<IPv4Match> mBlackList = new();

        private IPv4Match[] mWhiteMatchs = new IPv4Match[0];

        private IPv4Match[] mBlackMatchs = new IPv4Match[0];

        public void RemoveWhite(string ip)
        {
            mWhiteList.RemoveAll(p => p.Source == ip);
            Reload();
        }

        public void RemoveBlack(string ip)
        {
            mBlackList.RemoveAll(p => p.Source == ip);
            Reload();
        }

        public void AddWhite(params string[] ips)
        {
            foreach (var ip in ips)
            {
                var item = IPv4Match.GetIpMatch(ip);
                if (item != null)
                {
                    mWhiteList.Add(item);
                }
            }
            Reload();
        }

        public void AddBlack(params string[] ips)
        {
            foreach (var ip in ips)
            {
                var item = IPv4Match.GetIpMatch(ip);
                if (item != null)
                {
                    mBlackList.Add(item);
                }
            }
            Reload();
        }



        public void CleanWhite()
        {
            mWhiteList.Clear();
            Reload();
        }

        public void CleanBlack()
        {
            mBlackList.Clear();
            Reload();
        }

        private void Reload()
        {
            mWhiteMatchs = mWhiteList.ToArray();
            mBlackMatchs = mBlackList.ToArray();
        }

        public void SetConfig(Config config)
        {
            CleanWhite();
            CleanBlack();
            Type = config.Type;
            if (config.WhiteList != null)
                AddWhite(config.WhiteList);
            if (config.BlackList != null)
                AddBlack(config.BlackList);
        }

        public Config GetConfig()
        {
            return new Config
            {
                Type = Type,
                BlackList = (from a in mBlackList select a.Source).ToArray(),
                WhiteList = (from a in mWhiteList select a.Source).ToArray()
            };
        }

        public void Save()
        {
            using System.IO.StreamWriter writer = new(IPTABLE_FILE, false);
            writer.Write(JsonConvert.SerializeObject(GetConfig()));
            writer.Flush();
        }

        public void Load()
        {
            if (System.IO.File.Exists(IPTABLE_FILE))
            {
                using System.IO.StreamReader reader = new(IPTABLE_FILE);
                string data = reader.ReadToEnd();
                var config = JsonConvert.DeserializeObject<Config>(data);
                SetConfig(config);
            }
        }

        public bool Verify(System.Net.IPAddress ipaddress)
        {
            if (Type == VerifyType.None)
                return true;
            if (!ipaddress.IsIPv4MappedToIPv6)
            {
                if (ipaddress.GetAddressBytes().Length > 4)
                {
                    return true;
                }
            }
            if (Type == VerifyType.Black)
            {
                IPv4Match[] items = mBlackMatchs;
                foreach (var item in items)
                    if (item.Match(ipaddress))
                        return false;
                return true;
            }
            else
            {
                IPv4Match[] items = mWhiteMatchs;
                foreach (var item in items)
                    if (item.Match(ipaddress))
                        return true;
                return false;
            }
        }

        public enum VerifyType
        {
            None,
            White,
            Black

        }

        public class Config
        {
            [JsonConverter(typeof(StringEnumConverter))]
            public VerifyType Type { get; set; }

            public string[] WhiteList { get; set; }

            public string[] BlackList { get; set; }
        }

    }

    public class IPv4Match
    {
        public IPv4Match(string source, int ip, int? mark)
        {
            Source = source;
            IPValue = ip;
            if (mark > 0)
            {
                Mark = 0xFFFFFFFF << (32 - mark.Value);
            }
        }

        public string Source { get; set; }

        public static IPv4Match GetIpMatch(string ip)
        {
            string[] values = ip.Split('/');
            if (values.Length == 1)
            {
                if (System.Net.IPAddress.TryParse(values[0], out System.Net.IPAddress address))
                {
                    return new IPv4Match(ip, GetIP(address.GetAddressBytes()), null);
                }
            }
            else
            {

                if (System.Net.IPAddress.TryParse(values[0], out System.Net.IPAddress address))
                {
                    if (int.TryParse(values[1], out int mark))
                    {
                        if (mark > 31)
                            mark = 31;
                        return new IPv4Match(ip, GetIP(address.GetAddressBytes()), mark);
                    }
                }

            }
            return null;
        }

        private static int GetIP(Span<byte> data)
        {
            return data[0] << 24 | data[1] << 16 | data[2] << 8 | data[3];
        }

        private readonly int IPValue;

        private readonly uint? Mark;

        public bool Match(System.Net.IPAddress remote)
        {
            var bytes = remote.GetAddressBytes();
            int ipdata;

            if (remote.IsIPv4MappedToIPv6)
            {
                ipdata = GetIP(bytes.AsSpan()[(bytes.Length - 4)..]);
            }
            else
            {
                ipdata = GetIP(bytes);
            }
            if (ipdata == 1 || ipdata == 0)
                return true;
            if (Mark == null)
                return IPValue == ipdata;
            else
                return (IPValue & Mark.Value) == (ipdata & Mark.Value);
        }
    }
}
