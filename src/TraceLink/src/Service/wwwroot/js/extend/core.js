var apihost = "/";
//var apihost ="/api"
// var version = 20190503;
var globalOptions = {
    /**
     * 应用标识
     */
    appId: '10V6WMADM',
    /**
     * api访问配置
     */
    api: {
        /**
         * Demo的api访问地址
         */
        trace_linkApiHost: "localhost:8000",
        /**
         * api访问基地址
         */
        defApiHost: 'User',
        /**
         * api访问基地址
         */
        userApiHost: 'User',
        /**
         * 用户中心api访问基地址
         */
        authApiHost: 'Authority',
        /**
         * 页面api访问基地址
         */
        appApiHost: 'App',
        /**
        * 超时时间设置为10秒
        */
        timeOut: 30000,
        /**
         * 自定义api访问地址
         */
        customHost: null,
        hpcApiHost: "Hpc"
    },

    /**
	 * 组合并得到一个正确的api调用的url
     * @param {string} addr 地址
     * @param {string} host 主机
	 * @returns {string} url
	 */
    geturl: function (addr, host) {
        var href;
        if (!host)
            href = globalOptions.api.customHost
                ? globalOptions.api.customHost
                : globalOptions.api.defApiHost;
        else
            href = trimStr(host, "/\\");
        addr = trimStr(addr, "/\\");
        // var ver = addr.indexOf("?") > 0
        //     ? `&v__=${version}`
        //     : `?v__=${version}`;
        var ver = "";
        return `${apihost}/${href}/${addr}${ver}`;
    },

    /**
	 * 系统操作
	 */
    user: {
		/**
		 * 客户端令牌
		 * @returns {string} 令牌
		 */
        getToken: function () {
            if (typeof localStorage.token !== 'undefined')
                return localStorage.token;
            if (typeof localStorage.sys_access_token !== 'undefined')
                return localStorage.sys_access_token;
            if (typeof localStorage.sys_device_id !== 'undefined')
                return localStorage.sys_device_id;
            return "";
        },
		/**
		 * 客户端令牌
		 * @returns {string} 令牌
		 */
        getAccessToken: function () {
            if (typeof localStorage.token !== 'undefined')
                return localStorage.token;
            if (typeof localStorage.sys_access_token !== 'undefined')
                return localStorage.sys_access_token;
            return "";
        },
		/**
		 * 客户端设备标识
		 * @returns {string} 设备标识
		 */
        getDeviceId: function () {
            if (typeof localStorage.sys_device_id !== 'undefined')
                return localStorage.sys_device_id;
            return "";
        },
		/**
		 * 客户端设备标识
		 * @param {string} did 设备标识
		 */
        setDeviceId: function (did) {
            if (did)
                localStorage.sys_device_id = did;
            else
                localStorage.removeItem('sys_device_id');
        },
		/**
		 * 保存客户端缓存登录者信息
		 * @param {object} loginInfo 登录者信息
		 */
        setUserInfo: function (loginInfo) {
            localStorage.token = loginInfo.AccessToken;
            localStorage.sys_access_token = loginInfo.AccessToken;
            localStorage.sys_refresh_token = loginInfo.RefreshToken;
            if (loginInfo.Profile) {
                if (loginInfo.Profile.UserId)
                    localStorage.sys_user_id = loginInfo.Profile.UserId;
                if (loginInfo.Profile.PhoneNumber)
                    localStorage.sys_phone = loginInfo.Profile.PhoneNumber;
                if (loginInfo.Profile.NickName)
                    localStorage.sys_nick_name = loginInfo.Profile.NickName;
                if (loginInfo.Profile.AvatarUrl)
                    localStorage.sys_avatar_url = loginInfo.Profile.AvatarUrl;
                if (loginInfo.WechatAccessTokenExpires)
                    localStorage.wechat_access_token_expires = loginInfo.WechatAccessTokenExpires;
            }
        },
		/**
		 * 保存客户端缓存登录者信息
		 * @param {object} loginInfo 登录者信息
		 */
        setHpcUserInfo: function (loginInfo) {
            localStorage.token = loginInfo.token;
            localStorage.sys_access_token = loginInfo.rt;
        },
		/**
		 * 登录状态检查
		 * @returns {void}  
		 */
        isLogin: function () {
            return localStorage.sys_access_token;
        },
        logout: function (inLogin) {
            localStorage.removeItem("token");
            localStorage.removeItem("sys_access_token");
            localStorage.removeItem("sys_refresh_token");
            localStorage.removeItem("sys_user_id");
            localStorage.removeItem("sys_phone");
            localStorage.removeItem("sys_nick_name");
            localStorage.removeItem("sys_avatar_url");
            localStorage.removeItem("sys_expires_in");
            localStorage.removeItem("sys_diacounts");
            localStorage.removeItem("sys_usercounts");
            localStorage.removeItem("wechat_access_token_expires");

            if (!inLogin) {
                this.goLogin(window);
            }
        },
		/**
		 * 清除运行时缓存信息
		 * @param {int} type 1 指刚进去app时，有些缓存信息只有在刚进入app时才清除一次
		 * @returns {void}  
		 * 
		 */
        clearTempValue: function (type) {
            localStorage.removeItem("temp");
            localStorage.removeItem("tmp_nav_paths");
            localStorage.removeItem("tmp_diamondNum");
            localStorage.removeItem("tmp_diamondSource");
            localStorage.removeItem("tmp_everyId");
            localStorage.removeItem("tmp_everyVerSion");
            localStorage.removeItem("tmp_VerifyImgId");
            localStorage.removeItem("tmp_VerifyCodeContent");
            localStorage.removeItem("tmp_pho_num");
            localStorage.removeItem("tmp_verifyImgId");
            localStorage.removeItem("tmp_typid");
            localStorage.removeItem("tmp_images_check");

            if (type === 1) {
                localStorage.removeItem("tmp_did_lock");
                localStorage.removeItem("tmp_token_lock");
                localStorage.removeItem("tmp_tokenOK");
                localStorage.removeItem("tmp_hasInviteShow");
                localStorage.removeItem("tmp_sdkIsShow");
                localStorage.removeItem("tmp_backdoor_lock");
            }
        },
		/**
		 * 缓存NickName
		 * @param {string} value NickName
		 * @returns {void}
		 */
        setNickName: function (value) {
            localStorage.sys_nick_name = value;
        },
		/**
		 * 取缓存NickName
		 * @returns {string} NickName
		 */
        getNickName: function () {
            return localStorage.sys_nick_name;
        },
		/**
		 * 缓存用户头像
		 * @param {string} value 用户头像
		 * @returns {void} 
		 */
        setAvatarUrl: function (value) {
            localStorage.sys_avatar_url = value;
        },
		/**
		 * 取缓存用户头像
		 * @returns {object}  用户头像
		 */
        getAvatarUrl: function () {
            return localStorage.sys_avatar_url;
        },
		/**
		 * 取得登录用户信息
		 * @returns {object}  登录用户信息
		 */
        info: function () {
            return {
                userId: localStorage.sys_user_id,
                phone: localStorage.sys_phone,
                nickName: localStorage.sys_nick_name,
                avatarUrl: localStorage.sys_avatar_url,
                expiresIn: localStorage.sys_expires_in
            };
        },
		/**
		 * 取得用户ID
		 * @returns {int}  用户ID
		 */
        userId: function () {
            return localStorage.sys_user_id;
        },
        goLogin(win) {
            if (win.top !== win.self) {
                if (window.top.location.host) {
                    this.goLogin(win.parent);
                    return;
                }
            }
            win.location.href = "/login.html";
        },
        refreshDid() {
            console.log("Refresh device ID(...)");
            var that = this;
            ajax_post("初始化", "v1/refresh/did",
                {
                    appId: '10V6WMADM',
                    deviceId: this.getDeviceId()
                },
                function (jsonStr) {
                    var r = hpc_ajax.evalResult(jsonStr);
                    if (r && r.success) {
                        that.setDeviceId(r.data);
                        return;
                    }
                    localStorage.removeItem('sys_device_id');
                    if (r) {
                        if (r.status.code === 40022) {
                            that.refreshDid();
                        } else {
                            console.log("Refresh device ID(error):" + r.status.msg);
                        }
                    } else {
                        console.log("Refresh device ID(null):" + jsonStr);
                    }
                },
                null,
                globalOptions.api.authApiHost);
        },
        showLogout() {
            var that = this;
            common.confirm("登录过期", "当前登录已经过期,需要重新登录,是否继续?", () => {
                that.logout(false);
            });
        },
        inRefreshAt: false,
        waitCallback: [],
        refreshAt(callback) {
            this.waitCallback.push(callback);
            if (this.inRefreshAt) {
                return;
            }
            this.inRefreshAt = true;
            console.log("Refresh access token(...)");
            var that = this;
            ajax_post("刷新令牌", "v1/refresh/at",
                {
                    AccessToken: localStorage.token,
                    RefreshToken: localStorage.sys_refresh_token
                },
                function (jsonStr) {
                    this.inRefreshAt = false;
                    var res = hpc_ajax.evalResult(jsonStr);
                    if (res && res.success) {
                        localStorage.token = res.data.AccessToken;
                        localStorage.sys_access_token = res.data.AccessToken;
                        localStorage.sys_refresh_token = res.data.RefreshToken;
                        for (var idx = 0; idx < that.waitCallback.length; idx++) {
                            if (that.waitCallback[idx])
                                that.waitCallback[idx]();
                        }
                        return;
                    }
                    if (res) {
                        console.log("Refresh access token(error):" + res.status.msg);
                    } else {
                        console.log("Refresh device ID(error):" + res.status.msg);
                    }
                    that.showLogout();
                },
                function () {
                    this.inRefreshAt = false;
                    that.showLogout();
                },
                globalOptions.api.authApiHost
            );
        },
        checkSysErrorCode(result, callback) {
            if (!result || !result.status) {
                return false;
            }
            switch (result.status.code) {
                //case -13:
                //    location.href = "/deny.html";
                //    return true;
                case 40036:
                    this.refreshAt(callback);
                    return true;
                case 40022:
                case 40001:
                case 40083:
                case 40082:
                case 40081:
                case 40421:
                    common.confirm('登录过期', '登录过期,需要跳转到登录页面重新登录！', () => {
                        globalOptions.user.logout();
                    });
                    return true;
            }
            return false;
        }
    }
};
