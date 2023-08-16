using Chat.DL.Models;

namespace Chat.BL.Servies
{
    public interface IMessageService
    {

        Task<Message> GetMessageById(Guid messageId);
        Task<Message> CreateMessage(Message message);
        Task<bool> UpdateMessage(Message message);
        Task<bool> DeleteMessage(Guid messageId);
        IEnumerable<Message> GetConversationMessages(string userId, string currentUser, DateTime? before, int count, string sort);
        //IEnumerable<Message> GetConversationMessages(string userId, DateTime? before, int count, string sort);
        Task<List<Message>> SearchConversations(string currentUser, string query);
    }
}

