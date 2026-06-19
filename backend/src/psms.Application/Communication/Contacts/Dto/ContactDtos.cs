using psms.Domain.Shared.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace psms.Communication.Contacts.Dto;

/// <summary>COMM-04: the current user's reachable contact details + registered devices.</summary>
public class MyContactDto
{
    public string Email { get; set; }
    public string MobileNumber { get; set; }
    public string WhatsAppNumber { get; set; }
    public List<DeviceTokenDto> Devices { get; set; } = new List<DeviceTokenDto>();
}

public class DeviceTokenDto
{
    public Guid Id { get; set; }
    public DevicePlatform Platform { get; set; }
    public string DeviceName { get; set; }
    public DateTime LastSeenDate { get; set; }
}

public class UpdateMyContactDto
{
    [StringLength(32)]
    public string MobileNumber { get; set; }

    [StringLength(32)]
    public string WhatsAppNumber { get; set; }
}

public class RegisterDeviceDto
{
    [Required]
    [StringLength(UserDeviceTokenConsts.MaxTokenLength)]
    public string Token { get; set; }

    [Required]
    public DevicePlatform Platform { get; set; }

    [StringLength(200)]
    public string DeviceName { get; set; }
}

// Mirrors the entity's length cap without referencing the Core entity from the DTO.
public static class UserDeviceTokenConsts
{
    public const int MaxTokenLength = 512;
}
