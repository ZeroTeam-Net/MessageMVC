# MessageMVCʵ����ʲô
����Ҫʵ�ּ����ش�������������κ���Դ����Ϣ,����ʵ��һ��AspnetCore��WebApiһ����,����������

```csharp
using Agebull.Common.Logging;
using System.Threading;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.ZeroApis;

namespace ZeroTeam.MessageMVC.Sample.Controllers
{
    [Consumer("test1")]
    public class TestControler : IApiControler
    {
        [Route("test/res")]
        public ApiResult Result()
        {
            LogRecorder.Trace("Result");
            return ApiResult.Succees();
        }

    }
}
```
�������еļ���ϸ��,����Ҫ�����߹���Ĺ�ע.

# ֧�ֵ���Ϣ����
> �򵥵�ͨ�������Զ�������Ϣ�������

##  ������Ϣ����
1. Kafka(��ʵ��)
2. RabbitMq
3. ActiveMQ
4. ZMQ

## ����RPC
1. MicroZero(����Ǩ��)
2. gRPC

## ������ƽ̨��Ϣ����
1. ΢�Ź��ں�
2. ΢��֧��
3. ֧����

## ����֧��
1. ���Ե���
2. ����̨����
3. ������ͨѶ

# �����м��
ͨ��NetCoreǿ�������ע�빦��,��ÿһ�����ܶ�ͨ���м����ʽʵ��,���̶ȵĽ��������
1. ���÷���
2. ��־�м��
3. �쳣��Ϣ�洢�����������м��
4. �����ļ��м��
5. ApiExecuter�м��

