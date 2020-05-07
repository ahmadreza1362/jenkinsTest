using Datingapp.API.Helpers;
using Datingapp.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Datingapp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;

        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(d => d.LikerId == userId && d.LikeeId == recipientId);

        }

        public Task<Photo> GetMainPhotoForUser(int userId)
        {
            return _context.Photos.Where(d => d.UserId == userId).FirstOrDefaultAsync(u => u.IsMain);
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var test = _context.Messages;
            var messages =  _context.Messages.AsQueryable();

            switch(messageParams.MessageContainer)
            {
                case "Inbox": 
                    messages = messages.Where(d => d.RecipientId == messageParams.UserId && d.RecipientDeleted ==false);
                    break;
                case "Outbox":
                    messages = messages.Where(d => d.SenderId == messageParams.UserId && d.SenderDeleted ==false);
                    break;
                default:
                    messages = messages.Where(d => d.RecipientId == messageParams.UserId && d.RecipientDeleted ==false && d.IsRead==false);
                    break;

            }
            messages = messages.OrderByDescending(d => d.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            var messages = await _context.Messages
                                           .Where(m=>m.RecipientId==userId && m.RecipientDeleted ==false && m.SenderId== recipientId ||
                                           m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted==false)
                                           .OrderByDescending(d=>d.MessageSent)
                                           .ToListAsync();
            return messages;
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == id);
            return photo;
        }

        public async Task<User> GetUser(int Id,bool isCurrentUser)
        {
            var query = _context.Users.Include(d => d.Photos).AsQueryable();
            if (isCurrentUser)
            {
                query = query.IgnoreQueryFilters();
            }

            var user = await query.FirstOrDefaultAsync(d=>d.Id==Id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users = _context.Users.OrderByDescending(d=>d.LastActive).AsQueryable();
            users = users.Where(d => d.Id != userParams.UserId);
            users = users.Where(d => d.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(d => userLikers.Contains(d.Id));
            }
            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(d => userLikees.Contains(d.Id));
            }


            if (userParams.MinAge!= 18 || userParams.MaxAge!=99)
            {
                var minDoB = DateTime.Now.AddYears(-userParams.MinAge);
                var maxDoB = DateTime.Now.AddYears(-userParams.MaxAge - 1);

                users = users.Where(d => d.DateOfBirth <= minDoB && d.DateOfBirth >= maxDoB);
            }
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(d => d.Created);
                        break;
                    default:
                        users = users.OrderByDescending(d => d.LastActive);
                        break;
                }
            }

            return await PagedList<User>.CreateAsync(users,userParams.PageNumber,userParams.PageSize);
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool liker)
        {
            var user = await _context.Users.FirstOrDefaultAsync(d => d.Id == id);
            if (liker)
            {
                return user.Likers.Where(d => d.LikeeId == id).Select(d => d.LikerId);
            }
            else
                return user.Likees.Where(d => d.LikerId == id).Select(d => d.LikeeId);
  
        }


        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}
