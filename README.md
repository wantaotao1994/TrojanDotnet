# fay
A Trojan client  implementation by .net core 


# 声明：

基于个人爱好，以及学习网络知识所用，不用于其他任何的行为。以及代码稀烂，欢迎给位指教。






# 需要环境：

.net core 3.1





# 使用:
client.json

      {
         "ProxySetting": {
           "Trojan": {
             "Host": "host.com",  //远程服务器
             "Port": 443,   //端口
             "Pass": "your pass" //trojan 密码
           },

           "HttpProxPort": 8077, //本地http代理地址
           "PacServerPort":7900  //本地pac地址
         }
      }
暂时没有验证ssl证书 需要的自己添加即可。



# License

[WTFPL](https://en.wikipedia.org/wiki/WTFPL)

