# 服务发现
messageposter注册，zerocenter的station

# 服务发现分两种场景
- 外部调用的网关发现
- 本地调用的messageposter发现

# 发现方式分两种
> 调用者视角，此处为MessagePoster

- 主动发现
> 通过读取配置文件，被动拉取配置中心服务数据
 -  启动时拉取zerocenter站点配置
 -  启动时读取poster配置文件

- 被动发现
> 服务对象主动推送或事件通知，
 -  zerocenter站点变更事件
 -  配置中心，配置变更事件
 -  配置中心首次推送
 
# 配置中间件
> config
