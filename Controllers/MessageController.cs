using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using Datingapp.API.Data;
using Datingapp.API.Dtos;
using Datingapp.API.Helpers;
using Datingapp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Datingapp.API.Controllers
{
    [ServiceFilter(typeof(LogUserActivity))]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]
    public class MessageController : ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;

        public MessageController(IDatingRepository repo,IMapper mapper)
        {
            _repo = repo;
            _mapper = mapper;
        }

        [HttpGet("{id}",Name ="GetMessage")]
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);
            if (messageFromRepo == null)
                return NotFound();
            return Ok(messageFromRepo);
        }

        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messgeFromRepo = await _repo.GetMessageThread(userId, recipientId);
            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messgeFromRepo);

            return Ok(messageThread);
       }
        [HttpGet]
        public async Task<IActionResult> GetMessagesForUser(int userId,[FromQuery]MessageParams messageParams)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            messageParams.UserId = userId;
            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);

            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);

            Response.AddPaginationHeader(messagesFromRepo.CurrentPage,
                messagesFromRepo.PageSize, messagesFromRepo.TotalPages, messagesFromRepo.TotalCount);

            return Ok(messages);
        }

        
        
        [HttpPost]
        public async Task<IActionResult> CreateMessge (int userId, MessageForCreationDto messageForCreationDto)
        {
            var sender = await _repo.GetUser(userId,false);

            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageForCreationDto.SenderId = userId;

            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId,false);
            if (recipient == null)
                return BadRequest("Could not find user");
            var message = _mapper.Map<Message>(messageForCreationDto);

            _repo.Add<Message>(message);

            if (await _repo.SaveAll())
            {
                var messageForReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { id = message.Id, userId = userId }, messageForReturn);
            }
                

            throw new Exception("Creating the message failed on save");
        }

        [HttpPost("{Id}")]
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _repo.GetMessage(id);
            if(message.SenderId == userId)
               message.SenderDeleted = true;
            
            if(message.RecipientId == userId)
              message.RecipientDeleted = true;

            if (message.SenderDeleted && message.RecipientDeleted)
                _repo.Delete<Message>(message);
            if (await _repo.SaveAll())
                return NoContent();
            throw new Exception("Error deleting the message");
        }
        [HttpPost("{Id}/read")]
        public async Task<IActionResult> MarkMessageAsRead(int userId,int id)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var message = await _repo.GetMessage(id);
            if (message.RecipientId != userId)
                return Unauthorized();
            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _repo.SaveAll();
            return NoContent();
        }
    }
}