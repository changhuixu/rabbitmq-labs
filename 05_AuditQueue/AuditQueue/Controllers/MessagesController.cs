using System.Collections.Generic;
using System.Threading.Tasks;
using AuditQueue.Models;
using AuditQueue.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace AuditQueue.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class MessagesController : ControllerBase
    {
        private readonly ILogger<MessagesController> _logger;
        private readonly IConnection _rabbitConnection;
        private readonly IMessagesService _messagesService;

        public MessagesController(ILogger<MessagesController> logger, IConnection rabbitConnection, IMessagesService messagesService)
        {
            _logger = logger;
            _rabbitConnection = rabbitConnection;
            _messagesService = messagesService;
        }

        [HttpGet]
        public async Task<List<Message>> List()
        {
            return await _messagesService.GetAll();
        }

        [HttpGet("{messageId}")]
        public async Task<List<Message>> GetListByMessageId(string messageId)
        {
            return await _messagesService.Get(messageId);
        }
    }
}
