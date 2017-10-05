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
    using System.Threading;

    [Serializable]
    public class PlansDialog : IDialog
    {
        public const string MobileOption = "Mobiililaajakaistat";
        public const string BroadbandOption = "Kiinteät laajakaistat";
        public const string OpticalFibreOption = "Talokaapeli";
        public const string PrepaidOption = "Prepaid";

        public async Task StartAsync(IDialogContext context)
        {
            //var message = context.Activity as IMessageActivity;

            //// should never be null
            //if (message.Value != null)
            //{
            //    dynamic value = message.Value;

            //    if (value.PlanType != null)
            //    {
            //        string planType = value.PlanType.ToString();
            //        await context.PostAsync($"Liittymätyyppi: {planType}");
            //        switch (planType)
            //        {
            //            case MobileOption:
            //                await context.Forward(new MobilePlanDialog(), this.ResumeAfterMobilePlanDialog, message, CancellationToken.None);
            //                return;
            //            case BroadbandOption:
            //                await context.PostAsync("Ei toteutettu vielä.");
            //                return;
            //        }
            //    }
            //    else
            //    {
            //        try
            //        {
            //            PickPlanType(context);
            //        }
            //        catch (FormCanceledException ex)
            //        {
            //            context.Fail(new Exception(($"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}")));
            //        }
            //    }
            //}   
            try
            {
                PickPlanType(context);
            }
            catch (FormCanceledException ex)
            {
                context.Fail(new Exception(($"Oops! Something went wrong :( Technical Details: {ex.InnerException.Message}")));
            }
        }

        private void PickPlanType(IDialogContext context)
        {
            // Result count
            var title = "Valitse liittymätyyppi";
            PromptDialog.Choice(context, this.OnPlanTypeSelected, new List<string>() { MobileOption, BroadbandOption, OpticalFibreOption, PrepaidOption }, title, "Valitse jokin liittymävaihtoehdoista");
        }

        private async Task OnPlanTypeSelected(IDialogContext context, IAwaitable<string> result)
        {
            try
            {
                string optionSelected = await result;

                switch (optionSelected)
                {
                    case MobileOption:
                        //await context.Forward(new MobilePlanDialog(), this.ResumeAfterMobilePlanDialog, result, CancellationToken.None);
                        break;
                    default:
                        await context.PostAsync($"Demo: {optionSelected} ei toteutettu.");
                        break;  
                }
            }
            catch (TooManyAttemptsException ex)
            {
                await context.PostAsync($"Ooops! Too many attemps :(. But don't worry, I'm handling that exception and you can try again!");
            }
        }

        private Task ResumeAfterMobilePlanDialog(IDialogContext context, IAwaitable<object> result)
        {
            //await context.PostAsync($"MobilePlanDialog completed.");
            context.Done(result);
            return Task.CompletedTask;
        }

    }
}
