using DatingApp.API.Data;
using DatingApp.API.Models;
using DatingAppCore3.API.Helpers;
using DatingAppCore3.API.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DatingAppCore3.API.Data
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
            //throw new NotImplementedException();
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
            //throw new NotImplementedException();
        }

        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u => u.LikerId == userId && u.LikeeId==recipientId);
        }

        public  async Task<Photo> GetMainPhotoForUser(int userId)
        {

            return await _context.Photos.Where(u => u.UserId == userId).FirstOrDefaultAsync(p => p.IsMain);
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.IgnoreQueryFilters().
                FirstOrDefaultAsync(p => p.Id == id);

            return photo;
        }

        public async Task<User> GetUser(int id, bool isCurrentUser)
        {
            //var user = await _context.Users.Include(p => p.Photos)
            //   .FirstOrDefaultAsync(u => u.Id == id);
            //return user;
            var query = _context.Users.Include(p => p.Photos).AsQueryable();
            if (isCurrentUser)          
                query = query.IgnoreQueryFilters();
            
            var user = await _context.Users
               .FirstOrDefaultAsync(u => u.Id == id);
            return user;
            //throw new NotImplementedException();
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            //removing Include because we used lazy loading proxies 
            //var users =  _context.Users.Include(p => p.Photos).OrderByDescending(u => u.LastActive).AsQueryable();
            var users = _context.Users.OrderByDescending(u => u.LastActive).AsQueryable();
            users = users.Where(u => u.Id != userParams.UserId);
            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }
            if (userParams.likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob); 
            }
            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.Created);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;

                }
            }


            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);

            //throw new NotImplementedException();
        }

        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            //removing Include because we used lazy loading proxies 
            //var user = await _context.Users.Include(x => x.Likers).
            //    Include(x => x.Likees).FirstOrDefaultAsync(u => u.Id == id);
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

            if (likers)
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() >0;
            //throw new NotImplementedException();
        }

        public async Task<Message> GetMessage(int id)
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.Id == id);
        }

        public  async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            //Removed Include  because used lazyloading
            //var messages = _context.Messages.Include(u => u.Sender).ThenInclude(p => p.Photos)
            //    .Include(u => u.Recipient).ThenInclude(p => p.Photos).AsQueryable();
            var messages = _context.Messages.AsQueryable();
            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && u.Deleted==false);
                    break;

                case "Outbox":
                    messages = messages.Where(u => u.SenderId == messageParams.UserId && u.SenderDeleted==false);
                    break;
                default:
                    messages = messages.Where(u => u.RecipientId == messageParams.UserId && 
                    u.SenderDeleted==false && u.Deleted==false && u.IsRead == false);
                    break;

            }

            messages = messages.OrderByDescending(d => d.MassageSent);

            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            // removed include as we used lazy loading 
            //var messages =await _context.Messages.Include(u => u.Sender).ThenInclude(p => p.Photos)
            //  .Include(u => u.Recipient).ThenInclude(p => p.Photos)
            //  .Where(m => m.RecipientId == userId && m.SenderId == recipientId && m.Deleted== false
            //  || m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted==false)
            //  .OrderByDescending(m => m.MassageSent)
            //  .ToListAsync();
            var messages = await _context.Messages
              .Where(m => m.RecipientId == userId && m.SenderId == recipientId && m.Deleted == false
              || m.RecipientId == recipientId && m.SenderId == userId && m.SenderDeleted == false)
              .OrderByDescending(m => m.MassageSent)
              .ToListAsync();

            return messages;
        }
    }
}
