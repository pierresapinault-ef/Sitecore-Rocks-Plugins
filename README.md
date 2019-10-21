# Sitecore-Rocks-Plugins
Bundle of useful functions for Sitecore Rocks Windows or VS extension. Most are new functions that I created to bring more efficiency and speed up time at work. Some core functions have been amended as well (like update, delete, publish) so that normal users would not be able to use them (queries can be very dangerous!);
I have not amended the code before uploading to GitHub so some restrictions (username based or role based) might still be there as well as some company specific queries (like GetMediaUrl).
Code base: C#
Required: Sitecore DLLs and Sitecore Rocks binaries as well.
Intructions: Simply compile the project and place the DLL in the bin folder of your Sitecore instance.

List of new functions (explanation for each is in the project already):
  - Find Duplicate
  - FixHTML
  - FormatDate
  - GetFields
  - GetMediaUrl
  - GetReferrers
  - IsDuplicate
  - IsPublished
  - IsReferenced
  - PublishStatus
  - RegexContains
  - RemoveHTML
  - ToLower
  - ToUpper
  - Trim
  
  List of new keywords:
  - AllLanguagesQuery
  - CopyFieldsValue
  - Copy
  - InsertFromBranch
  - InsertFromTemplate
  - Move
  - PublishWithParams
  - Recycle
  - Remove
  - Republish
  - RestrictedUpdate
  - SafeDelete
  - SafePublish
  
  List of new Opcodes:
  same as list of new keywords
