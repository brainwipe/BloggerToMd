# BloggerToMd
A .NET converter for taking Blogger output and turning it into Markdown. It's a tool I used to conver the [Icar](http://www.icar.co.uk) blog. I hope you find it useful.

# Install
- Ensure you have dotnet Core 2 runtime installed
- Unzip the latest release into a folder

# Run
- Export your blogger blog from the Settings page in Blogger, you'll end up with a big XML file, we'll call that source (src)
- Put your blogger source in the place where you want the markdown put, as this tool puts the markdown next to it
- Open a command line
- Change directory to where BloggerToMd.dll is, eg: `cd c:/BloggerToMd`
- Run with the name of your file, eg: `dotnet BloggerToMd.dll -src "c:/users/me/documents/myblog.xml"`