using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.FilesCommentDTO;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.ServicesContract;

namespace SocialMediaApp.Core.Services
{
    public class CommentFilesServices : ICommentFilesServices
    {
        private readonly IFileServices _fileServices;
        private readonly IUnitOfWork _unitOfWork;

        public CommentFilesServices(IFileServices fileServices, IUnitOfWork unitOfWork)
        {
            _fileServices = fileServices;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> DeleteTweetFileAsync(IEnumerable<CommentFiles> files)
        {
            var fileDeletionTasks = files.Select(x => _fileServices.DeleteFile(Path.GetFileName(x.FileUrl)));
            await Task.WhenAll(fileDeletionTasks);
            await _unitOfWork.Repository<CommentFiles>().RemoveRangeAsync(files);
            return true;
        }

        public async Task<IEnumerable<CommentFiles>> SaveTweetFileAsync(FilesCommentAddRequest? fileCommentAdd)
        {
            if (fileCommentAdd == null)
            {
                throw new ArgumentNullException(nameof(fileCommentAdd), "FileCommentAddRequest cannot be null");
            }

            ValidationHelper.ValidateModel(fileCommentAdd);

            if (fileCommentAdd.formFiles == null || !fileCommentAdd.formFiles.Any())
            {
                throw new ArgumentException("No files to upload", nameof(fileCommentAdd.formFiles));
            }

            var commentFiles = new List<CommentFiles>();
            var commentID = fileCommentAdd.CommentID;

            var comment = await _unitOfWork.Repository<Comment>().GetByAsync(x => x.CommentID == fileCommentAdd.CommentID, isTracked: false);

            if (comment == null)
            {
                throw new InvalidOperationException("Comment not found.");
            }

            foreach (var file in fileCommentAdd.formFiles)
            {
                var commentFile = new CommentFiles
                {
                    CommentFileID = Guid.NewGuid(),
                    FileUrl = await _fileServices.CreateFile(file),
                    CommentID = commentID
                };

                commentFiles.Add(commentFile);
            }

            await _unitOfWork.Repository<CommentFiles>().AddRangeAsync(commentFiles);
            await _unitOfWork.CompleteAsync();

            return commentFiles;
        }
    }
}
