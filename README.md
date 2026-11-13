# Student Uniform Compliance Monitoring System

C# Windows Forms (.NET Framework 4.7.2) + SQL Server database. Built on top of your **FinalSystemUI** design.
One main window (`Form1`) shows one screen at a time. The screen is chosen from the account that logs in.

> **Important:** this code was written without being able to compile it (no Visual Studio here).
> The first build may show a small error or two. Send the exact error message and it can be fixed quickly.

## 1. Run it (Visual Studio 2022)

1. Open `FinalSystemUI.sln`.
2. Make sure SQL Server LocalDB exists: Visual Studio Installer > Modify > **.NET desktop development**
   (LocalDB is included), or install SQL Server Express and edit the connection string in `App.config`.
3. Press **F5**. No NuGet packages are needed.
4. On the first start the program **creates the service-based database file `App_Data\UniformComplianceDB.mdf`, all tables, and sample data**.
   It also saves sample login files to `Documents\UniformComplianceKeys\`
   (open `SAMPLE-ACCOUNTS.txt` there).

| Account | How to log in |
|---|---|
| Student | Student tab: email `juan@minsu.edu.ph`, password `Student@123` (no ID number needed) |
| HSG | HSG tab: email `hsg@minsu.edu.ph`, password `Hsg@12345` |
| OSSE | OSSE tab: email `osse@minsu.edu.ph`, password `Osse@12345` |

All three account types log in with **email + password**. Tick **Show password** to see what you typed. Press Enter to log in.
Passwords are saved only as PBKDF2 hashes. OSSE can set a new password for a Student or HSG account in **Users > Reset password**.
(An older database file that still has key-file accounts is upgraded on the first start: those HSG/OSSE accounts get the sample passwords above.)

## 2. What each role can do

| Role | Screens | Rules |
|---|---|---|
| Student | Dashboard, Violation history, My fines | Sees only their own records (forced in `RecordService`) |
| HSG (guard + CSG/USG) | Records, Add record | **Can add new students** ("+ New student" button, or when the ID is not found). Adds all 4 types (No uniform / Inappropriate attire / Invalid lanyard / No-ID). **Edits only within 12 hours** (checked in code and again in SQL). Sets fines and marks fines paid (the **Mark as Paid** button on the Records screen). Cannot edit No-ID records (only add them) |
| OSSE | Records, User, Clearance, **SQL Queries** | Adds (all 4 types) and deletes records, **adds new students** (User screen or "New student" in Add record), manages Student and HSG accounts, clears No-ID and lanyard records |

Violation rules: No uniform and Inappropriate attire are **fined**. Invalid lanyard and No ID are **not fined**; OSSE settles them at Clearance.
Standing: Good (0 violations), Warning (below the limit), Clearance hold (limit reached).

## 3. Database (service-based) and queries

**Service-based database** = the whole database is **one file**: `App_Data\UniformComplianceDB.mdf` (SQL Server LocalDB).
The program creates the file, the tables, the views, the stored procedure and the sample data on the first start.

**Connection:** `App.config` > `connectionStrings` > `UniformDb`

    Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=|DataDirectory|\UniformComplianceDB.mdf;Integrated Security=True

`|DataDirectory|` is the `App_Data` folder of the project (`Database/DataPath.cs`). All C# code connects through `Database/Db.cs`.

### See the database in Visual Studio (for the Information Management check)
1. Run the program once (F5), so the file exists. Close it.
2. **View > Server Explorer > Connect to Database** (plug icon) > Data source: **Microsoft SQL Server Database File** > Browse > `App_Data\UniformComplianceDB.mdf` > OK.
3. Expand the connection > **Tables / Views / Stored Procedures**. Right-click a table > **Show Table Data**.
4. Right-click the database > **New Query** to type or open a query.
5. (Optional) Solution Explorer > **Show All Files** > right-click `App_Data\UniformComplianceDB.mdf` > **Include In Project** (set *Copy to Output Directory* = Do not copy).

### Queries
* **In the program:** log in as OSSE > sidebar **SQL Queries**. 28 ready-made queries with the SQL text on screen and the result in a table
  (SELECT, WHERE, LIKE, INNER/LEFT JOIN, GROUP BY, HAVING, COUNT/SUM/AVG/MIN/MAX, subqueries, IN, EXISTS, NOT EXISTS, CASE, DATEDIFF, UNION, views, stored procedure).
  The window can only run those queries (read-only), so nothing can be damaged.
* **As files** (`Database/SqlScripts/`, also shown in Solution Explorer):

| File | Content |
|---|---|
| `01_CreateDatabase_Tables.sql` | CREATE DATABASE and CREATE TABLE (primary keys, foreign keys, UNIQUE, CHECK, DEFAULT, indexes) |
| `02_Views_StoredProcedures.sql` | 2 views and 1 stored procedure |
| `03_SampleData.sql` | INSERT sample data (only for an empty database) |
| `04_Queries.sql` | the same 28 queries |
| `05_Insert_Update_Delete_Examples.sql` | INSERT, UPDATE, DELETE inside a transaction that is rolled back |

The program's own SQL (the screens) is in `Repositories/` and always uses parameters.

### Tables
| Table | Purpose |
|---|---|
| Students | Student accounts (password saved as a PBKDF2 hash, never plain text) |
| HsgAdmin | HSG accounts: email, password hash, `IsStudent` flag (not a student = guard) |
| OsseAdmin | OSSE accounts: email and password hash |
| ViolationTypes | The 4 types, whether they are fined, default fine |
| Violations | Every record (who, what, when, who recorded it, last edit) |
| Fines | One fine per fined violation (amount, paid, paid date) |
| ClearanceRecords | When OSSE cleared a No-ID / lanyard record |
| Semesters | The current semester |
| Settings | `ClearanceThreshold` (5) and `EditWindowHours` (12) |
| AuditLog | Who did what and when |

Views: `vw_ViolationDetails`, `vw_StudentStanding`. Stored procedure: `usp_StudentSummary`.
Relationships: Students 1-N Violations, ViolationTypes 1-N Violations, Semesters 1-N Violations,
Violations 1-1 Fines, Violations 1-1 ClearanceRecords, OsseAdmin 1-N ClearanceRecords.
(`Violations.RecordedById` points to HsgAdmin or OsseAdmin depending on `RecordedByRole`, so it has no foreign key.)

## 4. Login security

* Password rule: at least 8 characters. Saved as `iterations.salt.hash` (PBKDF2-SHA256, 100,000 rounds), never as plain text (`Helpers/PasswordHasher.cs`).
* 5 wrong tries lock that email for 5 minutes (`LoginThrottle` in `Services/AuthService.cs`).
* A disabled account cannot log in. HSG and OSSE logins are written to the audit log.

## 5. Project folders

    Database/       Db.cs (connection + SQL helper), DataPath.cs (where the .mdf is), DbSchema.cs (tables, views, procedure),
                    DatabaseInitializer.cs (create the .mdf + sample data), SavedQueries.cs (the 28 queries), SqlScripts/*.sql
    Models/         plain classes (AppUser, ViolationRow, ...)
    Repositories/   all SQL, parameterized queries only
    Services/       the rules: AuthService, RecordService, UserService
    Helpers/        PasswordHasher, Session, Ui
    Dialogs/        pop-ups built in code (RecordEditorForm, UserEditorForm, InputBox, SplashForm)
    *.cs            your screens (LoginControl, StudentControl, HsgControl, OsseRecords, ...)

Screens never write SQL. They call `RecordService` / `UserService` / `AuthService`, which check who is logged in (`Session`).

## 6. Things I assumed (change if wrong)

* Any HSG can edit any record in its first 12 hours (not only their own).
* HSG and OSSE can set a fine and mark it paid at any time, as long as it is not paid yet.
* HSG and OSSE can both add No-ID records. Only OSSE can edit/delete or clear them.
* Fine 100.00 and the limit of 5 violations are placeholders (Settings / ViolationTypes tables).
* When a student is added without a password, a temporary 10-character password is made and shown once. OSSE can reset it later (User screen).
* No Appeal feature (it was not in your described system).
* There is no screen to add another OSSE account. The OSSE account is made on the first start (change its password with SQL if needed).

## 7. Not done / next steps

* Records screens load synchronously (fast with a small database). Login runs in the background.
* No printing or export of reports.
* Before real use: change or delete the sample accounts, create real accounts from the User screen,
  and give the app's SQL login only the rights it needs.

## Newly added (OSSE)
- **Users > Delete user** now works. An account with records cannot be deleted (disable it instead). The action is written to the audit log.
- **SQL Queries** and **Audit Log** buttons added to the OSSE sidebar (Records, Clearance, Users). Audit Log shows the newest 500 actions and can be searched.

## Revision (latest)
- **Login:** Student, HSG and OSSE all use **email + password**. Added **Show password** (view/hide). The key files and the `/osse` `/pass` start-up options were removed.
- **HSG and OSSE accounts** now have a password (`PasswordHash` column). Users > Add user asks for a password (or makes one), and **Reset password** works for Student and HSG.
- **Student > Violation History:** added the **No-ID** button. The filters are All, No Uniform, Lanyard (invalid lanyard only) and No-ID.
- **Cleaned the project:** removed `KeyFileService.cs`, `CommandLine.cs`, the unused `libbackground.jpg`, the hidden duplicate sidebars inside the Student pages,
  a hidden unused button in Clearance, the empty event handlers, and the `.vs`, `bin` and `obj` folders.
