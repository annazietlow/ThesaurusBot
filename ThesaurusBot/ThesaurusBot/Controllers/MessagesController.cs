using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Services.Description;
using Microsoft.Bot.Connector;
using Newtonsoft.Json;

namespace ThesaurusBot
{
    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var query = ExtractSingleWord(activity.Text);
                //                // calculate something for us to return
                //                int length = (activity.Text ?? string.Empty).Length;
                //
                //                // return our reply to the user
                //                Activity reply = activity.CreateReply($"You sent {activity.Text} which was {length} characters");

                var replyMessage = await GetSynonyms(query);
                var reply = activity.CreateReply(replyMessage);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                HandleSystemMessage(activity);
            }
            var response = Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }

        private async Task<string> GetSynonyms(string word)
        {
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Add("X-Mashape-Key", "vJD1NrdV8ZmshD9sN9KzxEbQRNvDp1L1C7ljsnherlwsVV7JAF");
                var response = await client.GetAsync($"https://wordsapiv1.p.mashape.com/words/{word}/synonyms").ConfigureAwait(false);
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    return $"I'm sorry, '{word}' did not get a generate a proper response. Please try another word.";
                }

                var wordresponse = await response.Content.ReadAsAsync<WordResponse>().ConfigureAwait(false);

                if (wordresponse.Synonyms.Length > 0)
                {
                    var messageResponse = "I have found the following synonyms:";

                    //TODO: figure out how to do all this w/o silly string manipulation
                    foreach (var synonym in wordresponse.Synonyms)
                    {
                        messageResponse = $"{messageResponse} {synonym},";
                    }
                    if (messageResponse.EndsWith(","))
                    {
                        messageResponse = messageResponse.Substring(0, messageResponse.Length - 1);
                    }
                    return messageResponse;
                }
                return $"I'm sorry, I could not find any synonyms for {word}.";
            }
        }

        private static string ExtractSingleWord(string text)
        {
            return text.IndexOf(" ", StringComparison.Ordinal) > 0 ? text.Substring(0, text.IndexOf(" ", StringComparison.Ordinal)) : text;
        }
    }
}