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

        public async Task CreateSubForumAsync(CreateSubForumCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subForum = new SubForum(command.Name, command.Description);

                await _subForumRepository.AddSubForumAsync(subForum);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task UpdateSubForumAsync(UpdateSubForumCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subForum = await _subForumRepository.GetSubForumByIdAsync(command.Id);
                if (subForum == null)
                {
                    throw new Exception("SubForum not found");
                }
                if (!subForum.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("The subforum has been modified by someone else. Please refresh and try again.");
                }

                subForum.UpdateName(command.Name);
                subForum.UpdateDescription(command.Description);

                await _subForumRepository.UpdateSubForumAsync(subForum);
                await _unitOfWork.CommitAsync();
            }
            catch
            {
                await _unitOfWork.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteSubForumAsync(DeleteSubForumCommandDTO command)
        {
            await _unitOfWork.BeginTransactionAsync();
            try
            {
                var subForum = await _subForumRepository.GetSubForumByIdAsync(command.Id);
                if (subForum == null)
                {
                    throw new Exception("SubForum not found");
                }
                if (!subForum.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("The subforum has been modified by someone else. Please refresh and try again.");
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

