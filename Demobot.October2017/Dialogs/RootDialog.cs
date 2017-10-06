using System;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Connector;
using System.Threading;
using AdaptiveCards;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using System.Linq;
using Demobot.October2017.Dialogs;

namespace Demobot.October2017.Dialogs
{
    [Serializable]
    public class RootDialog : IDialog<object>
    {
        public Task StartAsync(IDialogContext context)
        {
            context.Wait(MessageReceivedAsync);
            return Task.CompletedTask;
        }

        private const string PlansOption = "Plans";
        private const string CustomerInformationOption = "CustomerInformation";

        public const string MobileOption = "Mobiililaajakaistat";
        public const string BroadbandOption = "Kiinteät laajakaistat";
        public const string OpticalFibreOption = "Talokaapeli";
        public const string PrepaidOption = "Prepaid";

        private IEnumerable<string> _planOptions = new List<string> { MobileOption, BroadbandOption, OpticalFibreOption, PrepaidOption };

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            // IMessage.Value == Value returned by card activity
            if (message.Value != null)
            {
                dynamic value = message.Value;
                //await context.PostAsync($"Card activity returned value: {message.Text} which was {value.length} characters");

                // dynamic Type property provided with JsonData, see constructor 
                string submitType = value.Type.ToString();
                

                switch (submitType)
                {
                    case "Plans":
                        await ShowPlanTypesAsync(context);
                        return;
                    case "PlanSelection":
                        string planType = value.PlanType.ToString();
                        switch (planType)
                        {
                            case MobileOption:
                                await context.Forward(new MobilePlanDialog(), this.ResumeAfterPlansDialog, message, CancellationToken.None);
                                return;
                            default:
                                await context.PostAsync("Ei toteutettu vielä.");
                                context.Wait(MessageReceivedAsync);
                                return;
                        }
                    case "CustomerInformation":
                        await context.PostAsync("Ei toteutettu vielä.");
                        context.Wait(MessageReceivedAsync);
                        return;
                }
            }
            else if (message.Text != null && (message.Text.ToLower().Contains("apua") || message.Text.ToLower().Contains("tuki") || message.Text.ToLower().Contains("ongelma")))
            {
                await context.Forward(new SupportDialog(), this.ResumeAfterSupportDialog, message, CancellationToken.None);
            }
            else
            {
                await ShowOptionsAsync(context);
            }

            //context.Wait(MessageReceivedAsync);
        }

        private async Task ShowPlanTypesAsync(IDialogContext context)
        {

            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                    new TextBlock()
                    {
                        Text = "Valitse liittymätyyppi",
                        Speak = $"<s>Valitse liittymätyyppi</s>",
                        Size = TextSize.Medium
                    }
                },
                // Buttons
                Actions = new List<ActionBase>(_planOptions.Select(AsActionItem))
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

        private ActionBase AsActionItem(string plantype)
        {
            var submitActionData = JObject.Parse("{ \"Type\": \"PlanSelection\" }");
            submitActionData.Add("PlanType", plantype);

            return new SubmitAction
            {
                Title = plantype,
                Speak = $"<s>{plantype}</s>",
                DataJson = submitActionData.ToString()
            };

        }

        private async Task ShowOptionsAsync(IDialogContext context)
        {
            AdaptiveCard card = new AdaptiveCard()
            {
                Body = new List<CardElement>()
                {
                    new Container()
                    {
                        Speak = "<s>Hei!</s><s>Etsitkö uutta liittymää, vai haluatko muuttaa henkilötietojasi?</s>",
                        Items = new List<CardElement>()
                        {
                            new ColumnSet()
                            {
                                Columns = new List<Column>()
                                {
                                    new Column()
                                    {
                                        Size = ColumnSize.Auto,
                                        Items = new List<CardElement>()
                                        {
                                            new Image()
                                            {
                                                Url = "http://res.cloudinary.com/dhppflyb7/image/upload/v1506944518/elisa-robert_btf193.png",
                                                Size = ImageSize.Stretch,
                                                Style = ImageStyle.Person,
                                                
                                               
                                            }
                                        }
                                    },
                                    new Column()
                                    {
                                        Size = ColumnSize.Stretch,
                                        Items = new List<CardElement>()
                                        {
                                            new TextBlock()
                                            {
                                                Text =  "Hei!",
                                                Weight = TextWeight.Bolder,
                                                IsSubtle = true
                                            },
                                            new TextBlock()
                                            {
                                                Text = "Etsitkö uutta liittymää, vai haluatko muuttaa henkilötietojasi?",
                                                Wrap = true
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                },
                // Buttons
                Actions = new List<ActionBase>() {
                    new SubmitAction
                    {
                        Title = "Liittymät",
                        Speak = "<s>Liittymät</s>",
                        DataJson = "{ \"Type\": \"Plans\" }"
                    },
                    new ShowCardAction()
                    {
                        Title = "Henkilötiedot",
                        Speak = "<s>Henkilötiedot</s>",
                        Card = new AdaptiveCard()
                        {
                            Body = new List<CardElement>()
                            {
                                new TextBlock()
                                {
                                    Text = "Henkilötietoja ei ole toteutettu =(",
                                    Speak = "<s>Henkilötietoja ei ole toteutettu</s>",
                                    Weight = TextWeight.Bolder
                                }
                            }
                        }
                    }
                }
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

        //private void PickPlanType(IDialogContext context)
        //{
        //    // Result count
        //    var title = "Valitse liittymätyyppi";
        //    PromptDialog.Choice(context, this.OnPlanTypeSelected, new List<string>() { MobileOption, BroadbandOption, OpticalFibreOption, PrepaidOption }, title, "Valitse jokin liittymävaihtoehdoista");
        //}

        //private async Task OnPlanTypeSelected(IDialogContext context, IAwaitable<string> result)
        //{
        //    try
        //    {
        //        string optionSelected = await result;

        //        switch (optionSelected)
        //        {
        //            case MobileOption:
        //                await context.Forward(new MobilePlanDialog(), ResumeAfterMobilePlanDialog, null, CancellationToken.None);
        //                break;
        //            default:
        //                await context.PostAsync($"Demo: {optionSelected} ei toteutettu.");
        //                break;
        //        }
        //    }
        //    catch (TooManyAttemptsException ex)
        //    {
        //        await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");
        //    }
        //    context.Wait(this.MessageReceivedAsync);
        //}

        private async Task ResumeAfterMobilePlanDialog(IDialogContext context, IAwaitable<object> result)
        {
            //await context.PostAsync($"MobilePlanDialog completed.");
            //context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterOptionDialog(IDialogContext context, IAwaitable<object> result)
        {
            // context.Wait(this.MessageReceivedAsync);
        }

        private async Task ResumeAfterPlansDialog(IDialogContext context, IAwaitable<object> result)
        {
            // context.Wait(this.MessageReceivedAsync);
        }


        private async Task ResumeAfterSupportDialog(IDialogContext context, IAwaitable<int> result)
        {
            var ticketNumber = await result;

            await context.PostAsync($"Kiitos, kun otit yhteyttä tukitiimiimme. Tikettinumerosi on: {ticketNumber}.");
            context.Wait(this.MessageReceivedAsync);
        }

    }
}