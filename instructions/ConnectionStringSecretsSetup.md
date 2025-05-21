**Using .NET Secret Manager for Connection Strings (Development)**

This guide explains how to use the .NET Secret Manager tool to store sensitive data, like your SQL Server connection string containing a password, securely during local development for the Registry API project. This prevents accidentally committing passwords to source control.

**Why Use Secret Manager?**

-   **Security:** Keeps sensitive data (passwords, API keys) out of your project's source code and configuration files (appsettings.json).
-   **Prevents Accidental Commits:** Secrets stored this way are not part of your project directory and won't be checked into Git.
-   **Environment Specific:** Designed specifically for the local development environment.
-   **Integration:** The ASP.NET Core configuration system automatically reads from the Secret Manager in the Development environment.

**Steps:**

1.  **Open a Terminal/Command Prompt:**
    -   Navigate to the root directory of your RegistryApi project (the folder containing the RegistryApi.csproj file).
2.  **Initialize User Secrets:**
    -   If you haven't already done this for your project, run the following command:
    -   **dotnet user-secrets init**
-   This command adds a unique \<UserSecretsId\> tag to your .csproj file. It doesn't create the secrets file yet, but it links the project to a unique secrets location on your machine.
-    You may be asked to **run 'dotnet dev-certs https --trust'** - so do this.
1.  **Set the Connection String Secret:**
    -   Run the following command in your terminal. This command stores your specific connection string under the key ConnectionStrings:RegistryDatabaseConnection in a secrets.json file located in your user profile directory (e.g., %APPDATA%\\Microsoft\\UserSecrets\\\<user_secrets_id\>\\secrets.json on Windows).
    -   **dotnet user-secrets set "ConnectionStrings:RegistryDatabaseConnection" "Put your Connection string here"**
-   **Note:** Make sure the key "ConnectionStrings:RegistryDatabaseConnection" exactly matches the key used in your Program.cs when calling builder.Configuration.GetConnectionString("RegistryDatabaseConnection"). The colon (:) indicates nesting in the configuration structure.
1.  **Verify the Secret (Optional):**
    -   You can view the secrets you've set for the project by running:
    -   dotnet user-secrets list
-   You should see the key and value you just set.
1.  **Remove/Comment Out from** appsettings.json**:**
    -   Open your appsettings.json and appsettings.Development.json files.
    -   Locate the ConnectionStrings section.
    -   **Delete or comment out** the line containing the RegistryDatabaseConnection:
    -   {
    -   "ConnectionStrings": {
    -   // "RegistryDatabaseConnection": "Server=Edmund-PC;Database=RegistryDatabase;User ID=sa;Password=******;TrustServerCertificate=True;" // DELETE or COMMENT OUT this line
    -   },
    -   // ... other settings ...
    -   }
-   This ensures the application doesn't accidentally read the less secure connection string from the file.
1.  **Run Your Application:**
    -   When you run your application using the Development environment profile (dotnet run), the configuration system will automatically:
        -   Load appsettings.json.
        -   Load appsettings.Development.json (overriding values from the base file).
        -   Load user secrets (overriding values from the previous files).
    -   Your code in Program.cs:
    -   var connectionString = builder.Configuration.GetConnectionString("RegistryDatabaseConnection");

...will now seamlessly retrieve the connection string value you stored in the Secret Manager.

**Important Security Reminder:**

-   The .NET Secret Manager is intended **only for local development**. The secrets.json file is stored unencrypted on your machine.
-   **Do not** use this method for production or staging environments.
-   For production, use more secure methods like **Azure Key Vault**, other cloud provider secret managers, or secure **Environment Variables** configured on the hosting server.
-   Consider using **Windows Authentication** or a dedicated, less-privileged SQL login instead of sa for better overall database security.
