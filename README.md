SecretSanta
===========

### Simple Gift Exchange Organizer

SecretSanta is an easy to use gift exchange organizer that you can deploy to your own webserver, or even to a free Windows Azure webiste.

![Screenshot](https://raw.github.com/bradwestness/SecretSanta/master/screenshot.png)

## Features

* Add as many family members as you like
* No database needed, all data is stored as JSON in text files
* Users get to click a button to pick a recipient (more fun than just being told who you picked)
* Specify which people each user should NOT pick (e.g. spouses)
* Admin console for managing users
* Responsive Twitter Bootstrap template is mobile friendly
* Wish list functionality with pre-generated preview images for items included
* Configurable gift dollar limit (used for display purposes only)
* Send login links to all users, no passwords or account creation needed
* Emails are sent out to users once everyone has chosen their recipient
* Reminder functionality so users can anonymously remind their recipient to add items to their wish list

## Set Up

1. Add the following appSettings to appsettings.json, or [configure as app settings in Azure](https://azure.microsoft.com/en-us/blog/windows-azure-web-sites-how-application-strings-and-connection-strings-work/):
```json
{
    "SecretSanta:AdminEmail": "my_email@outlook.com",
    "SecretSanta:DefaultPreviewImage": "images/photo_not_available.png",
    "SecretSanta:DataDirectory": "App_Data",
    "SecretSanta:AccountFilePattern": "*.account.json",
    "SecretSanta:GiftDollarLimit": 40,
    "SecretSanta:SmtpHost": "smtp.sendgrid.net"
    "SecretSanta:SmtpPort": "587"
    "SecretSanta:SmtpUser": "my_email_user@azure.com"
    "SecretSanta:SmtpPass": "my_email_password"
}
```
3. Deploy to any server capable of running ASP.NET Core 6 or later
4. Make sure the folder specified in the `SecretSanta:DataDirectory` setting is writable
