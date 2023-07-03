using Chat.DL.DbContexts;
using Chat.DL.Models;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Chat.BL.Helper
{
    public class ChatHub : Hub
    {

        private readonly ChatDbContext _context;

        public ChatHub(ChatDbContext context)
        {
            _context = context;
        }

        public async Task SendMessage(string receiver, string chat)
        {
            var sender = Context.User.Identity.Name;
            var timestamp = DateTime.UtcNow;

            // Save the message to the database
            var message = new Message
            {
                SenderId = sender,
                ReceiverId = receiver,
                Timestamp = timestamp,
                Content = chat
            };
            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            // Send the message to the receiver
            await Clients.User(receiver).SendAsync("ReceiveMessage",
                new Message
                {
                    ReceiverId = receiver,
                    Content = chat,
                    Timestamp = timestamp
                });
        }
    }

    //public ChatHub(ChatDbContext context)
    //{
    //    _context = context;
    //}

    //public async Task SendMessage(string receiver, string chat)
    //{
    //    var sender = Context.User.Identity.Name;
    //    var timestamp = DateTime.UtcNow;

    //    // Get the sender user from the database
    //    var senderUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == sender);

    //    // Get the receiver user from the database
    //    var receiverUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == receiver);

    //    // Save the message to the database
    //    var message = new Message
    //    {
    //        Sender = senderUser,
    //        Receiver = receiverUser,
    //        Timestamp = timestamp,
    //        Content = chat
    //    };
    //    _context.Messages.Add(message);
    //    await _context.SaveChangesAsync();

    //    // Send the message to the receiver
    //    await Clients.User(receiver).SendAsync("ReceiveMessage",
    //        new
    //        {
    //            Receiver = receiver,
    //            Content = chat,
    //            Timestamp = timestamp
    //        });
    //}
}


    
