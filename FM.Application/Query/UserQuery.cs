﻿using FM.Application.Interfaces.IQuery;
using FM.Application.Interfaces.IRepositories;
using FM.Application.QueryDTO.UserDTO;

namespace FM.Application.Query
{
    public class UserQuery : IUserQuery
    {
        private readonly IUserRepository _userRepository;

        public UserQuery(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        //public async Task<IEnumerable<UserQueryDTO>> GetAllUsersAsync()
        //{
        //    var users = await _userRepository.GetAllUsersAsync();
        //    return users.Select(user => new UserQueryDTO
        //    {
        //        Id = user.Id,
        //        DisplayName = user.DisplayName,
        //        SpotifyUserId = user.SpotifyUserId,
        //        UserRoleId = user.UserRoleId,
        //        RowVersion = user.RowVersion
        //    });
        //}

        public async Task<UserQueryDTO> GetUserByIdAsync(string id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return null;
            }

            return new UserQueryDTO
            {
                Id = user.Id,
                DisplayName = user.DisplayName,
                SpotifyUserId = user.SpotifyUserId,
                UserRoleId = user.UserRoleId,
                RowVersion = user.RowVersion
            };
        }
    }
}
