# EduMatch



\# migration



---cmd

dotnet ef migrations add update-version-1.0 --project EduMatch.DataAccessLayer --startup-project EduMatch.PresentationLayer

dotnet ef database update --project EduMatch.DataAccessLayer --startup-project EduMatch.PresentationLayer





--- manager console

Add-Migration update-version-1.0 -Project EduMatch.DataAccessLayer -StartupProject EduMatch.PresentationLayer



Update-Database -Project EduMatch.DataAccessLayer -StartupProject EduMatch.PresentationLayer

