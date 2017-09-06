using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PushSharp.Apple;

namespace VoIPDemo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"PushVoIP");
            var certificate = AppDomain.CurrentDomain.BaseDirectory + @"you certificate.p12";
            ApnsConfiguration config = new ApnsConfiguration(ApnsConfiguration.ApnsServerEnvironment.Sandbox, File.ReadAllBytes(certificate), "you certificate password", false);
            var apnsBroker = new ApnsServiceBroker(config);
            //推送异常
            apnsBroker.OnNotificationFailed += (notification, aggregateEx) =>
            {
                aggregateEx.Handle(ex =>
                {
                    //判断例外，进行诊断
                    if (ex is ApnsNotificationException)
                    {
                        var notificationException = (ApnsNotificationException)ex;
                        //处理失败的通知 
                        var apnsNotification = notificationException.Notification;
                        var statusCode = notificationException.ErrorStatusCode;
                        Console.WriteLine(
                            $"Apple Notification Failed: ID={apnsNotification.Identifier}, Code={statusCode}" +
                            notification.DeviceToken);
                    }
                    else
                    {
                        Console.WriteLine($"pple Notification Failed for some unknown reason : {ex}------{notification.DeviceToken}");
                    }
                    // 标记为处理
                    return true;
                });
            };
            JObject obj = new JObject();
            obj.Add("content-availabl", 1);
            obj.Add("badge", 1);
            obj.Add("alert", "apple voip test !");
            obj.Add("sound", "default");
            var voIpToken = "you voip token";
            var payload = new JObject();
            payload.Add("aps", obj);
            var apns = new ApnsNotification()
            {
                DeviceToken = voIpToken,
                Payload = payload
            };
            //推送成功
            apnsBroker.OnNotificationSucceeded += (notification) =>
            {

                apnsBroker.QueueNotification(apns);
                Console.WriteLine("Apple Notification Sent ! " + notification.DeviceToken);
            };
            //启动代理
            apnsBroker.Start();
            apnsBroker.QueueNotification(apns);

            Console.ReadKey();
        }
    }
}
