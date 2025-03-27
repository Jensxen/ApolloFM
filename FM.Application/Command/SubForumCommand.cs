using FM.Application.Interfaces.ICommand;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Interfaces;
using FM.Application.Command.CommandDTO.SubForumCommandDTO;
using FM.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace FM.Application.Command
{
    public class SubForumCommand : ISubForumCommand
    {
        private readonly ISubForumRepository _subForumRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SubForumCommand(ISubForumRepository subForumRepository, IUnitOfWork unitOfWork)
        {
            _subForumRepository = subForumRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task CreateSubForum(CreateSubForumCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subForum = new SubForum
                {
                    Name = command.Name,
                    Description = command.Description,
                };

                await _subForumRepository.AddSubForumAsync(subForum);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateSubForum(UpdateSubForumCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subForum = await _subForumRepository.GetSubForumByIdAsync(command.Id);
                if (subForum == null)
                {
                    throw new Exception("SubForum not found");
                }

                subForum.Name = command.Name;
                subForum.Description = command.Description;

                await _subForumRepository.UpdateSubForumAsync(subForum);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteSubForum(DeleteSubForumCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subForum = await _subForumRepository.GetSubForumByIdAsync(command.Id);
                if (subForum == null)
                {
                    throw new Exception("SubForum not found");
                }

                await _subForumRepository.DeleteSubForumAsync(command.Id);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }
    }
}

