using Chat.DL.Models;  

namespace Chat.BL.Servies
{
    public interface IMessageService
    {
       
        Task<Message> GetMessageById(Guid messageId);
        Task<Message> CreateMessage(Message message);
        Task<bool> UpdateMessage(Message message);
        Task<bool> DeleteMessage(Guid messageId);
        IEnumerable<Message> GetConversationMessages(string currentUser, string userId, DateTime? before, int count, string sort);
    }
}
