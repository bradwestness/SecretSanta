using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using SecretSanta.Validation;

namespace SecretSanta.Models;

public class ReceivedGift
{
    [Required, HiddenInput]
    public Guid? Id { get; set; }

    [Required]
    public string? From { get; set; }

    [Required, HiddenInput]
    public string? To { get; set; }

    [DisplayName("Note"), Required]
    public string? Description { get; set; }

    public byte[]? Image { get; set; }

    [DisplayName("Photo with your gift"), Required, JsonIgnore, MaxFileSize(2000000)]
    public IFormFile? ImageUpload { get; set; }
}

public class ReceivedGiftEditModel
{
    public ReceivedGift Item { get; set; } = new();

    public IList<ReceivedGift> Gifts { get; set; } = new List<ReceivedGift>();
}