/*此标记表明此文件可被设计器更新,如果不允许此操作,请删除此行代码.design by:agebull designer date:2020/4/17 15:42:48*/
#region
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Runtime.Serialization;
using System.IO;
using Newtonsoft.Json;


using Agebull.Common;
using Agebull.EntityModel.Common;
using Agebull.EntityModel.Interfaces;


#endregion

namespace ZeroTeam.MessageMVC.MessageTraceLink
{
    /// <summary>
    /// 健康检查结果
    /// </summary>
    [JsonObject(MemberSerialization.OptIn)]
    public partial class HealthCheckData : IIdentityData
    {
        #region 构造
        
        /// <summary>
        /// 构造
        /// </summary>
        public HealthCheckData()
        {
            Initialize();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        partial void Initialize();
        #endregion

        #region 基本属性


        /// <summary>
        /// 修改主键
        /// </summary>
        public void ChangePrimaryKey(long id)
        {
            _id = id;
        }
        /// <summary>
        /// 主键
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public long _id;

        partial void OnIdGet();

        partial void OnIdSet(ref long value);

        partial void OnIdLoad(ref long value);

        partial void OnIdSeted();

        
        /// <summary>
        ///  主键
        /// </summary>
        /// <example>
        ///     0
        /// </example>
        [DataMember , JsonProperty("Id", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , ReadOnly(true) , DisplayName(@"主键")]
        public long Id
        {
            get
            {
                OnIdGet();
                return this._id;
            }
            set
            {
                if(this._id == value)
                    return;
                //if(this._id > 0)
                //    throw new Exception("主键一旦设置就不可以修改");
                OnIdSet(ref value);
                this._id = value;
                this.OnPropertyChanged(_DataStruct_.Real_Id);
                OnIdSeted();
            }
        }
        /// <summary>
        /// 检查标识
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _checkID;

        partial void OnCheckIDGet();

        partial void OnCheckIDSet(ref int value);

        partial void OnCheckIDSeted();

        
        /// <summary>
        ///  检查标识
        /// </summary>
        /// <remarks>
        ///     年月日时分组合成的数字
        /// </remarks>
        /// <example>
        ///     0
        /// </example>
        [DataMember , JsonProperty("CheckID", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"检查标识")]
        public  int CheckID
        {
            get
            {
                OnCheckIDGet();
                return this._checkID;
            }
            set
            {
                if(this._checkID == value)
                    return;
                OnCheckIDSet(ref value);
                this._checkID = value;
                OnCheckIDSeted();
                this.OnPropertyChanged(_DataStruct_.Real_CheckID);
                this.OnPropertyChanged(nameof(CheckID));
            }
        }
        /// <summary>
        /// 服务名称
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _service;

        partial void OnServiceGet();

        partial void OnServiceSet(ref string value);

        partial void OnServiceSeted();

        
        /// <summary>
        ///  服务名称
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Service", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"服务名称")]
        public  string Service
        {
            get
            {
                OnServiceGet();
                return this._service;
            }
            set
            {
                if(this._service == value)
                    return;
                OnServiceSet(ref value);
                this._service = value;
                OnServiceSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Service);
                this.OnPropertyChanged(nameof(Service));
            }
        }
        /// <summary>
        /// 服务地址
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _url;

        partial void OnUrlGet();

        partial void OnUrlSet(ref string value);

        partial void OnUrlSeted();

        
        /// <summary>
        ///  服务地址
        /// </summary>
        /// <value>
        ///     可存储400个字符.合理长度应不大于400.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Url", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"服务地址")]
        public  string Url
        {
            get
            {
                OnUrlGet();
                return this._url;
            }
            set
            {
                if(this._url == value)
                    return;
                OnUrlSet(ref value);
                this._url = value;
                OnUrlSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Url);
                this.OnPropertyChanged(nameof(Url));
            }
        }
        /// <summary>
        /// 机器名称
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _machine;

        partial void OnMachineGet();

        partial void OnMachineSet(ref string value);

        partial void OnMachineSeted();

        
        /// <summary>
        ///  机器名称
        /// </summary>
        /// <value>
        ///     可存储200个字符.合理长度应不大于200.
        /// </value>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Machine", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"机器名称")]
        public  string Machine
        {
            get
            {
                OnMachineGet();
                return this._machine;
            }
            set
            {
                if(this._machine == value)
                    return;
                OnMachineSet(ref value);
                this._machine = value;
                OnMachineSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Machine);
                this.OnPropertyChanged(nameof(Machine));
            }
        }
        /// <summary>
        /// 开始时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public DateTime _start;

        partial void OnStartGet();

        partial void OnStartSet(ref DateTime value);

        partial void OnStartSeted();

        
        /// <summary>
        ///  开始时间
        /// </summary>
        /// <example>
        ///     2012-12-21 23:59:59
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Start", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , JsonConverter(typeof(MyDateTimeConverter)) , DisplayName(@"开始时间")]
        public  DateTime Start
        {
            get
            {
                OnStartGet();
                return this._start;
            }
            set
            {
                if(this._start == value)
                    return;
                OnStartSet(ref value);
                this._start = value;
                OnStartSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Start);
                this.OnPropertyChanged(nameof(Start));
            }
        }
        /// <summary>
        /// 结束时间
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public DateTime _end;

        partial void OnEndGet();

        partial void OnEndSet(ref DateTime value);

        partial void OnEndSeted();

        
        /// <summary>
        ///  结束时间
        /// </summary>
        /// <example>
        ///     2012-12-21 23:59:59
        /// </example>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("End", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , JsonConverter(typeof(MyDateTimeConverter)) , DisplayName(@"结束时间")]
        public  DateTime End
        {
            get
            {
                OnEndGet();
                return this._end;
            }
            set
            {
                if(this._end == value)
                    return;
                OnEndSet(ref value);
                this._end = value;
                OnEndSeted();
                this.OnPropertyChanged(_DataStruct_.Real_End);
                this.OnPropertyChanged(nameof(End));
            }
        }
        /// <summary>
        /// 健康等级
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public int _level;

        partial void OnLevelGet();

        partial void OnLevelSet(ref int value);

        partial void OnLevelSeted();

        
        /// <summary>
        ///  健康等级
        /// </summary>
        /// <remarks>
        ///     0 未检查 -1异常 1-5为等级越高越好
        /// </remarks>
        /// <example>
        ///     0
        /// </example>
        [DataMember , JsonProperty("Level", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"健康等级")]
        public  int Level
        {
            get
            {
                OnLevelGet();
                return this._level;
            }
            set
            {
                if(this._level == value)
                    return;
                OnLevelSet(ref value);
                this._level = value;
                OnLevelSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Level);
                this.OnPropertyChanged(nameof(Level));
            }
        }
        /// <summary>
        /// 检查详情
        /// </summary>
        [IgnoreDataMember,JsonIgnore]
        public string _details;

        partial void OnDetailsGet();

        partial void OnDetailsSet(ref string value);

        partial void OnDetailsSeted();

        
        /// <summary>
        ///  检查详情
        /// </summary>
        [DataRule(CanNull = true)]
        [DataMember , JsonProperty("Details", NullValueHandling = NullValueHandling.Ignore, DefaultValueHandling= DefaultValueHandling.Ignore) , DisplayName(@"检查详情")]
        public  string Details
        {
            get
            {
                OnDetailsGet();
                return this._details;
            }
            set
            {
                if(this._details == value)
                    return;
                OnDetailsSet(ref value);
                this._details = value;
                OnDetailsSeted();
                this.OnPropertyChanged(_DataStruct_.Real_Details);
                this.OnPropertyChanged(nameof(Details));
            }
        }

        #region 接口属性

        #endregion
        #region 扩展属性

        #endregion
        #endregion


        #region 名称的属性操作

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected override bool SetValueInner(string property, string value)
        {
            if(property == null) return false;
            switch(property.Trim().ToLower())
            {
            case "id":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (long.TryParse(value, out var vl))
                    {
                        this.Id = vl;
                        return true;
                    }
                }
                return false;
            case "checkid":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.CheckID = vl;
                        return true;
                    }
                }
                return false;
            case "service":
                this.Service = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "url":
                this.Url = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "machine":
                this.Machine = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            case "start":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (DateTime.TryParse(value, out var vl))
                    {
                        this.Start = vl;
                        return true;
                    }
                }
                return false;
            case "end":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (DateTime.TryParse(value, out var vl))
                    {
                        this.End = vl;
                        return true;
                    }
                }
                return false;
            case "level":
                if (!string.IsNullOrWhiteSpace(value))
                {
                    if (int.TryParse(value, out var vl))
                    {
                        this.Level = vl;
                        return true;
                    }
                }
                return false;
            case "details":
                this.Details = string.IsNullOrWhiteSpace(value) ? null : value;
                return true;
            }
            return false;
        }

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="property"></param>
        /// <param name="value"></param>
        protected override void SetValueInner(string property, object value)
        {
            if(property == null) return;
            switch(property.Trim().ToLower())
            {
            case "id":
                this.Id = (long)Convert.ToDecimal(value);
                return;
            case "checkid":
                this.CheckID = (int)Convert.ToDecimal(value);
                return;
            case "service":
                this.Service = value == null ? null : value.ToString();
                return;
            case "url":
                this.Url = value == null ? null : value.ToString();
                return;
            case "machine":
                this.Machine = value == null ? null : value.ToString();
                return;
            case "start":
                this.Start = Convert.ToDateTime(value);
                return;
            case "end":
                this.End = Convert.ToDateTime(value);
                return;
            case "level":
                this.Level = (int)Convert.ToDecimal(value);
                return;
            case "details":
                this.Details = value == null ? null : value.ToString();
                return;
            }

            //System.Diagnostics.Trace.WriteLine(property + @"=>" + value);

        }

    

        /// <summary>
        ///     设置属性值
        /// </summary>
        /// <param name="index"></param>
        /// <param name="value"></param>
        protected override void SetValueInner(int index, object value)
        {
            switch(index)
            {
            case _DataStruct_.Id:
                this.Id = Convert.ToInt64(value);
                return;
            case _DataStruct_.CheckID:
                this.CheckID = Convert.ToInt32(value);
                return;
            case _DataStruct_.Service:
                this.Service = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Url:
                this.Url = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Machine:
                this.Machine = value == null ? null : value.ToString();
                return;
            case _DataStruct_.Start:
                this.Start = Convert.ToDateTime(value);
                return;
            case _DataStruct_.End:
                this.End = Convert.ToDateTime(value);
                return;
            case _DataStruct_.Level:
                this.Level = Convert.ToInt32(value);
                return;
            case _DataStruct_.Details:
                this.Details = value == null ? null : value.ToString();
                return;
            }
        }


        /// <summary>
        ///     读取属性值
        /// </summary>
        /// <param name="property"></param>
        protected override object GetValueInner(string property)
        {
            switch(property)
            {
            case "id":
                return this.Id;
            case "checkid":
                return this.CheckID;
            case "service":
                return this.Service;
            case "url":
                return this.Url;
            case "machine":
                return this.Machine;
            case "start":
                return this.Start;
            case "end":
                return this.End;
            case "level":
                return this.Level;
            case "details":
                return this.Details;
            }

            return null;
        }


        /// <summary>
        ///     读取属性值
        /// </summary>
        /// <param name="index"></param>
        protected override object GetValueInner(int index)
        {
            switch(index)
            {
                case _DataStruct_.Id:
                    return this.Id;
                case _DataStruct_.CheckID:
                    return this.CheckID;
                case _DataStruct_.Service:
                    return this.Service;
                case _DataStruct_.Url:
                    return this.Url;
                case _DataStruct_.Machine:
                    return this.Machine;
                case _DataStruct_.Start:
                    return this.Start;
                case _DataStruct_.End:
                    return this.End;
                case _DataStruct_.Level:
                    return this.Level;
                case _DataStruct_.Details:
                    return this.Details;
            }

            return null;
        }

        #endregion

        #region 复制
        

        partial void CopyExtendValue(HealthCheckData source);

        /// <summary>
        /// 复制值
        /// </summary>
        /// <param name="source">复制的源字段</param>
        protected override void CopyValueInner(DataObjectBase source)
        {
            var sourceEntity = source as HealthCheckData;
            if(sourceEntity == null)
                return;
            this._id = sourceEntity._id;
            this._checkID = sourceEntity._checkID;
            this._service = sourceEntity._service;
            this._url = sourceEntity._url;
            this._machine = sourceEntity._machine;
            this._start = sourceEntity._start;
            this._end = sourceEntity._end;
            this._level = sourceEntity._level;
            this._details = sourceEntity._details;
            CopyExtendValue(sourceEntity);
            this.__status.IsModified = true;
        }

        /// <summary>
        /// 复制
        /// </summary>
        /// <param name="source">复制的源字段</param>
        public void Copy(HealthCheckData source)
        {
                this.Id = source.Id;
                this.CheckID = source.CheckID;
                this.Service = source.Service;
                this.Url = source.Url;
                this.Machine = source.Machine;
                this.Start = source.Start;
                this.End = source.End;
                this.Level = source.Level;
                this.Details = source.Details;
        }
        #endregion

        #region 后期处理
        

        /// <summary>
        /// 单个属性修改的后期处理(保存后)
        /// </summary>
        /// <param name="subsist">当前实体生存状态</param>
        /// <param name="modifieds">修改列表</param>
        /// <remarks>
        /// 对当前对象的属性的更改,请自行保存,否则将丢失
        /// </remarks>
        protected override void OnLaterPeriodBySignleModified(EntitySubsist subsist,byte[] modifieds)
        {
            if (subsist == EntitySubsist.Deleting)
            {
                OnIdModified(subsist,false);
                OnCheckIDModified(subsist,false);
                OnServiceModified(subsist,false);
                OnUrlModified(subsist,false);
                OnMachineModified(subsist,false);
                OnStartModified(subsist,false);
                OnEndModified(subsist,false);
                OnLevelModified(subsist,false);
                OnDetailsModified(subsist,false);
                return;
            }
            else if (subsist == EntitySubsist.Adding || subsist == EntitySubsist.Added)
            {
                OnIdModified(subsist,true);
                OnCheckIDModified(subsist,true);
                OnServiceModified(subsist,true);
                OnUrlModified(subsist,true);
                OnMachineModified(subsist,true);
                OnStartModified(subsist,true);
                OnEndModified(subsist,true);
                OnLevelModified(subsist,true);
                OnDetailsModified(subsist,true);
                return;
            }
            else if(modifieds != null && modifieds[9] > 0)
            {
                OnIdModified(subsist,modifieds[_DataStruct_.Real_Id] == 1);
                OnCheckIDModified(subsist,modifieds[_DataStruct_.Real_CheckID] == 1);
                OnServiceModified(subsist,modifieds[_DataStruct_.Real_Service] == 1);
                OnUrlModified(subsist,modifieds[_DataStruct_.Real_Url] == 1);
                OnMachineModified(subsist,modifieds[_DataStruct_.Real_Machine] == 1);
                OnStartModified(subsist,modifieds[_DataStruct_.Real_Start] == 1);
                OnEndModified(subsist,modifieds[_DataStruct_.Real_End] == 1);
                OnLevelModified(subsist,modifieds[_DataStruct_.Real_Level] == 1);
                OnDetailsModified(subsist,modifieds[_DataStruct_.Real_Details] == 1);
            }
        }

        /// <summary>
        /// 主键修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnIdModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 检查标识修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnCheckIDModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 服务名称修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnServiceModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 服务地址修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnUrlModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 机器名称修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnMachineModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 开始时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnStartModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 结束时间修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnEndModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 健康等级修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnLevelModified(EntitySubsist subsist,bool isModified);

        /// <summary>
        /// 检查详情修改的后期处理(保存前)
        /// </summary>
        /// <param name="subsist">当前对象状态</param>
        /// <param name="isModified">是否被修改</param>
        /// <remarks>
        /// 对关联的属性的更改,请自行保存,否则可能丢失
        /// </remarks>
        partial void OnDetailsModified(EntitySubsist subsist,bool isModified);
        #endregion

        #region 数据结构

        /// <summary>
        /// 实体结构
        /// </summary>
        [IgnoreDataMember,Browsable (false)]
        public override EntitySturct __Struct
        {
            get
            {
                return _DataStruct_.Struct;
            }
        }
        /// <summary>
        /// 实体结构
        /// </summary>
        public class _DataStruct_
        {
            /// <summary>
            /// 实体名称
            /// </summary>
            public const string EntityName = @"HealthCheck";
            /// <summary>
            /// 实体标题
            /// </summary>
            public const string EntityCaption = @"健康检查";
            /// <summary>
            /// 实体说明
            /// </summary>
            public const string EntityDescription = @"健康检查结果";
            /// <summary>
            /// 实体标识
            /// </summary>
            public const int EntityIdentity = 0x0;
            /// <summary>
            /// 实体说明
            /// </summary>
            public const string EntityPrimaryKey = "Id";
            
            
            /// <summary>
            /// 主键的数字标识
            /// </summary>
            public const int Id = 0;
            
            /// <summary>
            /// 主键的实时记录顺序
            /// </summary>
            public const int Real_Id = 0;

            /// <summary>
            /// 检查标识的数字标识
            /// </summary>
            public const int CheckID = 1;
            
            /// <summary>
            /// 检查标识的实时记录顺序
            /// </summary>
            public const int Real_CheckID = 1;

            /// <summary>
            /// 服务名称的数字标识
            /// </summary>
            public const int Service = 2;
            
            /// <summary>
            /// 服务名称的实时记录顺序
            /// </summary>
            public const int Real_Service = 2;

            /// <summary>
            /// 服务地址的数字标识
            /// </summary>
            public const int Url = 3;
            
            /// <summary>
            /// 服务地址的实时记录顺序
            /// </summary>
            public const int Real_Url = 3;

            /// <summary>
            /// 机器名称的数字标识
            /// </summary>
            public const int Machine = 4;
            
            /// <summary>
            /// 机器名称的实时记录顺序
            /// </summary>
            public const int Real_Machine = 4;

            /// <summary>
            /// 开始时间的数字标识
            /// </summary>
            public const int Start = 5;
            
            /// <summary>
            /// 开始时间的实时记录顺序
            /// </summary>
            public const int Real_Start = 5;

            /// <summary>
            /// 结束时间的数字标识
            /// </summary>
            public const int End = 6;
            
            /// <summary>
            /// 结束时间的实时记录顺序
            /// </summary>
            public const int Real_End = 6;

            /// <summary>
            /// 健康等级的数字标识
            /// </summary>
            public const int Level = 7;
            
            /// <summary>
            /// 健康等级的实时记录顺序
            /// </summary>
            public const int Real_Level = 7;

            /// <summary>
            /// 检查详情的数字标识
            /// </summary>
            public const int Details = 8;
            
            /// <summary>
            /// 检查详情的实时记录顺序
            /// </summary>
            public const int Real_Details = 8;

            /// <summary>
            /// 实体结构
            /// </summary>
            public static readonly EntitySturct Struct = new EntitySturct
            {
                EntityName = EntityName,
                Caption    = EntityCaption,
                Description= EntityDescription,
                PrimaryKey = EntityPrimaryKey,
                EntityType = EntityIdentity,
                Properties = new Dictionary<int, PropertySturct>
                {
                    {
                        Real_Id,
                        new PropertySturct
                        {
                            Index        = Id,
                            Name         = "Id",
                            Caption      = @"主键",
                            JsonName     = "Id",
                            ColumnName   = "id",
                            PropertyType = typeof(long),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 8,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"主键"
                        }
                    },
                    {
                        Real_CheckID,
                        new PropertySturct
                        {
                            Index        = CheckID,
                            Name         = "CheckID",
                            Caption      = @"检查标识",
                            JsonName     = "CheckID",
                            ColumnName   = "check_id",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"年月日时分组合成的数字"
                        }
                    },
                    {
                        Real_Service,
                        new PropertySturct
                        {
                            Index        = Service,
                            Name         = "Service",
                            Caption      = @"服务名称",
                            JsonName     = "Service",
                            ColumnName   = "service",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"服务名称"
                        }
                    },
                    {
                        Real_Url,
                        new PropertySturct
                        {
                            Index        = Url,
                            Name         = "Url",
                            Caption      = @"服务地址",
                            JsonName     = "Url",
                            ColumnName   = "url",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"服务地址"
                        }
                    },
                    {
                        Real_Machine,
                        new PropertySturct
                        {
                            Index        = Machine,
                            Name         = "Machine",
                            Caption      = @"机器名称",
                            JsonName     = "Machine",
                            ColumnName   = "machine",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 15,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"机器名称"
                        }
                    },
                    {
                        Real_Start,
                        new PropertySturct
                        {
                            Index        = Start,
                            Name         = "Start",
                            Caption      = @"开始时间",
                            JsonName     = "Start",
                            ColumnName   = "start",
                            PropertyType = typeof(DateTime),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 12,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"开始时间"
                        }
                    },
                    {
                        Real_End,
                        new PropertySturct
                        {
                            Index        = End,
                            Name         = "End",
                            Caption      = @"结束时间",
                            JsonName     = "End",
                            ColumnName   = "end",
                            PropertyType = typeof(DateTime),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 12,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"结束时间"
                        }
                    },
                    {
                        Real_Level,
                        new PropertySturct
                        {
                            Index        = Level,
                            Name         = "Level",
                            Caption      = @"健康等级",
                            JsonName     = "Level",
                            ColumnName   = "level",
                            PropertyType = typeof(int),
                            CanNull      = false,
                            ValueType    = PropertyValueType.Value,
                            DbType       = 3,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"0 未检查 -1异常 1-5为等级越高越好"
                        }
                    },
                    {
                        Real_Details,
                        new PropertySturct
                        {
                            Index        = Details,
                            Name         = "Details",
                            Caption      = @"检查详情",
                            JsonName     = "Details",
                            ColumnName   = "details",
                            PropertyType = typeof(string),
                            CanNull      = false,
                            ValueType    = PropertyValueType.String,
                            DbType       = 752,
                            CanImport    = false,
                            CanExport    = false,
                            Description  = @"检查详情"
                        }
                    }
                }
            };
        }
        #endregion

    }
}