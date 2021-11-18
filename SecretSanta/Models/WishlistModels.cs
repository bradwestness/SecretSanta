using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SecretSanta.Models;

public class WishlistItem
{
    [HiddenInput]
    public Guid? Id { get; set; }

    [Required]
    public string? Name { get; set; }

    public string? Description { get; set; }

    [DisplayName("Link"), DataType(DataType.Url)]
    public string? Url { get; set; }

    public byte[]? PreviewImage { get; set; }
}

public class WishlistEditModel
{
    [Required]
    public Guid? AccountId { get; set; }

    [Required]
    public string? DisplayName { get; set; }

    public WishlistItem NewItem { get; set; } = new WishlistItem();

    public IList<WishlistItem> Items { get; set; } = new List<WishlistItem>();
}