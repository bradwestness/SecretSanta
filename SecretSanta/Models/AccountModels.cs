using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace SecretSanta.Models;

public class Account
{
    public Guid? Id { get; set; }

    [DisplayName("E-Mail Address"), Required, DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [DisplayName("Display Name"), Required]
    public string? DisplayName { get; set; }

    [DisplayName("Do Not Pick")]
    public IList<Guid> DoNotPick { get; set; } = new List<Guid>();

    public IDictionary<int, Guid?> Picked { get; set; } = new Dictionary<int, Guid?>();

    [DisplayName("Wish List")]
    public IDictionary<int, IList<WishlistItem>> Wishlist { get; set; } = new Dictionary<int, IList<WishlistItem>>();

    [DisplayName("Gift Received")]
    public IDictionary<int, ReceivedGift> ReceivedGift { get; set; } = new Dictionary<int, ReceivedGift>();
}

public class SendLogInLinkModel
{
    [Required, EmailAddress, DisplayName("E-Mail Address")]
    public string? Email { get; set; }
}

public class EditUsersModel
{
    public AddUserModel NewUser { get; set; } = new AddUserModel();

    public IList<EditUserModel> Users { get; set; } = new List<EditUserModel>();

    public bool AllPicked { get; set; }
}

public class AddUserModel
{
    [DisplayName("E-Mail Address"), Required, DataType(DataType.EmailAddress)]
    public string? Email { get; set; }

    [DisplayName("Display Name"), Required]
    public string? DisplayName { get; set; }
}

public class EditUserModel
{
    [Required]
    public Guid? AccountId { get; set; }

    [Required, EmailAddress, DisplayName("E-Mail Address")]
    public string? Email { get; set; }

    [Required, DisplayName("Display Name")]
    public string? DisplayName { get; set; }

    public Guid? Picked { get; set; }

    [DisplayName("PickedBy")]
    public string? PickedBy { get; set; }

    [DisplayName("Do Not Pick")]
    public IList<Guid> DoNotPick { get; set; } = new List<Guid>();

    public IList<SelectListItem> PickOptions { get; set; } = new List<SelectListItem>();

    public IList<SelectListItem> DoNotPickOptions { get; set; } = new List<SelectListItem>();
}
