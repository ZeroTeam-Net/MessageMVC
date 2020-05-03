using Agebull.Common;
using Agebull.Common.Ioc;
using Agebull.Common.Logging;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZeroTeam.MessageMVC.Context;
using ZeroTeam.MessageMVC.Messages;
using ZeroTeam.MessageMVC.PlanTasks.BusinessLogic;

namespace ZeroTeam.MessageMVC.PlanTasks
{

    public static class EnumHelper
    {
        /// <summary>
        ///     ����״̬��������ת��
        /// </summary>
        public static string ToCaption(this PlanMessageState value)
        {
            return value switch
            {
                PlanMessageState.none => "��״̬",
                PlanMessageState.queue => "�Ŷ�",
                PlanMessageState.execute => "����ִ��",
                PlanMessageState.retry => "����ִ��",
                PlanMessageState.skip => "����",
                PlanMessageState.pause => "��ͣ",
                PlanMessageState.waiting => "Զ�̵ȴ�",
                PlanMessageState.error => "����ر�",
                PlanMessageState.close => "�����ر�",
                PlanMessageState.remove => "ɾ��",
                _ => "����״̬����(����)",
            };
        }

        /// <summary>
        ///     ִ��״̬��������ת��
        /// </summary>
        public static string ToCaption(this MessageState value)
        {
            return value switch
            {
                MessageState.None => "δ����",
                MessageState.Cancel => "ȡ������",
                MessageState.NonSupport => "��֧�ִ���",
                MessageState.Accept => "�ѽ���",
                MessageState.Unsend => "δ����",
                MessageState.Send => "�ѷ���",
                MessageState.Recive => "�ѽ���",
                MessageState.Processing => "���ڴ���",
                MessageState.AsyncQueue => "�첽�Ŷ�",
                MessageState.Success => "����ɹ�",
                MessageState.Failed => "����ʧ��",
                MessageState.Unhandled => "�޴�����",
                MessageState.Deny => "�ܾ�����",
                MessageState.FormalError => "��ʽ����",
                MessageState.NetworkError => "�������",
                MessageState.BusinessError => "�������",
                MessageState.NoUs => "����MessageMVC����",
                MessageState.FrameworkError => "��ܴ���",
                _ => "ִ��״̬����(����)",
            };
        }

        /// <summary>
        ///     �ƻ���������ת��
        /// </summary>
        public static string ToCaption(this PlanTimeType value)
        {
            return value switch
            {
                PlanTimeType.none => "�޼ƻ�����������",
                PlanTimeType.time => "��ָ����ʱ�䷢��",
                PlanTimeType.second => "��������",
                PlanTimeType.minute => "���Ӽ������",
                PlanTimeType.hour => "Сʱ�������",
                PlanTimeType.day => "�ռ������",
                PlanTimeType.week => "ÿ�ܼ�",
                PlanTimeType.month => "ÿ�¼���",
                _ => "�ƻ�����(����)",
            };
        }



    }
}