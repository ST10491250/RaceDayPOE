# Build and verification

- .NET SDK 8.0.427 was obtained in the editing environment.
- Both API and MVC C# sources compiled successfully with the SDK's Roslyn compiler.
- All Razor views were generated with the SDK Razor compiler using discovered MVC tag helpers, then compiled together with the MVC C# sources successfully.
- Standard `dotnet build` / MSBuild could not run in this environment because the host failed to provide .NET with its own process information. Direct compiler checks were used instead. API compilation used dependency assemblies from the uploaded ZIP; a fresh NuGet restore and standard solution build must still be run on your machine.
- A running MVC instance passed HTTP checks with a fixture API for: public home, sign-in, both dashboard routes, event/category/enrolment/result/banner/profile pages, role restriction and logout.
- MVC HTTP form checks passed for event creation (including all detail fields), category creation, result capture, participant enrolment, profile upload/display and banner upload/display. An invalid image was rejected before event creation reached the API.
- The actual C# upload service passed checks for signatures, extension and size rejection, generated filenames, saving/loading after service recreation, replacing images, old-file cleanup, optional uploads and user ID isolation.
- The original site.css hash matches the uploaded file. Static asset references and ZIP integrity were checked.

Limitations: No SQL Server was available, so the actual API/database end-to-end flow and the optional existing-database SQL script were not executed. MVC HTTP checks used a fixture API, not SQL Server. No visual browser rendering was verified. A successful direct compilation is not the same as a completed normal NuGet-restored solution build or database acceptance test.

Dashboard patch: added a Development-only, transactional check for the four legacy Event columns, JSON API error responses, local error detail propagation and /api/health. The suspected missing-column cause is not confirmed without the user database error. Actual SQL compatibility execution remains untested here.

Dashboard patch verification: both C# assemblies and MVC Razor views recompiled. The MVC HTTP suite passed again, including a simulated API 500 with an invalid-column error; the exact error and status were displayed on the page. No real SQL Server instance was available to execute the schema update.
