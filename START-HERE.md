# RaceDay POE — completed functionality

This is the POEFINAL application you uploaded, with its original dark, white, red and orange colour scheme. The original `RaceDayMVC/wwwroot/css/site.css` is unchanged. Added dashboard elements use those same colour variables.

## Start in Visual Studio

1. Extract this ZIP to a new writable folder. Open **RaceDay.sln** in Visual Studio 2022 with the ASP.NET workload and .NET 8 SDK installed.
2. Check `RaceDayApi/appsettings.json` → `ConnectionStrings:RaceDayConnection` for your SQL Server instance. Your uploaded database/JWT settings were retained. Start SQL Server.
3. Restore NuGet packages and build the solution. The API Entity Framework packages are aligned to 8.0.11.
4. Run `dotnet dev-certs https --trust` if your development HTTPS certificate is not trusted.
5. Configure both projects as startup projects, with **Start** and the **https** launch profile. A multi-project `.slnLaunch` profile is included for recent Visual Studio versions.
6. Website: **https://localhost:7267**. API Swagger: **https://localhost:7130/swagger**. MVC `ApiSettings:BaseUrl` already matches that API port. If using IIS Express, update it to the API address actually shown in your browser.

From two terminals in this folder:

```powershell
dotnet restore RaceDay.sln
dotnet build RaceDay.sln -c Release
dotnet run --project RaceDayApi/RaceDayApi.csproj --launch-profile https
```

```powershell
dotnet run --project RaceDayMVC/RaceDayMVC.csproj --launch-profile https
```

The API uses `EnsureCreated()` for a new database. In Development, it now also checks and adds missing Location, Distance, EventType and BannerImage columns in the existing Events table. This preserves existing rows and does not drop/recreate the database. Other schema changes still need migrations. If an older database is missing the Location, Distance, EventType or BannerImage columns already defined in your uploaded Event model, review and run `Database/UpdateExistingEvents.sql` against that database. Do not delete your database to resolve a schema mismatch.

## Finished flows

- Login now creates a protected MVC authentication cookie, forwards the API bearer token, respects token expiry and protects pages by account role. Logout is a real antiforgery-protected POST.
- Organiser dashboard shows live counts, enrolments and upcoming owned events.
- Create and edit events with name, description, date/time, location, distance and type. Delete events with no enrolments; events with enrolments return a clear conflict message.
- Upload a banner during event creation, or replace it using the Banner link on an owned event.
- Select owned events to add categories, view enrolments and capture results. Finish time format is hh:mm:ss (below 24 hours); position must be above zero. Duplicate results are rejected by the API.
- Participant dashboard shows real enrolments, upcoming entries and results. Browse live events, choose a category and enrol. Duplicate enrolment is handled with a clear message.
- Both roles can edit names and upload/replace profile photos. Organisers can also edit the contact phone number. Email is displayed as read-only because the supplied API does not support changing it.
- Existing public Home/About/Contact content and colour scheme remain. The Events page now uses live API events and displays locally uploaded banners. The Contact page remains the original informational page/form; no email delivery feature was added.

## Pictures: local wwwroot, no Azure

- `RaceDayMVC/wwwroot/uploads/banners/` — event banners.
- `RaceDayMVC/wwwroot/uploads/profiles/` — profile photos.
- `RaceDayMVC/App_Data/image-references/` — persistent links from API user/event IDs to filenames.

Preserve BOTH the uploads and App_Data folders when moving/updating the project. They must be writable by the application. Images remain after restart. Profile images are public static assets. These MVC-local files do not require database image columns and are not exposed as image paths in API DTOs. The original API banner endpoint remains available separately; the completed website uses the MVC-local upload flow.

JPG/JPEG, PNG and WebP are accepted, up to 5 MB per image. File extensions and signatures are checked on the server, generated filenames are used, and old files are removed after replacement. Total requests above 6 MB are rejected. Browser previews are included. If a banner save fails after an event has been created, a message directs the organiser to retry from the Banner page without creating a duplicate event.

## Quick walkthrough

Register an Organiser → create an event with banner → add a category → log out → register a Participant → enrol → check My enrolments → upload a profile photo → log out → Organiser opens Enrolments and captures a result → Participant checks My results. Try a second Organiser account to confirm the first account's events are inaccessible.

See BUILD-REPORT.md for checks performed and their limits.

## Dashboard error fix

This version automatically applies the four known legacy Event-column additions in Development. Stop both old projects before running this version. Keep your working SQL Server connection string in RaceDayApi/appsettings.json. If the page still fails, it now displays the API status/path and the actual local API exception rather than hiding it. Open https://localhost:7130/api/health for startup/database errors. Database changes require ALTER permission; if permission is denied, run Database/UpdateExistingEvents.sql in SSMS with an authorised database account. No rows are deleted.
