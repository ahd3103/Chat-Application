﻿using Chat.BL.Helper;
using Chat.DL.Models;
using Chat.DL.Repostiory;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.BL.Servies
{
    public class MessageService : IMessageService
    {
        private readonly IRepository<Message> _messageRepo;
        private readonly IHubContext<ChatHub> _chatHubContext;

        public MessageService(IRepository<Message> messageRepo, IHubContext<ChatHub> chatHubContext)
        {
            _messageRepo = messageRepo;
            _chatHubContext = chatHubContext;
        }

        public async Task<Message> GetMessageById(Guid messageId)
        {
            return await _messageRepo.Get(messageId);
        }

        public IEnumerable<Message> GetConversationMessages(string currentUser, string userId, DateTime? before, int count, string sort)
        {
            var query = _messageRepo.GetAll().Where(m =>
                m.SenderId.Trim().ToLower() == currentUser.ToLower() &&
                m.ReceiverId.Trim().ToLower() == userId.ToLower());

            if (before.HasValue)
            {
                query = query.Where(m => m.Timestamp < before.Value);
            }

            if (sort.ToLower() == "desc")
            {
                query = query.OrderByDescending(m => m.Timestamp);
            }
            else
            {
                query = query.OrderBy(m => m.Timestamp);
            }

            return query.Take(count).ToList();
        }

        public async Task<Message> CreateMessage(Message message)
        {
            await _messageRepo.Insert(message);
            _messageRepo.SaveChanges();

            // Send the message to the receiver via SignalR
            await _chatHubContext.Clients.User(message.ReceiverId).SendAsync("ReceiveMessage", message);

            return message;
        }

        public async Task<bool> UpdateMessage(Message message)
        {
            await _messageRepo.Update(message);
            _messageRepo.SaveChanges();
            return true;
        }

        public async Task<bool> DeleteMessage(Guid messageId)
        {
            var message = await _messageRepo.Get(messageId);
            if (message == null)
                return false;

            await _messageRepo.Remove(message);
            _messageRepo.SaveChanges();
            return true;
        }

    }

}
