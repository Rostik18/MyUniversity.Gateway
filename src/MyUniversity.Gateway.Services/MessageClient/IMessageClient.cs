using System.Threading.Tasks;

namespace MyUniversity.Gateway.Services.MessageClient
{
    public interface IMessageClient
    {
        Task<TRespond> RequestAsync<TRequest, TRespond>(string requestQueue, TRequest body);
    }
}
