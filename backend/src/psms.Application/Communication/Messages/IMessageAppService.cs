using Abp.Application.Services;
using Abp.Application.Services.Dto;
using psms.Communication.Messages.Dto;
using System;
using System.Threading.Tasks;

namespace psms.Communication.Messages;

public interface IMessageAppService : IApplicationService
{
    Task<MessageDto> GetAsync(Guid id);
    Task<PagedResultDto<MessageListDto>> GetInboxAsync(GetMessagesInput input);
    Task<PagedResultDto<MessageListDto>> GetSentAsync(GetMessagesInput input);
    Task<ListResultDto<MessageDto>> GetThreadAsync(Guid threadId);
    Task<MessageDto> SendAsync(CreateMessageDto input);

    /// <summary>
    /// #204. The people the caller may start a conversation with. For a parent,
    /// their children's teachers. Empty for anyone else: staff choose
    /// recipients on their own screens.
    /// </summary>
    Task<ListResultDto<MessageRecipientDto>> GetMyRecipientsAsync();
    Task MarkAsReadAsync(Guid id);
    Task DeleteAsync(Guid id);
}
