//***************************************************************************************
// Writer: Stylish Esper
//***************************************************************************************

using Esper.FeelSpeak.Settings;
using Esper.MemoryDB;
using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;

namespace Esper.FeelSpeak.Database
{
    /// <summary>
    /// Feel Speak's database handler. Executes all database queries.
    /// </summary>
    public static class FeelSpeakDatabase
    {
        /// <summary>
        /// The database directory path in the editor.
        /// </summary>
        public static string DatabaseEditorDirectoryPath { get; private set; } = Application.streamingAssetsPath;

        /// <summary>
        /// The database directory path at runtime.
        /// </summary>
        public static string DatabaseRuntimeDirectoryPath { get; private set; } = Application.persistentDataPath;

        public static string databaseName;
        private readonly static string dialogueTableName = "dialogue_graphs";
        private readonly static string characterTableName = "characters";
        private readonly static string emotionTableName = "emotions";

        private static SQLiteConnection sqlConnection;
        private static MemoryDatabase mdbConnection;

        /// <summary>
        /// If there is an active connection to the database.
        /// </summary>
        public static bool IsConnected
        {
            get
            {
                return DynamicDBReturnCall(() => sqlConnection != null, () => mdbConnection != null);
            }
        }

        #region DYNAMIC
        /// <summary>
        /// Invokes a DB method depending on the current database settings and platform.
        /// </summary>
        /// <param name="sql">The SQLite method.</param>
        /// <param name="mdb">The MDB method.</param>
        private static void DynamicDBCall(Action sql, Action mdb)
        {
#if UNITY_EDITOR
            sql();
            return;
#endif

#pragma warning disable CS0162
            if (FeelSpeak.Settings.databaseType == FeelSpeakSettings.DatabaseType.SQLite)
            {
                sql();
            }
            else
            {
                mdb();
            }
#pragma warning restore CS0162
        }

        /// <summary>
        /// Invokes a DB method depending on the current database settings and platform.
        /// </summary>
        /// <param name="sql">The SQLite method.</param>
        /// <param name="mdb">The MDB method.</param>
        private static T DynamicDBReturnCall<T>(Func<T> sql, Func<T> mdb)
        {
#if UNITY_EDITOR
            return sql();
#endif

#pragma warning disable CS0162
            if (FeelSpeak.Settings.databaseType == FeelSpeakSettings.DatabaseType.SQLite)
            {
                return sql();
            }
            else
            {
                return mdb();
            }
#pragma warning restore CS0162
        }

        /// <summary>
        /// Initializes the ite database connection.
        /// </summary>
        public static void Initialize()
        {
            DynamicDBCall(InitializeSQL, InitializeMDB);
        }

        /// <summary>
        /// Disconnects from the ite database.
        /// </summary>
        public static void Disconnect()
        {
            DynamicDBCall(DisconnectSQL, DisconnectMDB);
        }

        /// <summary>
        /// Deletes all records from a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        public static void ClearTable(string tableName)
        {
            DynamicDBCall(() => ClearTableSQL(tableName), () => ClearTableMDB(tableName));
        }

        /// <summary>
        /// Gets the total number of records in a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The total number of records.</returns>
        public static int GetRecordCount(string tableName)
        {
            return DynamicDBReturnCall(() => GetRecordCountSQL(tableName), () => GetRecordCountMDB(tableName));
        }

        /// <summary>
        /// Creates the dialogue table if it doesn't exist.
        /// </summary>
        public static void CreateDialogueTable()
        {
            DynamicDBCall(CreateDialogueTableSQL, CreateDialogueTableMDB);
        }

        /// <summary>
        /// Inserts a dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        public static void InsertDialogueRecord(DialogueRecord dialogueRecord)
        {
            DynamicDBCall(() => InsertDialogueRecordSQL(dialogueRecord), () => InsertDialogueRecordMDB(dialogueRecord));
        }

        /// <summary>
        /// Updates an existing dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        public static void UpdateDialogueRecord(DialogueRecord dialogueRecord)
        {
            DynamicDBCall(() => UpdateDialogueRecordSQL(dialogueRecord), () => UpdateDialogueRecordMDB(dialogueRecord));
        }

        /// <summary>
        /// Deletes an existing dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        public static void DeleteDialogueRecord(DialogueRecord dialogueRecord)
        {
            DynamicDBCall(() => DeleteDialogueRecordSQL(dialogueRecord), () => DeleteDialogueRecordMDB(dialogueRecord));
        }

        /// <summary>
        /// Deletes an existing dialogue record.
        /// </summary>
        /// <param name="id">The dialogue record ID.</param>
        public static void DeleteDialogueRecord(int id)
        {
            DynamicDBCall(() => DeleteDialogueRecordSQL(id), () => DeleteDialogueRecordMDB(id));
        }

        /// <summary>
        /// Checks if a dialogue record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        public static bool HasDialogueRecord(int id)
        {
            return DynamicDBReturnCall(() => HasDialogueRecordSQL(id), () => HasDialogueRecordMDB(id));
        }

        /// <summary>
        /// Gets a dialogue record.
        /// </summary>
        /// <param name="id">The dialogue ID.</param>
        /// <returns>A dialogue record with the ID or null if it doesn't exist.</returns>
        public static DialogueRecord GetDialogueRecord(int id)
        {
            return DynamicDBReturnCall(() => GetDialogueRecordSQL(id), () => GetDialogueRecordMDB(id));
        }

        /// <summary>
        /// Gets a dialogue record.
        /// </summary>
        /// <param name="dialogueName">The dialogue name.</param>
        /// <returns>A dialogue record with the ID or null if it doesn't exist.</returns>
        public static DialogueRecord GetDialogueRecord(string dialogueName)
        {
            return DynamicDBReturnCall(() => GetDialogueRecordSQL(dialogueName), () => GetDialogueRecordMDB(dialogueName));
        }

        /// <summary>
        /// Gets all dialogue records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All dialogue records within range filtered by the pattern.</returns>
        public static List<DialogueRecord> GetDialogueRecords(int min, int max, string pattern, bool desc)
        {
            return DynamicDBReturnCall(() => GetDialogueRecordsSQL(min, max, pattern, desc), () => GetDialogueRecordsMDB(min, max, pattern, desc));
        }

        /// <summary>
        /// Gets all dialogue records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All dialogue records within range filtered by the pattern.</returns>
        public static List<DialogueRecord> GetDialogueRecords(string pattern, bool desc)
        {
            return DynamicDBReturnCall(() => GetDialogueRecordsSQL(pattern, desc), () => GetDialogueRecordsMDB(pattern, desc));
        }

        /// <summary>
        /// Gets the total number of records in the dialogue table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        public static int GetDialogueCount()
        {
            return DynamicDBReturnCall(GetDialogueCountSQL, GetDialogueCountMDB);
        }

        /// <summary>
        /// Gets the total number of records in the dialogue table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        public static int GetFilteredDialogueCount(string pattern)
        {
            return DynamicDBReturnCall(() => GetFilteredDialogueCountSQL(pattern), () => GetFilteredDialogueCountMDB(pattern));
        }

        /// <summary>
        /// Creates the character table if it doesn't exist.
        /// </summary>
        public static void CreateCharacterTable()
        {
            DynamicDBCall(CreateCharacterTableSQL, CreateCharacterTableMDB);
        }

        /// <summary>
        /// Inserts a character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        public static void InsertCharacterRecord(CharacterRecord characterRecord)
        {
            DynamicDBCall(() => InsertCharacterRecordSQL(characterRecord), () => InsertCharacterRecordMDB(characterRecord));
        }

        /// <summary>
        /// Updates an existing character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        public static void UpdateCharacterRecord(CharacterRecord characterRecord)
        {
            DynamicDBCall(() => UpdateCharacterRecordSQL(characterRecord), () => UpdateCharacterRecordMDB(characterRecord));
        }

        /// <summary>
        /// Deletes an existing character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        public static void DeleteCharacterRecord(CharacterRecord characterRecord)
        {
            DynamicDBCall(() => DeleteCharacterRecordSQL(characterRecord), () => DeleteCharacterRecordMDB(characterRecord));
        }

        /// <summary>
        /// Deletes an existing character record.
        /// </summary>
        /// <param name="id">The character record ID.</param>
        public static void DeleteCharacterRecord(int id)
        {
            DynamicDBCall(() => DeleteCharacterRecordSQL(id), () => DeleteCharacterRecordMDB(id));
        }

        /// <summary>
        /// Checks if a character record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        public static bool HasCharacterRecord(int id)
        {
            return DynamicDBReturnCall(() => HasCharacterRecordSQL(id), () => HasCharacterRecordMDB(id));
        }

        /// <summary>
        /// Gets a character record.
        /// </summary>
        /// <param name="id">The character ID.</param>
        /// <returns>A character record with the ID or null if it doesn't exist.</returns>
        public static CharacterRecord GetCharacterRecord(int id)
        {
            return DynamicDBReturnCall(() => GetCharacterRecordSQL(id), () => GetCharacterRecordMDB(id));
        }

        /// <summary>
        /// Gets a character record.
        /// </summary>
        /// <param name="characterName">The character name.</param>
        /// <returns>A character record with the ID or null if it doesn't exist.</returns>
        public static CharacterRecord GetCharacterRecord(string characterName)
        {
            return DynamicDBReturnCall(() => GetCharacterRecordSQL(characterName), () => GetCharacterRecordMDB(characterName));
        }

        /// <summary>
        /// Gets all character records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All character records within range filtered by the pattern.</returns>
        public static List<CharacterRecord> GetCharacterRecords(int min, int max, string pattern, bool desc)
        {
            return DynamicDBReturnCall(() => GetCharacterRecordsSQL(min, max, pattern, desc), () => GetCharacterRecordsMDB(min, max, pattern, desc));
        }

        /// <summary>
        /// Gets all character records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All character records within range filtered by the pattern.</returns>
        public static List<CharacterRecord> GetCharacterRecords(string pattern, bool desc)
        {
            return DynamicDBReturnCall(() => GetCharacterRecordsSQL(pattern, desc), () => GetCharacterRecordsMDB(pattern, desc));
        }

        /// <summary>
        /// Gets the total number of records in the character table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        public static int GetCharacterCount()
        {
            return DynamicDBReturnCall(GetCharacterCountSQL, GetCharacterCountMDB);
        }

        /// <summary>
        /// Gets the total number of records in the character table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        public static int GetFilteredCharacterCount(string pattern)
        {
            return DynamicDBReturnCall(() => GetFilteredCharacterCountSQL(pattern), () => GetFilteredCharacterCountMDB(pattern));
        }

        /// <summary>
        /// Creates the emotion table if it doesn't exist.
        /// </summary>
        public static void CreateEmotionTable()
        {
            DynamicDBCall(CreateEmotionTableSQL, CreateEmotionTableMDB);
        }

        /// <summary>
        /// Inserts an emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        public static void InsertEmotionRecord(EmotionRecord emotionRecord)
        {
            DynamicDBCall(() => InsertEmotionRecordSQL(emotionRecord), () => InsertEmotionRecordMDB(emotionRecord));
        }

        /// <summary>
        /// Updates an existing emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        public static void UpdateEmotionRecord(EmotionRecord emotionRecord)
        {
            DynamicDBCall(() => UpdateEmotionRecordSQL(emotionRecord), () => UpdateEmotionRecordMDB(emotionRecord));
        }

        /// <summary>
        /// Deletes an existing emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        public static void DeleteEmotionRecord(EmotionRecord emotionRecord)
        {
            DynamicDBCall(() => DeleteEmotionRecordSQL(emotionRecord), () => DeleteEmotionRecordMDB(emotionRecord));
        }

        /// <summary>
        /// Deletes an existing emotion record.
        /// </summary>
        /// <param name="id">The emotion record ID.</param>
        public static void DeleteEmotionRecord(int id)
        {
            DynamicDBCall(() => DeleteEmotionRecordSQL(id), () => DeleteEmotionRecordMDB(id));
        }

        /// <summary>
        /// Checks if an emotion record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        public static bool HasEmotionRecord(int id)
        {
            return DynamicDBReturnCall(() => HasEmotionRecordSQL(id), () => HasEmotionRecordMDB(id));
        }

        /// <summary>
        /// Gets an emotion record.
        /// </summary>
        /// <param name="id">The emotion ID.</param>
        /// <returns>An emotion record with the ID or null if it doesn't exist.</returns>
        public static EmotionRecord GetEmotionRecord(int id)
        {
            return DynamicDBReturnCall(() => GetEmotionRecordSQL(id), () => GetEmotionRecordMDB(id));
        }

        /// <summary>
        /// Gets an emotion record.
        /// </summary>
        /// <param name="emotionName">The emotion name.</param>
        /// <returns>An emotion record with the ID or null if it doesn't exist.</returns>
        public static EmotionRecord GetEmotionRecord(string emotionName)
        {
            return DynamicDBReturnCall(() => GetEmotionRecordSQL(emotionName), () => GetEmotionRecordMDB(emotionName));
        }

        /// <summary>
        /// Gets all emotion records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All emotion records within range filtered by the pattern.</returns>
        public static List<EmotionRecord> GetEmotionRecords(int min, int max, string pattern, bool desc)
        {
            return DynamicDBReturnCall(() => GetEmotionRecordsSQL(min, max, pattern, desc), () => GetEmotionRecordsMDB(min, max, pattern, desc));
        }

        /// <summary>
        /// Gets all emotion records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All emotion records within range filtered by the pattern.</returns>
        public static List<EmotionRecord> GetEmotionRecords(string pattern, bool desc)
        {
            return DynamicDBReturnCall(() => GetEmotionRecordsSQL(pattern, desc), () => GetEmotionRecordsMDB(pattern, desc));
        }

        /// <summary>
        /// Gets the total number of records in the emotion table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        public static int GetEmotionCount()
        {
            return DynamicDBReturnCall(GetEmotionCountSQL, GetEmotionCountMDB);
        }

        /// <summary>
        /// Gets the total number of records in the emotion table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        public static int GetFilteredEmotionCount(string pattern)
        {
            return DynamicDBReturnCall(() => GetFilteredEmotionCountSQL(pattern), () => GetFilteredEmotionCountMDB(pattern));
        }
        #endregion

        #region SQLITE
        /// <summary>
        /// Initializes the SQLite database connection.
        /// </summary>
        private static void InitializeSQL()
        {
            if (IsConnected)
            {
                return;
            }

            string databaseFullPath = string.Empty;

            bool updateDatabase = false;
            databaseName = $"{FeelSpeak.Settings.databaseName}.db";

#if UNITY_EDITOR
            databaseFullPath = Path.Combine(DatabaseEditorDirectoryPath, databaseName);

            // Create database directory if it does not exists
            if (!Directory.Exists(DatabaseEditorDirectoryPath))
            {
                Directory.CreateDirectory(DatabaseEditorDirectoryPath);
            }

            // Create database file if it does not exist
            if (!File.Exists(databaseFullPath))
            {
                File.WriteAllBytes(databaseFullPath, new byte[] { });
                updateDatabase = true;
            }
#else
            databaseFullPath = Path.Combine(DatabaseRuntimeDirectoryPath, databaseName);

            // Create database directory if it does not exist
            if (!Directory.Exists(DatabaseRuntimeDirectoryPath))
            {
                Directory.CreateDirectory(DatabaseRuntimeDirectoryPath);
            }

            if (!File.Exists(databaseFullPath))
            {
                CreateRuntimeDatabaseSQL();
            }

            // Always update the database in case any changes were made
            updateDatabase = true;
#endif

            sqlConnection = new SQLiteConnection(databaseFullPath);

            // Create tables if they don't exist
            CreateDialogueTable();
            CreateCharacterTable();
            CreateEmotionTable();

            if (updateDatabase)
            {
                ClearTable(dialogueTableName);
                ClearTable(characterTableName);
                ClearTable(emotionTableName);

                var graphs = FeelSpeak.GetAllDialogueGraphs();

                foreach (var item in graphs)
                {
                    item.UpdateDatabaseRecord();
                }

                var characters = FeelSpeak.GetAllCharacters();

                foreach (var item in characters)
                {
                    item.UpdateDatabaseRecord();
                }

                var emotions = FeelSpeak.GetAllEmotions();

                foreach (var item in emotions)
                {
                    item.UpdateDatabaseRecord();
                }

                graphs = null;
                characters = null;
                emotions = null;

                Resources.UnloadUnusedAssets();
            }

            if (Application.isPlaying)
            {
                FeelSpeakLogger.Log("Feel Speak: Initialized database connection.");
            }
        }

        /// <summary>
        /// Disconnects from the SQLite database.
        /// </summary>
        private static void DisconnectSQL()
        {
            if (!IsConnected)
            {
                return;
            }

            sqlConnection.Close();
            sqlConnection.Dispose();
            sqlConnection = null;

            if (Application.isPlaying)
            {
                DeleteRuntimeDatabaseSQL();
                FeelSpeakLogger.Log("Feel Speak: Disconnected from database.");
            }
        }

        /// <summary>
        /// Copies the database from StreamingAssets and stores it in the user's system. This is
        /// necessary to have a connection with the database at runtime.
        /// </summary>
        private static void CreateRuntimeDatabaseSQL()
        {
            string databaseEditorFullPath = Path.Combine(DatabaseEditorDirectoryPath, databaseName);
            string databaseRuntimeFullPath = Path.Combine(DatabaseRuntimeDirectoryPath, databaseName);

            string uri = string.Empty;

#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_IOS
            uri = "file://" + databaseEditorFullPath;
#else
            uri = databaseEditorFullPath;
#endif

            // Copy file from default database (from StreamingAssets)
            var loadingRequest = UnityWebRequest.Get(uri);
            loadingRequest.SendWebRequest();

            while (!loadingRequest.isDone)
            {
                switch (loadingRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                        FeelSpeakLogger.LogError("Feel Speak: Failed to create runtime database (connection error).");
                        return;

                    case UnityWebRequest.Result.ProtocolError:
                        FeelSpeakLogger.LogError("Feel Speak: Failed to create runtime database (protocol error).");
                        return;

                    case UnityWebRequest.Result.DataProcessingError:
                        FeelSpeakLogger.LogError("Feel Speak: Failed to create runtime database (data processing error).");
                        return;
                }
            }

            if (loadingRequest.downloadHandler.data == null || loadingRequest.downloadHandler.data.Length == 0)
            {
                FeelSpeakLogger.LogError("Feel Speak: Failed to create runtime database. This usually means that the editor " +
                    "database has not been created or found. Try navigating to Window > Feel Speak > Settings, click " +
                    "'Validate', and then try again. If this persists, consider contacting the developer.");
                return;
            }

            File.WriteAllBytes(databaseRuntimeFullPath, loadingRequest.downloadHandler.data);
            loadingRequest.Dispose();
        }

        /// <summary>
        /// Deletes the runtime database. This requires a disconnection first.
        /// </summary>
        public static void DeleteRuntimeDatabaseSQL()
        {
            var dbName = string.IsNullOrEmpty(databaseName) ? FeelSpeak.Settings.databaseName : databaseName;
            string databaseFullPath = Path.Combine(DatabaseRuntimeDirectoryPath, dbName);
            File.Delete(databaseFullPath);
        }

        /// <summary>
        /// Deletes the editor database. This requires a disconnection first.
        /// </summary>
        public static void DeleteEditorDatabaseSQL()
        {
            var dbName = string.IsNullOrEmpty(databaseName) ? FeelSpeak.Settings.databaseName : databaseName;
            string databaseFullPath = Path.Combine(DatabaseEditorDirectoryPath, dbName);
            File.Delete(databaseFullPath);
            File.Delete($"{databaseFullPath}.meta");
        }

        /// <summary>
        /// Deletes all records from a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        private static void ClearTableSQL(string tableName)
        {
            sqlConnection.Execute($"DELETE FROM {tableName}");
        }

        /// <summary>
        /// Gets the total number of records in a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The total number of records.</returns>
        private static int GetRecordCountSQL(string tableName)
        {
            int count = sqlConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {tableName}");
            return count;
        }

        /// <summary>
        /// Creates the dialogue table if it doesn't exist.
        /// </summary>
        private static void CreateDialogueTableSQL()
        {
            sqlConnection.Execute($"CREATE TABLE IF NOT EXISTS {dialogueTableName} (id INTEGER NOT NULL PRIMARY KEY, object_name VARCHAR(255), graph_name VARCHAR(255), group_id INTEGER)");
        }

        /// <summary>
        /// Inserts a dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        private static void InsertDialogueRecordSQL(DialogueRecord dialogueRecord)
        {
            var cmd = sqlConnection.CreateCommand($"INSERT INTO {dialogueTableName} VALUES (@id, @object_name, @graph_name, @group_id)");
            cmd.Bind("@id", dialogueRecord.id);
            cmd.Bind("@object_name", dialogueRecord.objectName);
            cmd.Bind("@graph_name", dialogueRecord.graphName);
            cmd.Bind("@group_id", -1);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates an existing dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        private static void UpdateDialogueRecordSQL(DialogueRecord dialogueRecord)
        {
            var cmd = sqlConnection.CreateCommand($"UPDATE {dialogueTableName} SET object_name = @object_name, graph_name = @graph_name, group_id = @group_id WHERE id = @id");
            cmd.Bind("@id", dialogueRecord.id);
            cmd.Bind("@object_name", dialogueRecord.objectName);
            cmd.Bind("@graph_name", dialogueRecord.graphName);
            cmd.Bind("@group_id", -1);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes an existing dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        private static void DeleteDialogueRecordSQL(DialogueRecord dialogueRecord)
        {
            var cmd = sqlConnection.CreateCommand($"DELETE FROM {dialogueTableName} WHERE id = @id");
            cmd.Bind("@id", dialogueRecord.id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes an existing dialogue record.
        /// </summary>
        /// <param name="id">The dialogue record ID.</param>
        private static void DeleteDialogueRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"DELETE FROM {dialogueTableName} WHERE id = @id");
            cmd.Bind("@id", id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Checks if a dialogue record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        private static bool HasDialogueRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {dialogueTableName} WHERE id = @id");
            cmd.Bind("@id", id);

            var records = cmd.ExecuteQuery<DialogueRecord>();

            return records.Count > 0;
        }

        /// <summary>
        /// Gets a dialogue record.
        /// </summary>
        /// <param name="id">The dialogue ID.</param>
        /// <returns>A dialogue record with the ID or null if it doesn't exist.</returns>
        private static DialogueRecord GetDialogueRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {dialogueTableName} WHERE id = @id");
            cmd.Bind("@id", id);

            var records = cmd.ExecuteQuery<DialogueRecord>();

            if (records.Count > 0)
            {
                return records.First();
            }

            return null;
        }

        /// <summary>
        /// Gets a dialogue record.
        /// </summary>
        /// <param name="dialogueName">The dialogue name.</param>
        /// <returns>A dialogue record with the ID or null if it doesn't exist.</returns>
        private static DialogueRecord GetDialogueRecordSQL(string dialogueName)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {dialogueTableName} WHERE LOWER(graph_name) = LOWER(@graph_name)");
            cmd.Bind("@graph_name", dialogueName);

            var records = cmd.ExecuteQuery<DialogueRecord>();

            if (records.Count > 0)
            {
                return records.First();
            }

            return null;
        }

        /// <summary>
        /// Gets all dialogue records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All dialogue records within range filtered by the pattern.</returns>
        private static List<DialogueRecord> GetDialogueRecordsSQL(int min, int max, string pattern, bool desc)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {dialogueTableName} WHERE LOWER(graph_name) LIKE LOWER(@pattern) ORDER BY LOWER(graph_name) {(desc ? "DESC" : "ASC")} LIMIT @max OFFSET @min");
            cmd.Bind("@min", min);
            cmd.Bind("@max", max);
            cmd.Bind("@pattern", $"%{pattern}%");

            var records = cmd.ExecuteQuery<DialogueRecord>();
            return records.ToList();
        }

        /// <summary>
        /// Gets all dialogue records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All dialogue records within range filtered by the pattern.</returns>
        private static List<DialogueRecord> GetDialogueRecordsSQL(string pattern, bool desc)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {dialogueTableName} WHERE LOWER(graph_name) LIKE LOWER(@pattern) ORDER BY LOWER(graph_name) {(desc ? "DESC" : "ASC")}");
            cmd.Bind("@pattern", $"%{pattern}%");

            var records = cmd.ExecuteQuery<DialogueRecord>();
            return records.ToList();
        }

        /// <summary>
        /// Gets the total number of records in the dialogue table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        private static int GetDialogueCountSQL()
        {
            return GetRecordCountSQL(dialogueTableName);
        }

        /// <summary>
        /// Gets the total number of records in the dialogue table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        private static int GetFilteredDialogueCountSQL(string pattern)
        {
            int count = sqlConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {dialogueTableName} WHERE LOWER(graph_name) LIKE LOWER(?)", $"%{pattern}%");
            return count;
        }

        /// <summary>
        /// Creates the character table if it doesn't exist.
        /// </summary>
        private static void CreateCharacterTableSQL()
        {
            sqlConnection.Execute($"CREATE TABLE IF NOT EXISTS {characterTableName} (id INTEGER NOT NULL PRIMARY KEY, object_name VARCHAR(255), character_name TEXT)");
        }

        /// <summary>
        /// Inserts a character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        private static void InsertCharacterRecordSQL(CharacterRecord characterRecord)
        {
            var cmd = sqlConnection.CreateCommand($"INSERT INTO {characterTableName} VALUES (@id, @object_name, @character_name)");
            cmd.Bind("@id", characterRecord.id);
            cmd.Bind("@object_name", characterRecord.objectName);
            cmd.Bind("@character_name", characterRecord.characterName);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates an existing character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        private static void UpdateCharacterRecordSQL(CharacterRecord characterRecord)
        {
            var cmd = sqlConnection.CreateCommand($"UPDATE {characterTableName} SET object_name = @object_name, character_name = @character_name WHERE id = @id");
            cmd.Bind("@id", characterRecord.id);
            cmd.Bind("@object_name", characterRecord.objectName);
            cmd.Bind("@character_name", characterRecord.characterName);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes an existing character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        private static void DeleteCharacterRecordSQL(CharacterRecord characterRecord)
        {
            var cmd = sqlConnection.CreateCommand($"DELETE FROM {characterTableName} WHERE id = @id");
            cmd.Bind("@id", characterRecord.id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes an existing character record.
        /// </summary>
        /// <param name="id">The character record ID.</param>
        private static void DeleteCharacterRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"DELETE FROM {characterTableName} WHERE id = @id");
            cmd.Bind("@id", id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Checks if a character record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        private static bool HasCharacterRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {characterTableName} WHERE id = @id");
            cmd.Bind("@id", id);

            var records = cmd.ExecuteQuery<CharacterRecord>();

            return records.Count > 0;
        }

        /// <summary>
        /// Gets a character record.
        /// </summary>
        /// <param name="id">The character ID.</param>
        /// <returns>A character record with the ID or null if it doesn't exist.</returns>
        private static CharacterRecord GetCharacterRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {characterTableName} WHERE id = @id");
            cmd.Bind("@id", id);

            var records = cmd.ExecuteQuery<CharacterRecord>();

            if (records.Count > 0)
            {
                return records.First();
            }

            return null;
        }

        /// <summary>
        /// Gets a character record.
        /// </summary>
        /// <param name="characterName">The character name.</param>
        /// <returns>A character record with the ID or null if it doesn't exist.</returns>
        private static CharacterRecord GetCharacterRecordSQL(string characterName)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {characterTableName} WHERE LOWER(character_name) = LOWER(@character_name)");
            cmd.Bind("@character_name", characterName);

            var records = cmd.ExecuteQuery<CharacterRecord>();

            if (records.Count > 0)
            {
                return records.First();
            }

            return null;
        }

        /// <summary>
        /// Gets all character records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All character records within range filtered by the pattern.</returns>
        private static List<CharacterRecord> GetCharacterRecordsSQL(int min, int max, string pattern, bool desc)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {characterTableName} WHERE LOWER(character_name) LIKE LOWER(@pattern) ORDER BY LOWER(character_name) {(desc ? "DESC" : "ASC")} LIMIT @max OFFSET @min");
            cmd.Bind("@min", min);
            cmd.Bind("@max", max);
            cmd.Bind("@pattern", $"%{pattern}%");

            var records = cmd.ExecuteQuery<CharacterRecord>();
            return records.ToList();
        }

        /// <summary>
        /// Gets all character records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All character records within range filtered by the pattern.</returns>
        private static List<CharacterRecord> GetCharacterRecordsSQL(string pattern, bool desc)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {characterTableName} WHERE LOWER(character_name) LIKE LOWER(@pattern) ORDER BY LOWER(character_name) {(desc ? "DESC" : "ASC")}");
            cmd.Bind("@pattern", $"%{pattern}%");

            var records = cmd.ExecuteQuery<CharacterRecord>();
            return records.ToList();
        }

        /// <summary>
        /// Gets the total number of records in the character table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        private static int GetCharacterCountSQL()
        {
            return GetRecordCountSQL(characterTableName);
        }

        /// <summary>
        /// Gets the total number of records in the character table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        private static int GetFilteredCharacterCountSQL(string pattern)
        {
            int count = sqlConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {characterTableName} WHERE LOWER(character_name) LIKE LOWER(?)", $"%{pattern}%");
            return count;
        }

        /// <summary>
        /// Creates the emotion table if it doesn't exist.
        /// </summary>
        private static void CreateEmotionTableSQL()
        {
            sqlConnection.Execute($"CREATE TABLE IF NOT EXISTS {emotionTableName} (id INTEGER NOT NULL PRIMARY KEY, object_name VARCHAR(255), emotion_name TEXT)");
        }

        /// <summary>
        /// Inserts an emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        private static void InsertEmotionRecordSQL(EmotionRecord emotionRecord)
        {
            var cmd = sqlConnection.CreateCommand($"INSERT INTO {emotionTableName} VALUES (@id, @object_name, @emotion_name)");
            cmd.Bind("@id", emotionRecord.id);
            cmd.Bind("@object_name", emotionRecord.objectName);
            cmd.Bind("@emotion_name", emotionRecord.emotionName);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Updates an existing emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        private static void UpdateEmotionRecordSQL(EmotionRecord emotionRecord)
        {
            var cmd = sqlConnection.CreateCommand($"UPDATE {emotionTableName} SET object_name = @object_name, emotion_name = @emotion_name WHERE id = @id");
            cmd.Bind("@id", emotionRecord.id);
            cmd.Bind("@object_name", emotionRecord.objectName);
            cmd.Bind("@emotion_name", emotionRecord.emotionName);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes an existing emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        private static void DeleteEmotionRecordSQL(EmotionRecord emotionRecord)
        {
            var cmd = sqlConnection.CreateCommand($"DELETE FROM {emotionTableName} WHERE id = @id");
            cmd.Bind("@id", emotionRecord.id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Deletes an existing emotion record.
        /// </summary>
        /// <param name="id">The emotion record ID.</param>
        private static void DeleteEmotionRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"DELETE FROM {emotionTableName} WHERE id = @id");
            cmd.Bind("@id", id);
            cmd.ExecuteNonQuery();
        }

        /// <summary>
        /// Checks if an emotion record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        private static bool HasEmotionRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {emotionTableName} WHERE id = @id");
            cmd.Bind("@id", id);

            var records = cmd.ExecuteQuery<EmotionRecord>();

            return records.Count > 0;
        }

        /// <summary>
        /// Gets an emotion record.
        /// </summary>
        /// <param name="id">The emotion ID.</param>
        /// <returns>An emotion record with the ID or null if it doesn't exist.</returns>
        private static EmotionRecord GetEmotionRecordSQL(int id)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {emotionTableName} WHERE id = @id");
            cmd.Bind("@id", id);

            var records = cmd.ExecuteQuery<EmotionRecord>();

            if (records.Count > 0)
            {
                return records.First();
            }

            return null;
        }

        /// <summary>
        /// Gets an emotion record.
        /// </summary>
        /// <param name="emotionName">The emotion name.</param>
        /// <returns>An emotion record with the ID or null if it doesn't exist.</returns>
        private static EmotionRecord GetEmotionRecordSQL(string emotionName)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {emotionTableName} WHERE LOWER(emotion_name) = LOWER(@emotion_name)");
            cmd.Bind("@emotion_name", emotionName);

            var records = cmd.ExecuteQuery<EmotionRecord>();

            if (records.Count > 0)
            {
                return records.First();
            }

            return null;
        }

        /// <summary>
        /// Gets all emotion records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All emotion records within range filtered by the pattern.</returns>
        private static List<EmotionRecord> GetEmotionRecordsSQL(int min, int max, string pattern, bool desc)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {emotionTableName} WHERE LOWER(emotion_name) LIKE LOWER(@pattern) ORDER BY LOWER(emotion_name) {(desc ? "DESC" : "ASC")} LIMIT @max OFFSET @min");
            cmd.Bind("@min", min);
            cmd.Bind("@max", max);
            cmd.Bind("@pattern", $"%{pattern}%");

            var records = cmd.ExecuteQuery<EmotionRecord>();
            return records.ToList();
        }

        /// <summary>
        /// Gets all emotion records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All emotion records within range filtered by the pattern.</returns>
        private static List<EmotionRecord> GetEmotionRecordsSQL(string pattern, bool desc)
        {
            var cmd = sqlConnection.CreateCommand($"SELECT * FROM {emotionTableName} WHERE LOWER(emotion_name) LIKE LOWER(@pattern) ORDER BY LOWER(emotion_name) {(desc ? "DESC" : "ASC")}");
            cmd.Bind("@pattern", $"%{pattern}%");

            var records = cmd.ExecuteQuery<EmotionRecord>();
            return records.ToList();
        }

        /// <summary>
        /// Gets the total number of records in the emotion table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        private static int GetEmotionCountSQL()
        {
            return GetRecordCountSQL(emotionTableName);
        }

        /// <summary>
        /// Gets the total number of records in the emotion table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        private static int GetFilteredEmotionCountSQL(string pattern)
        {
            int count = sqlConnection.ExecuteScalar<int>($"SELECT COUNT(*) FROM {emotionTableName} WHERE LOWER(emotion_name) LIKE LOWER(?)", $"{pattern}");
            return count;
        }
        #endregion

        #region MDB
        /// <summary>
        /// Initializes the MDB database connection.
        /// </summary>
        private static void InitializeMDB()
        {
            if (IsConnected)
            {
                return;
            }

#if UNITY_EDITOR
            var existing = Resources.LoadAll<MemoryDatabase>("FeelSpeakResources");

            foreach (var item in existing)
            {
                UnityEditor.AssetDatabase.DeleteAsset(UnityEditor.AssetDatabase.GetAssetPath(item));
            }

            var path = Path.Combine(Editor.AssetSearch.FolderOf<UnityEngine.TextAsset>("FeelSpeakIdentifier"), "Resources", "FeelSpeakResources", $"{FeelSpeak.Settings.databaseName}.asset");
            var db = MemoryDatabase.CreateIfNotExists(path);
#endif

            mdbConnection = Resources.Load<MemoryDatabase>($"FeelSpeakResources/{FeelSpeak.Settings.databaseName}");

#if UNITY_EDITOR
            PopulateMemoryDatabase(mdbConnection);
#endif

            if (Application.isPlaying)
            {
                FeelSpeakLogger.Log("Feel Speak: Initialized database connection.");
            }
        }

        /// <summary>
        /// Populates the memory database (MDB).
        /// </summary>
        /// <param name="db">The MDB to populate.</param>
        public static void PopulateMemoryDatabase(MemoryDatabase db)
        {
            db.ClearTable(dialogueTableName);
            db.ClearTable(characterTableName);
            db.ClearTable(emotionTableName);

            var graphs = FeelSpeak.GetAllDialogueGraphs();

            foreach (var item in graphs)
            {
                var record = item.DatabaseRecord;
                db.Set(dialogueTableName, record.id.ToString(), record);
            }

            var characters = FeelSpeak.GetAllCharacters();

            foreach (var item in characters)
            {
                var record = item.DatabaseRecord;
                db.Set(characterTableName, record.id.ToString(), record);
            }

            var emotions = FeelSpeak.GetAllEmotions();

            foreach (var item in emotions)
            {
                var record = item.DatabaseRecord;
                db.Set(emotionTableName, record.id.ToString(), record);
            }

            graphs = null;
            characters = null;
            emotions = null;

            Resources.UnloadUnusedAssets();

            if (Application.isPlaying)
            {
                FeelSpeakLogger.Log("Feel Speak: Initialized database connection.");
            }
        }

        /// <summary>
        /// Disconnects from the MDB database.
        /// </summary>
        private static void DisconnectMDB()
        {
            if (!IsConnected)
            {
                return;
            }

            mdbConnection = null;
            Resources.UnloadUnusedAssets();

            if (Application.isPlaying)
            {
                FeelSpeakLogger.Log("Feel Speak: Disconnected from database.");
            }
        }

        /// <summary>
        /// Deletes all records from a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        private static void ClearTableMDB(string tableName)
        {
            mdbConnection.ClearTable(tableName);
        }

        /// <summary>
        /// Gets the total number of records in a table.
        /// </summary>
        /// <param name="tableName">The name of the table.</param>
        /// <returns>The total number of records.</returns>
        private static int GetRecordCountMDB(string tableName)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Creates the dialogue table if it doesn't exist.
        /// </summary>
        private static void CreateDialogueTableMDB()
        {
            mdbConnection.GetOrCreateTable(dialogueTableName);
        }

        /// <summary>
        /// Inserts a dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        private static void InsertDialogueRecordMDB(DialogueRecord dialogueRecord)
        {
            mdbConnection.Set(dialogueTableName, dialogueRecord.id.ToString(), dialogueRecord);
        }

        /// <summary>
        /// Updates an existing dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        private static void UpdateDialogueRecordMDB(DialogueRecord dialogueRecord)
        {
            mdbConnection.Set(dialogueTableName, dialogueRecord.id.ToString(), dialogueRecord);
        }

        /// <summary>
        /// Deletes an existing dialogue record.
        /// </summary>
        /// <param name="dialogueRecord">The dialogue record.</param>
        private static void DeleteDialogueRecordMDB(DialogueRecord dialogueRecord)
        {
            mdbConnection.Delete(dialogueTableName, dialogueRecord.id.ToString());
        }

        /// <summary>
        /// Deletes an existing dialogue record.
        /// </summary>
        /// <param name="id">The dialogue record ID.</param>
        private static void DeleteDialogueRecordMDB(int id)
        {
            mdbConnection.Delete(dialogueTableName, id.ToString());
        }

        /// <summary>
        /// Checks if a dialogue record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        private static bool HasDialogueRecordMDB(int id)
        {
            var success = mdbConnection.TryGet(dialogueTableName, id.ToString(), out bool v);
            return success && v;
        }

        /// <summary>
        /// Gets a dialogue record.
        /// </summary>
        /// <param name="id">The dialogue ID.</param>
        /// <returns>A dialogue record with the ID or null if it doesn't exist.</returns>
        private static DialogueRecord GetDialogueRecordMDB(int id)
        {
            var success = mdbConnection.TryGet(dialogueTableName, id.ToString(), out DialogueRecord v);

            if (!success)
            {
                return null;
            }

            return v;
        }

        /// <summary>
        /// Gets a dialogue record.
        /// </summary>
        /// <param name="dialogueName">The dialogue name.</param>
        /// <returns>A dialogue record with the ID or null if it doesn't exist.</returns>
        private static DialogueRecord GetDialogueRecordMDB(string dialogueName)
        {
            var success = mdbConnection.TryGetByProperty(dialogueTableName, "graphName", dialogueName, out DialogueRecord v);

            if (!success)
            {
                return null;
            }

            return v;
        }

        /// <summary>
        /// Gets all dialogue records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All dialogue records within range filtered by the pattern.</returns>
        private static List<DialogueRecord> GetDialogueRecordsMDB(int min, int max, string pattern, bool desc)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Gets all dialogue records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All dialogue records within range filtered by the pattern.</returns>
        private static List<DialogueRecord> GetDialogueRecordsMDB(string pattern, bool desc)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Gets the total number of records in the dialogue table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        private static int GetDialogueCountMDB()
        {
            return GetRecordCountMDB(dialogueTableName);
        }

        /// <summary>
        /// Gets the total number of records in the dialogue table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        private static int GetFilteredDialogueCountMDB(string pattern)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Creates the character table if it doesn't exist.
        /// </summary>
        private static void CreateCharacterTableMDB()
        {
            mdbConnection.GetOrCreateTable(characterTableName);
        }

        /// <summary>
        /// Inserts a character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        private static void InsertCharacterRecordMDB(CharacterRecord characterRecord)
        {
            mdbConnection.Set(characterTableName, characterRecord.id.ToString(), characterRecord);
        }

        /// <summary>
        /// Updates an existing character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        private static void UpdateCharacterRecordMDB(CharacterRecord characterRecord)
        {
            mdbConnection.Set(characterTableName, characterRecord.id.ToString(), characterRecord);
        }

        /// <summary>
        /// Deletes an existing character record.
        /// </summary>
        /// <param name="characterRecord">The character record.</param>
        private static void DeleteCharacterRecordMDB(CharacterRecord characterRecord)
        {
            mdbConnection.Delete(characterTableName, characterRecord.id.ToString());
        }

        /// <summary>
        /// Deletes an existing character record.
        /// </summary>
        /// <param name="id">The character record ID.</param>
        private static void DeleteCharacterRecordMDB(int id)
        {
            mdbConnection.Delete(characterTableName, id.ToString());
        }

        /// <summary>
        /// Checks if a character record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        private static bool HasCharacterRecordMDB(int id)
        {
            var success = mdbConnection.TryGet(characterTableName, id.ToString(), out bool v);
            return success && v;
        }

        /// <summary>
        /// Gets a character record.
        /// </summary>
        /// <param name="id">The character ID.</param>
        /// <returns>A character record with the ID or null if it doesn't exist.</returns>
        private static CharacterRecord GetCharacterRecordMDB(int id)
        {
            var success = mdbConnection.TryGet(characterTableName, id.ToString(), out CharacterRecord v);

            if (!success)
            {
                return null;
            }

            return v;
        }

        /// <summary>
        /// Gets a character record.
        /// </summary>
        /// <param name="characterName">The character name.</param>
        /// <returns>A character record with the ID or null if it doesn't exist.</returns>
        private static CharacterRecord GetCharacterRecordMDB(string characterName)
        {
            var success = mdbConnection.TryGetByProperty(characterTableName, "characterName", characterName, out CharacterRecord v);

            if (!success)
            {
                return null;
            }

            return v;
        }

        /// <summary>
        /// Gets all character records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All character records within range filtered by the pattern.</returns>
        private static List<CharacterRecord> GetCharacterRecordsMDB(int min, int max, string pattern, bool desc)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Gets all character records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All character records within range filtered by the pattern.</returns>
        private static List<CharacterRecord> GetCharacterRecordsMDB(string pattern, bool desc)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Gets the total number of records in the character table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        private static int GetCharacterCountMDB()
        {
            return GetRecordCountMDB(characterTableName);
        }

        /// <summary>
        /// Gets the total number of records in the character table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        private static int GetFilteredCharacterCountMDB(string pattern)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Creates the emotion table if it doesn't exist.
        /// </summary>
        private static void CreateEmotionTableMDB()
        {
            mdbConnection.GetOrCreateTable(emotionTableName);
        }

        /// <summary>
        /// Inserts an emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        private static void InsertEmotionRecordMDB(EmotionRecord emotionRecord)
        {
            mdbConnection.Set(emotionTableName, emotionRecord.id.ToString(), emotionRecord);
        }

        /// <summary>
        /// Updates an existing emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        private static void UpdateEmotionRecordMDB(EmotionRecord emotionRecord)
        {
            mdbConnection.Set(emotionTableName, emotionRecord.id.ToString(), emotionRecord);
        }

        /// <summary>
        /// Deletes an existing emotion record.
        /// </summary>
        /// <param name="emotionRecord">The emotion record.</param>
        private static void DeleteEmotionRecordMDB(EmotionRecord emotionRecord)
        {
            mdbConnection.Delete(emotionTableName, emotionRecord.id.ToString());
        }

        /// <summary>
        /// Deletes an existing emotion record.
        /// </summary>
        /// <param name="id">The emotion record ID.</param>
        private static void DeleteEmotionRecordMDB(int id)
        {
            mdbConnection.Delete(emotionTableName, id.ToString());
        }

        /// <summary>
        /// Checks if an emotion record exists.
        /// </summary>
        /// <param name="id">The record ID.</param>
        /// <returns>True if the record exists. Otherwise, false.</returns>
        private static bool HasEmotionRecordMDB(int id)
        {
            var success = mdbConnection.TryGet(emotionTableName, id.ToString(), out bool v);
            return success && v;
        }

        /// <summary>
        /// Gets an emotion record.
        /// </summary>
        /// <param name="id">The emotion ID.</param>
        /// <returns>An emotion record with the ID or null if it doesn't exist.</returns>
        private static EmotionRecord GetEmotionRecordMDB(int id)
        {
            var success = mdbConnection.TryGet(emotionTableName, id.ToString(), out EmotionRecord v);

            if (!success)
            {
                return null;
            }

            return v;
        }

        /// <summary>
        /// Gets an emotion record.
        /// </summary>
        /// <param name="emotionName">The emotion name.</param>
        /// <returns>An emotion record with the ID or null if it doesn't exist.</returns>
        private static EmotionRecord GetEmotionRecordMDB(string emotionName)
        {
            var success = mdbConnection.TryGetByProperty(emotionTableName, "emotionName", emotionName, out EmotionRecord v);

            if (!success)
            {
                return null;
            }

            return v;
        }

        /// <summary>
        /// Gets all emotion records within a range and only includes those that contain a given string.
        /// </summary>
        /// <param name="min">The min range.</param>
        /// <param name="max">The max range.</param>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All emotion records within range filtered by the pattern.</returns>
        private static List<EmotionRecord> GetEmotionRecordsMDB(int min, int max, string pattern, bool desc)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Gets all emotion records that contain a given string.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <param name="desc">If descending order by mode should be used.</param>
        /// <returns>All emotion records within range filtered by the pattern.</returns>
        private static List<EmotionRecord> GetEmotionRecordsMDB(string pattern, bool desc)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }

        /// <summary>
        /// Gets the total number of records in the emotion table.
        /// </summary>
        /// <returns>The total number of records.</returns>
        private static int GetEmotionCountMDB()
        {
            return GetRecordCountMDB(emotionTableName);
        }

        /// <summary>
        /// Gets the total number of records in the emotion table filtered by the pattern.
        /// </summary>
        /// <param name="pattern">The pattern.</param>
        /// <returns>The total number of filtered records.</returns>
        private static int GetFilteredEmotionCountMDB(string pattern)
        {
            throw new NotSupportedException("Feel Speak: this functionality is not supported with MDB. Consider using SQLite instead.");
        }
        #endregion
    }
}