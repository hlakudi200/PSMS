using Abp.Application.Services;
using Abp.Application.Services.Dto;
using Abp.Authorization;
using Abp.Domain.Repositories;
using Abp.Linq.Extensions;
using Abp.UI;
using Microsoft.EntityFrameworkCore;
using psms.Authorization;
using psms.Communication.Messages.Dto;
using psms.Communication.Shared;
using psms.Authorization.Users;
using psms.Domain.Communication.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace psms.Communication.Messages;

[AbpAuthorize(PermissionNames.Communication_Messages)]
public class MessageAppService : ApplicationService, IMessageAppService
{
    private readonly IRepository<Message, Guid> _messageRepository;
    private readonly IRepository<User, long> _userRepository;

    public MessageAppService(
        IRepository<Message, Guid> messageRepository,
        IRepository<User, long> userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    [AbpAuthorize(PermissionNames.Communication_Messages_View)]
    public async Task<MessageDto> GetAsync(Guid id)
    {
        var currentUserId = AbpSession.UserId.Value;

        var message = await _messageRepository
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (message == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.MessageNotFound,
                "Message not found.");

        if (!message.IsVisibleToUser(currentUserId))
            throw new UserFriendlyException(CommunicationExceptionCodes.MessageNotVisible,
                "You do not have access to this message.");

        return ObjectMapper.Map<MessageDto>(message);
    }

    [AbpAuthorize(PermissionNames.Communication_Messages_View)]
    public async Task<PagedResultDto<MessageListDto>> GetInboxAsync(GetMessagesInput input)
    {
        var currentUserId = AbpSession.UserId.Value;

        var query = _messageRepository
            .GetAll()
            .Where(m => m.TenantId == AbpSession.TenantId
                && m.RecipientUserId == currentUserId
                && !m.IsDeletedByRecipient)
            .WhereIf(input.IsRead.HasValue, m => m.IsRead == input.IsRead.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                m => (m.Subject != null && m.Subject.ToLower().Contains(input.Search.Trim().ToLower()))
                    || m.Content.ToLower().Contains(input.Search.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                m => (m.Subject != null && m.Subject.ToLower().Contains(input.Keyword.ToLower()))
                    || m.Content.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<MessageListDto>(
            totalCount,
            ObjectMapper.Map<List<MessageListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Communication_Messages_View)]
    public async Task<PagedResultDto<MessageListDto>> GetSentAsync(GetMessagesInput input)
    {
        var currentUserId = AbpSession.UserId.Value;

        var query = _messageRepository
            .GetAll()
            .Where(m => m.TenantId == AbpSession.TenantId
                && m.SenderUserId == currentUserId
                && !m.IsDeletedBySender)
            .WhereIf(input.IsRead.HasValue, m => m.IsRead == input.IsRead.Value)
            .WhereIf(!string.IsNullOrWhiteSpace(input.Search),
                m => (m.Subject != null && m.Subject.ToLower().Contains(input.Search.Trim().ToLower()))
                    || m.Content.ToLower().Contains(input.Search.Trim().ToLower()))
            .WhereIf(!string.IsNullOrWhiteSpace(input.Keyword),
                m => (m.Subject != null && m.Subject.ToLower().Contains(input.Keyword.ToLower()))
                    || m.Content.ToLower().Contains(input.Keyword.ToLower()));

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderBy(input.Sorting ?? "CreationTime DESC")
            .PageBy(input)
            .ToListAsync();

        return new PagedResultDto<MessageListDto>(
            totalCount,
            ObjectMapper.Map<List<MessageListDto>>(items));
    }

    [AbpAuthorize(PermissionNames.Communication_Messages_View)]
    public async Task<ListResultDto<MessageDto>> GetThreadAsync(Guid threadId)
    {
        var currentUserId = AbpSession.UserId.Value;

        var messages = await _messageRepository
            .GetAll()
            .Where(m => m.TenantId == AbpSession.TenantId
                && m.ThreadId == threadId)
            .OrderBy(m => m.CreationTime)
            .ToListAsync();

        // Filter to only messages visible to the current user
        var visibleMessages = messages
            .Where(m => m.IsVisibleToUser(currentUserId))
            .ToList();

        return new ListResultDto<MessageDto>(
            ObjectMapper.Map<List<MessageDto>>(visibleMessages));
    }

    [AbpAuthorize(PermissionNames.Communication_Messages_Send)]
    public async Task<MessageDto> SendAsync(CreateMessageDto input)
    {
        var currentUserId = AbpSession.UserId.Value;

        // Cannot message self
        if (input.RecipientUserId == currentUserId)
            throw new UserFriendlyException(CommunicationExceptionCodes.CannotMessageSelf,
                "You cannot send a message to yourself.");

        // Validate recipient exists
        var recipientExists = await _userRepository
            .GetAll()
            .AnyAsync(u => u.Id == input.RecipientUserId);

        if (!recipientExists)
            throw new UserFriendlyException(CommunicationExceptionCodes.RecipientNotFound,
                "Recipient user not found.");

        // If replying, validate parent message exists and belongs to tenant
        Guid? threadId = null;
        if (input.ParentMessageId.HasValue)
        {
            var parentMessage = await _messageRepository
                .FirstOrDefaultAsync(m => m.Id == input.ParentMessageId.Value
                    && m.TenantId == AbpSession.TenantId);

            if (parentMessage == null)
                throw new UserFriendlyException(CommunicationExceptionCodes.MessageNotFound,
                    "Parent message not found.");

            // Inherit thread from parent
            threadId = parentMessage.ThreadId ?? parentMessage.Id;
        }

        var message = new Message(
            Guid.NewGuid(),
            AbpSession.TenantId,
            currentUserId,
            input.RecipientUserId,
            input.Content.Trim(),
            input.Subject?.Trim(),
            input.ParentMessageId);

        // Override ThreadId if inherited from parent
        if (threadId.HasValue)
            message.ThreadId = threadId;

        message.AttachmentUrl = input.AttachmentUrl?.Trim();

        await _messageRepository.InsertAsync(message);
        await CurrentUnitOfWork.SaveChangesAsync();

        return ObjectMapper.Map<MessageDto>(message);
    }

    [AbpAuthorize(PermissionNames.Communication_Messages_View)]
    public async Task MarkAsReadAsync(Guid id)
    {
        var currentUserId = AbpSession.UserId.Value;

        var message = await _messageRepository
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (message == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.MessageNotFound,
                "Message not found.");

        // Only recipient can mark as read
        if (message.RecipientUserId != currentUserId)
            throw new UserFriendlyException(CommunicationExceptionCodes.OnlyRecipientCanMarkRead,
                "Only the recipient can mark a message as read.");

        message.MarkAsRead();
        await _messageRepository.UpdateAsync(message);
        await CurrentUnitOfWork.SaveChangesAsync();
    }

    [AbpAuthorize(PermissionNames.Communication_Messages_Delete)]
    public async Task DeleteAsync(Guid id)
    {
        var currentUserId = AbpSession.UserId.Value;

        var message = await _messageRepository
            .FirstOrDefaultAsync(m => m.Id == id && m.TenantId == AbpSession.TenantId);

        if (message == null)
            throw new UserFriendlyException(CommunicationExceptionCodes.MessageNotFound,
                "Message not found.");

        if (currentUserId == message.SenderUserId)
        {
            message.DeleteForSender();
        }
        else if (currentUserId == message.RecipientUserId)
        {
            message.DeleteForRecipient();
        }
        else
        {
            throw new UserFriendlyException(CommunicationExceptionCodes.MessageNotVisible,
                "You do not have access to this message.");
        }

        await _messageRepository.UpdateAsync(message);
        await CurrentUnitOfWork.SaveChangesAsync();
    }
}
