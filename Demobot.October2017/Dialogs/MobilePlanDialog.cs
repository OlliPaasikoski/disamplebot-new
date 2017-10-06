
namespace Demobot.October2017.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;
    using Microsoft.Bot.Builder.FormFlow;
    using AdaptiveCards;
    using System.Collections.Generic;
    using Newtonsoft.Json.Linq;
    using System.Linq;
    using Model;
    using System.Threading;

    [Serializable]
    public class MobilePlanDialog : IDialog<object> { 
        private const string MobileOption = "Päivittäin";
        private const string BroadbandOption = "Kiinteät laajakaistat";

        public async Task StartAsync(IDialogContext context)
        {
            //var id = (string)context.ConversationData.GetValue<string>("id");
            //await context.PostAsync($"{id}");
            //context.Activity.Conversation.Id = "newid";

            var message = context.Activity as IMessageActivity;
            dynamic value = message.Value;

            if (value.FrequencyOfUsage != null && value.MobilePlan == null)
            {
                await PickPerformance(context, value);
            }
            else if (value.FrequencyOfUsage == null)
            {
                await PickFrequency(context);
            }
            else
            {
                await context.PostAsync($"Kiitos, tilaus lähetetty.");
                context.Done(value);
            }

            //context.Wait(this.MessageReceivedAsync);
        }

        //public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        //{
        //    try
        //    {
        //        var r = await result;
        //        //var wait = await result;
        //        var message = context.Activity as IMessageActivity;
        //        dynamic value = message.Value;

        //        if (value.FrequencyOfUsage != null && value.MobilePlan == null)
        //        {
        //            await PickPerformance(context, value);
        //            context.Wait(this.MessageReceivedAsync);
        //        }
        //        else
        //        {
        //            
        //            
        //        }
                
        //    }
        //    catch (Exception ex)
        //    {
        //        context.Fail(ex);
        //    }


        //}

        private async Task PickFrequency(IDialogContext context)
        { 
            var title = $"Käytätkö nettiä päivittäin vai silloin tällöin?";
            var frequencies = new List<FrequencyOfUsage>
            {
                new FrequencyOfUsage
                {
                    Name ="Kuukausimaksu",
                    Description = "Käytän mobiilinettiä lähes päivittäin",
                    Image = "http://res.cloudinary.com/dhppflyb7/image/upload/v1507039324/daily_ralbrz.png"
                },
                new FrequencyOfUsage
                {
                    Name ="Vuorokausimaksu",
                    Description = "Tarvitsen mobiilinettiä alle 3 päivänä viikossa",
                    Image = "http://res.cloudinary.com/dhppflyb7/image/upload/v1507039326/occasionally_y5g2vu.png"
                }
            };

            List<Column> frequencyColumns = GetFrequenciesAsColumns(frequencies).ToList();

            var elements = new List<CardElement>()
            {
                    new TextBlock()
                    {
                        Text = title,
                        Size = TextSize.Large,
                        Speak = $"<s>{title}</s>",
                        Wrap = true
                    },
                    new ColumnSet() 
                    {
                        Columns = frequencyColumns
                    }
            };

            var card = new AdaptiveCard()
            {
                Body = elements
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);

            await context.PostAsync(reply, CancellationToken.None);          
        }

        private IEnumerable<Column> GetFrequenciesAsColumns(IEnumerable<FrequencyOfUsage> frequencies)
        {
            var columns = new List<Column>();


            foreach (FrequencyOfUsage f in frequencies)
            {
                //JObject copy = (JObject) submitActionData.DeepClone();
                //copy.Add("FrequencyOfUsage", JObject.FromObject(f));
                JObject submitActionData = JObject.Parse("{ \"Type\": \"PlanSelection\" }");
                submitActionData.Add("PlanType", PlansDialog.MobileOption);
                submitActionData.Add("FrequencyOfUsage", JObject.FromObject(f));
                Column column = AsFrequencyColumnItem(f);
                column.SelectAction = new SubmitAction()
                {
                    DataJson = submitActionData.ToString()
                };
                columns.Add(column);
            }
            return columns;
        }

        private async Task PickPerformance(IDialogContext context, JObject payload)
        {
            var title = $"Kuinka nopean netin tarvitset?";
            var availablePlans  = GetMobilePlans();

            List<Column> performanceColumns = GetPlansAsColumns(availablePlans, payload).ToList();

            var elements = new List<CardElement>()
            {
                    new TextBlock()
                    {
                        Text = title,
                        Size = TextSize.Large,
                        Speak = $"<s>{title}</s>",
                        Wrap = true
                    },
                    new ColumnSet()
                    {
                        Columns = performanceColumns
                    }
            };

            // foreach)

            var card = new AdaptiveCard()
            {
                Body = elements
            };

            Attachment attachment = new Attachment()
            {
                ContentType = AdaptiveCard.ContentType,
                Content = card
            };

            var reply = context.MakeMessage();
            reply.Attachments.Add(attachment);

            await context.PostAsync(reply, CancellationToken.None);       
        }

        private IEnumerable<Column> GetPlansAsColumns(IEnumerable<MobilePlan> plans, JObject payload)
        {
            var columns = new List<Column>();

            foreach (MobilePlan p in plans)
            {
                JObject copy = (JObject) payload.DeepClone();
                //JObject submitActionData = JObject.Parse("{ \"Type\": \"Plans\" }");
                //submitActionData.Add("PlanType", PlansDialog.MobileOption);
                copy.Add("MobilePlan", JObject.FromObject(p));
                Column column = AsPerfomanceColumnItem(p);
                column.SelectAction = new SubmitAction()
                {
                    DataJson = copy.ToString()
                };
                columns.Add(column);
            }
            return columns;
        }



        private Column AsFrequencyColumnItem(FrequencyOfUsage frequency)
        {
            return new Column()
            {
                Size = "20",
                Items = new List<CardElement>()
                {
                    new TextBlock()
                    {
                        Text = frequency.Name,
                        Speak = $"<s>{frequency.Name}</s>",
                        Size = TextSize.Medium
                    },
                    new Image()
                    {
                        Size = ImageSize.Large,
                        Url = frequency.Image
                    },
                    new TextBlock()
                    {
                        Text = frequency.Description,
                        Speak = $"<s>{frequency.Description}</s>",
                        Size = TextSize.Small,
                        Wrap = true,
                        MaxLines = 2
                    }
                }
            };
        }

        private Column AsPerfomanceColumnItem(MobilePlan plan)
        {          
            return new Column()
            {
                Size = "20",
                Items = new List<CardElement>()
                {
                    new TextBlock()
                    {
                        Text = plan.Name,
                        Speak = $"<s>{plan.Name}</s>",
                        Size = TextSize.Small
                    },
                    new Image()
                    {
                        Size = ImageSize.Medium,
                        Url = plan.Image
                    },
                    new TextBlock()
                    {
                        Text = $"{plan.SpeedMbSec} Mbit/s",
                        Speak = $"<s>{plan.SpeedMbSec} Mbit/s</s>",
                        Size = TextSize.Small
                    }
                }
            };
        }


        private IEnumerable<MobilePlan> GetMobilePlans()
        {
            return new List<MobilePlan>
            {
                new MobilePlan
                {
                    Name = "4G",
                    SpeedMbSec = 50m,
                    Price = 17.90m,
                    Image = "http://res.cloudinary.com/dhppflyb7/image/upload/v1507201385/4G_prugoc.png"
                },
                new MobilePlan
                {
                    Name = "4G Super",
                    SpeedMbSec = 100m,
                    Price = 24.90m,
                    Image = "http://res.cloudinary.com/dhppflyb7/image/upload/v1507201385/4GSuper_asct6c.png"
                },
                new MobilePlan
                {
                    Name = "4G Super+",
                    SpeedMbSec = 300m,
                    Price = 29.90m,
                    Image = "http://res.cloudinary.com/dhppflyb7/image/upload/v1507201385/4GSuper_qqz9ll.png"
                },
                new MobilePlan
                {
                    Name = "Mini",
                    SpeedMbSec = 0.12m,
                    Price = 3.90m,
                    Image = "http://res.cloudinary.com/dhppflyb7/image/upload/v1507201385/Mini_zfvqky.png"
                }
            };
        }

    }
}
