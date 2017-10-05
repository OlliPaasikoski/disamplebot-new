namespace Demobot.October2017.Dialogs
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.Bot.Builder.Dialogs;
    using Microsoft.Bot.Connector;

    [Serializable]
    public class SupportDialog : IDialog<int>
    {
        public async Task StartAsync(IDialogContext context)
        {
            context.Wait(this.MessageReceivedAsync);
        }

        public virtual async Task MessageReceivedAsync(IDialogContext context, IAwaitable<IMessageActivity> result)
        {
            var message = await result;

            var ticketNumber = new Random().Next(0, 20000);

            await context.PostAsync($"Viestisi'{message.Text}' on rekisteröity. Palaamme asiaan pikimmiten.");

            context.Done(ticketNumber);
        }
    }
}

