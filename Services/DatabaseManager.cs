using System.Data.SQLite;
using Discord;
using Discord.Rest;
using Discord.Webhook;
using Discord.WebSocket;
using Y2DL.Models;
using Y2DL.Utils;

namespace Y2DL.Services;

public class DatabaseManager
{
    private static SQLiteConnection database;

    public DatabaseManager(string filePath)
    {
        database = new SQLiteConnection(new SQLiteConnectionStringBuilder()
        {
            DataSource = filePath,
            DateTimeFormat = SQLiteDateFormats.UnixEpoch,
            DateTimeKind = DateTimeKind.Utc,
            DefaultDbType = System.Data.DbType.Object,
            FailIfMissing = false,
            ForeignKeys = true,
            SyncMode = SynchronizationModes.Full,
        }.ToString());
    }

    public async Task Initialize()
    {
        try
        {
            await database.OpenAsync();

            using (SQLiteCommand command = database.CreateCommand())
            {
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS 'DynamicChannelInfo' ('ChannelId' INTEGER, 'MessageId' INTEGER, 'YouTubeChannelId' TEXT, 'Hash' TEXT, PRIMARY KEY('Hash'));";
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (SQLiteException ex) { 
			await Program.Log(new LogMessage(LogSeverity.Warning, "Database", "Database has thrown a exception", ex));
		}
        catch (Exception ex) { }
        finally
        {
            await database.CloseAsync();
        }
    }

    public async Task Add(ulong ChannelId, Database db)
    {
        try
        {
            await DatabaseManager.database.OpenAsync();
            using (SQLiteCommand command = database.CreateCommand())
            {
                command.CommandText = $"INSERT INTO 'DynamicChannelInfo' VALUES ({ChannelId}, {db.MessageId}, '{db.ChannelId}', '{Hashing.HashThingToSHA256String(ChannelId + db.MessageId + db.ChannelId)}');";
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (SQLiteException ex) { 
			await Program.Log(new LogMessage(LogSeverity.Warning, "Database", "Database has thrown a exception", ex));
		}
        catch (Exception ex) { }
        finally
        {
            await database.CloseAsync();
        }
    }
    
    public async Task<ulong> Get(string ytChannelId, ulong channelId)
    {
        ulong num = 0;
        try
        {
            await DatabaseManager.database.OpenAsync();
            using (SQLiteCommand cmd = database.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM 'DynamicChannelInfo' WHERE 'DynamicChannelInfo'.'YouTubeChannelId' == '{ytChannelId}' AND 'DynamicChannelInfo'.'ChannelId' == {channelId}";
                var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    num = reader.GetInt64(1).ToUlong();
                }
            }
        }
        catch (SQLiteException ex) { 
			await Program.Log(new LogMessage(LogSeverity.Warning, "Database", "Database has thrown a exception", ex));
		}
        catch (Exception ex) { }
        finally
        {
            await database.CloseAsync();
        }
        return num;
    }

    public async Task<bool> Exists(string ytChannelId, ulong channelId)
    {
        bool exists = false;
        try
        {
            await DatabaseManager.database.OpenAsync();
            using (SQLiteCommand cmd = database.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM 'DynamicChannelInfo' WHERE 'DynamicChannelInfo'.'YouTubeChannelId' == '{ytChannelId}' AND 'DynamicChannelInfo'.'ChannelId' == {channelId}";
                exists = await cmd.ExecuteScalarAsync() != null;
            }
        }
        catch (SQLiteException ex) { 
			await Program.Log(new LogMessage(LogSeverity.Warning, "Database", "Database has thrown a exception", ex));
		}
        catch (Exception ex) { }
        finally
        {
            await database.CloseAsync();
        }
        return exists;
    }

    public async void Remove(ulong messageId)
    {
        try
        {
            await database.OpenAsync();

            using (SQLiteCommand command = database.CreateCommand())
            {
                command.CommandText = $"DELETE FROM 'DynamicChannelInfo' WHERE 'DynamicChannelInfo'.'MessageId' == {messageId};";
                var reader = await command.ExecuteNonQueryAsync();
            }
        }
        catch (SQLiteException ex) { 
			await Program.Log(new LogMessage(LogSeverity.Warning, "Database", "Database has thrown a exception", ex));
		}
        catch (Exception ex) { }
        finally { await database.CloseAsync(); }
    }
}