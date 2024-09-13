using Microsoft.AspNetCore.Http;
using SocialMediaApp.Core.Domain.Entites;
using SocialMediaApp.Core.DTO.FilesTweetDTO;
using SocialMediaApp.Core.Helper;
using SocialMediaApp.Core.IUnitOfWorkConfig;
using SocialMediaApp.Core.ServicesContract;
using System.IO;

public class TweetFilesServices : ITweetFilesServices
{
    private readonly IFileServices _fileServices;
    private readonly IUnitOfWork _unitOfWork;

    public TweetFilesServices(IFileServices fileServices, IUnitOfWork unitOfWork)
    {
        _fileServices = fileServices;
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> DeleteTweetFileAsync(IEnumerable<TweetFiles> files)
    {
        var fileDeletionTasks = files.Select(x => _fileServices.DeleteFile(Path.GetFileName(x.FileURL)));
        await Task.WhenAll(fileDeletionTasks);
        await _unitOfWork.Repository<TweetFiles>().RemoveRangeAsync(files);
        return true;
    }

    public async Task<IEnumerable<TweetFiles>> SaveTweetFileAsync(FileTweetAddRequest? fileTweetAdd)
    {
        if (fileTweetAdd == null)
        {
            throw new ArgumentNullException(nameof(fileTweetAdd), "FileTweetAddRequest cannot be null");
        }

        ValidationHelper.ValidateModel(fileTweetAdd);

        if (fileTweetAdd.formFiles == null || !fileTweetAdd.formFiles.Any())
        {
            throw new ArgumentException("No files to upload", nameof(fileTweetAdd.formFiles));
        }

        var tweetFiles = new List<TweetFiles>();
        var tweetID = fileTweetAdd.TweetID;

        var tweet = await _unitOfWork.Repository<Tweet>().GetByAsync(x => x.TweetID == tweetID, isTracked: false);

        if (tweet == null)
        {
            throw new InvalidOperationException("Tweet not found.");
        }

        foreach (var file in fileTweetAdd.formFiles)
        {
            var tweetFile = new TweetFiles
            {
                TweetFilesID = Guid.NewGuid(),
                FileURL = await _fileServices.CreateFile(file), 
                TweetID = tweetID
            };

            tweetFiles.Add(tweetFile);
        }

        await _unitOfWork.Repository<TweetFiles>().AddRangeAsync(tweetFiles);
        await _unitOfWork.CompleteAsync();

        return tweetFiles;
    }
}
