using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Extensions;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;
using API.Entities;
using API.DTOs;
using API.Helpers;

namespace API.Controllers
{
    public class LikesController : BaseApiController
    {
        private readonly IUserRepository _userRepository;
        private readonly ILikesRepository _likesRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            _likesRepository = likesRepository;
            _userRepository = userRepository;
        }

        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var SourceUserId = User.GetUserId();
            var LikedUser = await _userRepository.GetUserByUsernameAsync(username);
            var SourceUser = await _likesRepository.GetUserWithLikes(SourceUserId);

            if (LikedUser == null) return NotFound();

            if (SourceUser.UserName == username) return BadRequest("you cannot like yourself");

            var UserLike = await _likesRepository.GetUserLike(SourceUserId, LikedUser.Id);

            if (UserLike != null) return BadRequest("you already like this user.");

            UserLike = new UserLike
            {
                SourceUserId = SourceUserId,
                LikedUserId = LikedUser.Id
            };

            SourceUser.LikedUsers.Add(UserLike);

            if (await _userRepository.SaveAllAsync()) return Ok();

            return BadRequest("failed to like user");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDto>>> GetUserLikes([FromQuery]LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await _likesRepository.GetUserLikes(likesParams);

            Response.AddPaginationHeader(users.CurrentPage,users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }



    }
}