using HB.OnlinePsikologMerkezi.Common.CustomEnums;
using HB.OnlinePsikologMerkezi.Data.Context;
using HB.OnlinePsikologMerkezi.Data.Interface;
using HB.OnlinePsikologMerkezi.Entities.Entities;
using Microsoft.EntityFrameworkCore;

namespace HB.OnlinePsikologMerkezi.Web.BackgroundService
{
    public class Control3DFail : IHostedService
    {

        private readonly AppDbContext context;
        private Timer timer;
        public Control3DFail(IServiceScopeFactory factory)
        {
            context = factory.CreateScope().ServiceProvider.GetRequiredService<AppDbContext>();
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            //timer = new Timer(ControlDb,null,0,2300);
           timer = new Timer(ControlDb,null,0,230000);
          
                Console.WriteLine("background task start");
                await Task.Delay(2000);
         }
        private void ControlDb(object? state)
        {

            Console.WriteLine("backgroud task tick");

            var data = 
                context.Set<Appointment>()
                .Where(x => x.Status == (int)AppointmentEnum.s3dCheck)
                .ToList();


            if (data != null)
            {

                if (data.Count > 0)
                {
                    List<Appointment> appList = new();

                    foreach (var item in data)
                    {
                        var tempdate = item.Start3DTime!.Value;
                        if (tempdate.AddMinutes(5) < DateTime.Now)
                        {
                            item.Status = (int)AppointmentEnum.new_appointment;
                            item.CustomerId = null;
                            item.ConversationId = null;
                            item.Start3DTime = null;

                        }

                    }

                    context.SaveChanges();

                    Console.WriteLine("###### SİLME OPERASYONU 3D OTOMATİK TEMİZLEME ######");
                }
            }

        }
        public  Task StopAsync(CancellationToken cancellationToken)
        {

            Console.WriteLine("background task end");
            return Task.CompletedTask;

        }
    }
}
