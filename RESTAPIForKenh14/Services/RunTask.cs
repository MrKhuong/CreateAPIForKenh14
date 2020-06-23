using Quartz;
using Quartz.Impl;
using System;
using System.Configuration;
using System.Threading.Tasks;

namespace RESTAPIForKenh14.Services
{
    public class RunTask
    {
        private static readonly string SchedulingExpression = ConfigurationManager.AppSettings["SchedulingExpression"];
       
        // Get data fro Kenh14 with schedule
        public static async Task RunProgram()
        {
            try
            {
                var scheduler = await StdSchedulerFactory.GetDefaultScheduler();

                if (!scheduler.IsStarted)
                {
                    await scheduler.Start();
                }

                var job = JobBuilder.Create<CrawlerData>()
                    .WithIdentity("trigger1", "group1")
                    .Build();

                var trigger = TriggerBuilder.Create()
                    .WithIdentity("trigger2", "group1")
                    .WithCronSchedule(SchedulingExpression)
                    .Build();

                await scheduler.ScheduleJob(job, trigger);
            }
            catch (SchedulerException se)
            {
                await Console.Error.WriteLineAsync(se.ToString());
            }
        }
    }
}