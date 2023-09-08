using System.Data;
using System.Data.SQLite;
using Serilog;
using Serilog.Events;
using Y2DL.Utils;

namespace Y2DL.Database;

public class DatabaseManager
{
    private static SQLiteConnection database;

    public DatabaseManager(string filePath)
    {
        database = new SQLiteConnection(new SQLiteConnectionStringBuilder
        {
            DataSource = filePath,
            DateTimeFormat = SQLiteDateFormats.UnixEpoch,
            DateTimeKind = DateTimeKind.Utc,
            DefaultDbType = DbType.Object,
            FailIfMissing = false,
            ForeignKeys = true,
            SyncMode = SynchronizationModes.Full
        }.ToString());
    }

    public async Task Initialize()
    {
        try
        {
            await database.OpenAsync();

            using (var command = database.CreateCommand())
            {
                command.CommandText =
                    "CREATE TABLE IF NOT EXISTS 'DynamicChannelInfo' ('ChannelId' INTEGER, 'MessageId' INTEGER, 'YouTubeChannelId' TEXT, 'Hash' TEXT, PRIMARY KEY('Hash'));";
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (SQLiteException ex)
        {
            Log.Write(LogEventLevel.Warning, ex, "Database: Database has thrown an exception");
        }
        catch (Exception ex)
        {
        }
        finally
        {
            await database.CloseAsync();
        }
    }

    public async Task Add(ulong ChannelId, ulong messageId, string channelId)
    {
        try
        {
            await database.OpenAsync();
            using (var command = database.CreateCommand())
            {
                command.CommandText =
                    $"INSERT INTO 'DynamicChannelInfo' VALUES ({ChannelId}, {messageId}, '{channelId}', '{HashUtils.HashThingToSHA256String(ChannelId + messageId + channelId)}');";
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (SQLiteException ex)
        {
            Log.Write(LogEventLevel.Warning, ex, "Database: Database has thrown an exception");
        }
        catch (Exception ex)
        {
        }
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
            await database.OpenAsync();
            using (var cmd = database.CreateCommand())
            {
                cmd.CommandText =
                    $"SELECT * FROM 'DynamicChannelInfo' WHERE 'DynamicChannelInfo'.'YouTubeChannelId' == '{ytChannelId}' AND 'DynamicChannelInfo'.'ChannelId' == {channelId}";
                var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync()) num = reader.GetInt64(1).ToUlong();
            }
        }
        catch (SQLiteException ex)
        {
            Log.Write(LogEventLevel.Warning, ex, "Database: Database has thrown an exception");
        }
        catch (Exception ex)
        {
        }
        finally
        {
            await database.CloseAsync();
        }

        return num;
    }

    public async Task<bool> Exists(string ytChannelId, ulong channelId)
    {
        var exists = false;
        try
        {
            await database.OpenAsync();
            using (var cmd = database.CreateCommand())
            {
                cmd.CommandText =
                    $"SELECT * FROM 'DynamicChannelInfo' WHERE 'DynamicChannelInfo'.'YouTubeChannelId' == '{ytChannelId}' AND 'DynamicChannelInfo'.'ChannelId' == {channelId}";
                exists = await cmd.ExecuteScalarAsync() != null;
            }
        }
        catch (SQLiteException ex)
        {
            Log.Write(LogEventLevel.Warning, ex, "Database: Database has thrown an exception");
        }
        catch (Exception ex)
        {
        }
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

            using (var command = database.CreateCommand())
            {
                command.CommandText =
                    $"DELETE FROM 'DynamicChannelInfo' WHERE 'DynamicChannelInfo'.'MessageId' == {messageId};";
                var reader = await command.ExecuteNonQueryAsync();
            }
        }
        catch (SQLiteException ex)
        {
            Log.Write(LogEventLevel.Warning, ex, "Database: Database has thrown an exception");
        }
        catch (Exception ex)
        {
        }
        finally
        {
            await database.CloseAsync();
        }
    }
}