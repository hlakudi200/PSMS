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
    Task MarkAsReadAsync(Guid id);
    Task DeleteAsync(Guid id);
}
