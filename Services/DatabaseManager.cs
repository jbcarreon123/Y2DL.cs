using System.Data.SQLite;
using Y2DL.Models;
using Y2DL.Utils;

namespace Y2DL.Services;

public class DatabaseManager
{
    private static SQLiteConnection database;

    public DatabaseManager(string filePath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(filePath));
        
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
                    "CREATE TABLE IF NOT EXISTS 'DynamicChannelInfo' ('ChannelId' INTEGER, 'MessageId' INTEGER, 'Hash' TEXT, PRIMARY KEY('ChannelId' AUTOINCREMENT));";
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (SQLiteException ex) { }
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
                command.CommandText = $"INSERT INTO 'DynamicChannelInfo' VALUES ({ChannelId}, {db.MessageId}, '{db.MessageHash}');";
                await command.ExecuteNonQueryAsync();
            }
        }
        catch (SQLiteException ex) { }
        catch (Exception ex) { }
        finally
        {
            await database.CloseAsync();
        }
    }
    
    public async Task<ulong> Get(string messageHash, ulong channelId)
    {
        ulong num = 0;
        try
        {
            await DatabaseManager.database.OpenAsync();
            using (SQLiteCommand cmd = database.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM 'DynamicChannelInfo' WHERE 'DynamicChannelInfo'.'Hash' == '{messageHash}' AND 'DynamicChannelInfo'.'ChannelId' == {channelId}";
                var reader = await cmd.ExecuteReaderAsync();

                while (reader.Read())
                {
                    num = reader.GetInt64(1).ToUlong();
                }
            }
        }
        catch (SQLiteException ex) { }
        catch (Exception ex) { }
        finally
        {
            await database.CloseAsync();
        }
        return num;
    }

    public async Task<bool> Exists(string messageHash, ulong channelId)
    {
        bool exists = false;
        try
        {
            await DatabaseManager.database.OpenAsync();
            using (SQLiteCommand cmd = database.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM 'DynamicChannelInfo' WHERE 'DynamicChannelInfo'.'Hash' == '{messageHash}' AND 'DynamicChannelInfo'.'ChannelId' == {channelId}";
                exists = await cmd.ExecuteScalarAsync() != null;
            }
        }
        catch (SQLiteException ex) { }
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
        catch (SQLiteException ex) { }
        catch (Exception ex) { }
        finally { await database.CloseAsync(); }
    }
}