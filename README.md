# Pulic_Ip_Uploader
Command that get your device public ip and trigger an IFTTT event of choice.
Will trigger the IFTTT event if the public ip has changed since last run. 
Public ip is stored locally and compared with new value every run.

Is created to be run in a crontab to update the lastest public ip in a google spreadsheet in order
to access the device through ssh remotely.

Updated with functionality to send mail when ip changes aswell.
Set up with a mailserver of your choice, password is saved using dataprotection.

If using a gmail for sending emails, you will need to tick "Allow unprotected apps" in your gmail.
Could also work with api-keys, but not implemented that possibility yet.

___
### Configuration (configuration/configuration.json)

- IFTTTKey -> Your secret key to your IFTTT Recipe
- IFTTTEventName -> Your event name of the IFTTT Recipe to trigger the recipe
- IpProviders -> A list of domains to fetch the public ip of your device
- UseEmailNotification -> Toggle if email should be sent when IP changes
- EmailConfiguration -> Configurations needed to send email.


## Example configuration
```json
    {
        "IFTTTKey": "YOUR_MAKER_KEY",
        "IFTTTEventName": "YOUR_MAKER_EVENT",
        "IpProviders": [
            "http://ipinfo.io/ip",
            "https://api.ipify.org/",
            "http://bot.whatismyipaddress.com/"
        ],
        "UseEmailNotification": true,
        "EmailConfiguration": {
            "Sender": "example@gmail.com",
            "Receiver": "receiver@gmail.com",
            "SmtpServer": "smtp.gmail.com",
            "SenderUserName": "example@gmail.com",
            "Port": 465
        }
    }
```
