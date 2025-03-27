using FM.Application.Interfaces.ICommand;
using FM.Application.Interfaces.IRepositories;
using FM.Application.Interfaces;
using FM.Application.Command.CommandDTO.SubForumCommandDTO;
using FM.Domain.Entities;

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

        void ISubForumCommand.CreateSubForum(CreateSubForumCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var subForum = new SubForum
                {
                    Name = command.Name,
                    Description = command.Description,
                };

                _subForumRepository.AddSubForumAsync(subForum);
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        void ISubForumCommand.UpdateSubForum(UpdateSubForumCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var subForum = _subForumRepository.GetSubForumByIdAsync(command.Id).Result;
                if (subForum == null)
                {
                    throw new Exception("SubForum not found");
                }
                if (!subForum.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("SubForum has been modified by someone else. Please refresh and try again.");
                }

                subForum.Name = command.Name;
                subForum.Description = command.Description;

                _subForumRepository.UpdateSubForumAsync(subForum);
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }

        void ISubForumCommand.DeleteSubForum(DeleteSubForumCommandDTO command)
        {
            _unitOfWork.BeginTransaction();
            try
            {
                var subForum = _subForumRepository.GetSubForumByIdAsync(command.Id).Result;
                if (subForum == null)
                {
                    throw new Exception("SubForum not found");
                }
                if (!subForum.RowVersion.SequenceEqual(command.RowVersion))
                {
                    throw new Exception("SubForum has been modified by someone else. Please refresh and try again.");
                }
                _subForumRepository.DeleteSubForumAsync(command.Id);
                _unitOfWork.Commit();
            }
            catch
            {
                _unitOfWork.Rollback();
                throw;
            }
        }
    }
}
