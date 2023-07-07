using Chat.BL.Helper;
using Chat.BL.Servies;
using Chat.DL.DbContexts;
using Chat.DL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace Chat.PL.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {

        private readonly IHubContext<ChatHub> _chatHub;
        private readonly ChatDbContext _context;
        private readonly IUserService _userRepository;
        private readonly IMessageService _messageRepository;

        public MessageController(IHubContext<ChatHub> chatHub, ChatDbContext context, IUserService userRepository, IMessageService messageRepository)
        {
            _chatHub = chatHub;
            _context = context;
            _userRepository = userRepository;
            _messageRepository = messageRepository;
        }

        [Authorize]
        [HttpPost("sendMessage")]
        public async Task<IActionResult> SendMessage(string receiver, string message)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return Unauthorized();
            }

            var sender = User.Identity.Name;
            var timestamp = DateTime.UtcNow;

            var user = await _userRepository.Get(new Guid(sender));

            // Validation logic for receiver and message
            if (string.IsNullOrEmpty(receiver) || string.IsNullOrEmpty(message))
            {
                return BadRequest();
            }

            try
            {
                // Save the message to the database
                var chatMessage = new Message
                {
                    SenderId = sender,
                    ReceiverId = receiver,
                    Timestamp = timestamp,
                    Content = message,
                    User = user

                };

                var createdMessage = await _messageRepository.CreateMessage(chatMessage);
                chatMessage.User = null;

                //Send the message to the receiver
                 await _chatHub.Clients.All.SendAsync("ReceiveMessage", createdMessage);

                return Ok(createdMessage);
            }
            catch (Exception ex)
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpPut("messages/{messageId}")]
        public async Task<IActionResult> EditMessage(string messageId, [FromBody] string content)
        {
            var sender = User.Identity.Name;

            // Convert the messageId string to Guid
            if (!Guid.TryParse(messageId, out Guid messageIdGuid))
            {
                return BadRequest(new { error = "Invalid messageId" });
            }

            //Get the message from the database
            var message = await _messageRepository.GetMessageById(messageIdGuid);
            if (message == null)
            {
                return NotFound(new { error = "Message not found" });
            }

            // Check if the message sender is the current user
            if (message.SenderId != sender)
            {
                return Unauthorized();
            }

            // Update the message content
            message.Content = content;
            var isUpdated = await _messageRepository.UpdateMessage(message);

            if (isUpdated)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }

        [Authorize]
        [HttpDelete("messages/{messageId}")]
        public async Task<IActionResult> DeleteMessage(string messageId)
        {
            var sender = User.Identity.Name;

            //Convert the messageId string to Guid
            if (!Guid.TryParse(messageId, out Guid messageIdGuid))
            {
                return BadRequest(new { error = "Invalid messageId" });
            }

            // Get the message from the database
            var message = await _messageRepository.GetMessageById(messageIdGuid);
            if (message == null)
            {
                return NotFound(new { error = "Message not found" });
            }

            //Check if the message sender is the current user
            if (message.SenderId != sender)
            {
                return Unauthorized();
            }

            // Delete the message
            var isDeleted = await _messageRepository.DeleteMessage(messageIdGuid);

            if (isDeleted)
            {
                return Ok();
            }
            else
            {
                return BadRequest();
            }
        }


        [Authorize]
        [HttpGet("conversations/{userId}")]
        public async Task<IActionResult> GetConversation(string userId, DateTime? before = null, int count = 20, string sort = "asc")
        {
            try
            {
                var currentUser = User.Identity.Name?.Trim();

                // Check if the current user has access to retrieve the conversation
                if (!string.Equals(currentUser, userId, StringComparison.OrdinalIgnoreCase))
                {
                    return Unauthorized();
                }

                // Get the conversation messages from the repository
                var messages = _messageRepository.GetConversationMessages(userId, before, count, sort);

                // Prepare the response body
                var response = new
                {
                    messages = messages.Select(m => new
                    {
                        id = m.Id.ToString(),
                        senderId = m.SenderId,
                        receiverId = m.ReceiverId,
                        content = m.Content,
                        timestamp = m.Timestamp
                    })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> SearchConversations(string query)
        {
            try
            {
                var currentUser = User.Identity.Name?.Trim();

                // Search conversations for messages containing the query
                var messages = await _messageRepository.SearchConversations(currentUser, query);

                // Prepare the response body
                var response = new
                {
                    messages = messages.Select(m => new
                    {
                        id = m.Id.ToString(),
                        senderId = m.SenderId,
                        receiverId = m.ReceiverId,
                        content = m.Content,
                        timestamp = m.Timestamp
                    })
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }

        }
    }

}

