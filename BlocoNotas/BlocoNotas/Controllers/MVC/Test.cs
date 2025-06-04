// 1. Create Interface (optional but good practice)

using BlocoNotas.Data;
using Console = System.Console;
using Exception = System.Exception;
using Microsoft.EntityFrameworkCore;

public interface IDatabaseTestService
{
    Task<bool> TestConnectionAsync();
    Task<string> GetDatabaseInfoAsync();
}

// 2. Create Service Class


namespace BlocoNotas.Services
{
    public class DatabaseTestService : IDatabaseTestService
    {
        private readonly ApplicationDbContext _context; // Replace with your actual DbContext name
        
        public DatabaseTestService(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public async System.Threading.Tasks.Task<bool> TestConnectionAsync()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                
                if (canConnect)
                {
                    var userCount = await _context.Users.CountAsync();
                    Console.WriteLine($"✅ Connected! Found {userCount} users in database.");
                    return true;
                }
                else
                {
                    Console.WriteLine("❌ Cannot connect to database");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Connection failed: {ex.Message}");
                return false;
            }
        }
        
        public async System.Threading.Tasks.Task<string> GetDatabaseInfoAsync()
        {
            try
            {
                var connectionString = _context.Database.GetConnectionString();
                var databaseName = _context.Database.GetDbConnection().Database;
                
                await _context.Database.OpenConnectionAsync();
                var serverVersion = _context.Database.GetDbConnection().ServerVersion;
                await _context.Database.CloseConnectionAsync();
                
                var userCount = await _context.Users.CountAsync();
                var noteCount = await _context.Notes.CountAsync();
                
                return $"✅ Connected to: {databaseName}\n" +
                       $"📍 Server: {serverVersion}\n" +
                       $"👥 Users: {userCount}\n" +
                       $"📝 Notes: {noteCount}\n" +
                       $"🔗 Status: Active";
            }
            catch (Exception ex)
            {
                return $"❌ Connection failed: {ex.Message}";
            }
        }
    }
}

// 3. Register in Program.cs
/*
Add this to your Program.cs before builder.Build():

builder.Services.AddScoped<IDatabaseTestService, DatabaseTestService>();
*/