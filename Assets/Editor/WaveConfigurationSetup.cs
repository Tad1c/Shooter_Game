using UnityEditor;
using UnityEngine;
using Scripts;

namespace Editor
{
    public static class WaveConfigurationSetup
    {
        [MenuItem("Game/Quick Setup/Create Example Wave Configuration")]
        public static void CreateExampleConfiguration()
        {
            if (!EditorUtility.DisplayDialog(
                "Create Example Configuration",
                "This will create example Enemy Data, Wave Data, and Level Configuration assets in Assets/Resources/. Continue?",
                "Create", "Cancel"))
            {
                return;
            }

            // Ensure Resources folder exists
            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Enemies"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Enemies");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Waves"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Waves");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Levels"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Levels");
            }
            if (!AssetDatabase.IsValidFolder("Assets/Resources/Weapons"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Weapons");
            }

            // Create Enemy Data
            var basicEnemy = CreateEnemyData("BasicEnemy", 50f, 2f, 10f, Color.white, 1f);
            var fastEnemy = CreateEnemyData("FastEnemy", 30f, 4f, 5f, new Color(0.5f, 0.8f, 1f), 0.8f);
            var tankEnemy = CreateEnemyData("TankEnemy", 150f, 1f, 20f, new Color(1f, 0.5f, 0.5f), 1.3f);

            // Create Waves
            var wave1 = CreateWaveData("Wave_01_Easy", "Easy Wave - Basic enemies only",
                new[] { basicEnemy }, new[] { 100f }, 3f, 1, WaveCompletionType.KillCount, 10, 30f);

            var wave2 = CreateWaveData("Wave_02_Medium", "Medium Wave - Mixed enemies",
                new[] { basicEnemy, fastEnemy }, new[] { 60f, 40f }, 2f, 1, WaveCompletionType.KillCount, 20, 45f);

            var wave3 = CreateWaveData("Wave_03_Hard", "Hard Wave - All enemy types",
                new[] { basicEnemy, fastEnemy, tankEnemy }, new[] { 50f, 30f, 20f }, 1.5f, 2, WaveCompletionType.KillCount, 30, 60f);

            // Create Level Configuration
            var level1 = CreateLevelConfiguration("Level_01", "First level - Tutorial waves",
                new[] { wave1, wave2, wave3 }, 5f, 2f, 15f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorUtility.DisplayDialog(
                "Setup Complete!",
                "Example configuration created!\n\n" +
                "Created:\n" +
                "- 3 Enemy Data assets\n" +
                "- 3 Wave Data assets\n" +
                "- 1 Level Configuration\n" +
                "- 1 Weapon Configuration (Crossbow)\n\n" +
                "⚠️ IMPORTANT: You still need to:\n" +
                "1. Create an Enemy prefab with EnemyView component\n" +
                "2. Create a Projectile prefab with ProjectileView component\n" +
                "3. Assign prefabs to Enemy Data and Projectile Data\n" +
                "4. Assign Level_01 and Crossbow to GameInstaller\n\n" +
                "See COMBAT_SYSTEM_SETUP.md for detailed instructions.",
                "OK");

            // Select the level configuration
            Selection.activeObject = level1;
            EditorGUIUtility.PingObject(level1);
        }

        private static EnemyData CreateEnemyData(string name, float health, float speed, float damage,
            Color color, float scale)
        {
            var enemyData = ScriptableObject.CreateInstance<EnemyData>();

            var so = new SerializedObject(enemyData);
            so.FindProperty("_enemyName").stringValue = name;
            so.FindProperty("_maxHealth").floatValue = health;
            so.FindProperty("_moveSpeed").floatValue = speed;
            so.FindProperty("_damage").floatValue = damage;
            so.FindProperty("_attackRange").floatValue = 1.2f; // Close range attacks
            so.FindProperty("_attackCooldown").floatValue = 1f; // 1 attack per second
            so.FindProperty("_tintColor").colorValue = color;
            so.FindProperty("_scale").floatValue = scale;
            so.ApplyModifiedProperties();

            string path = $"Assets/Resources/Enemies/{name}.asset";
            AssetDatabase.CreateAsset(enemyData, path);

            Debug.Log($"Created Enemy Data: {name}");
            return enemyData;
        }

        private static WaveData CreateWaveData(string fileName, string description,
            EnemyData[] enemies, float[] weights, float interval, int enemiesPerSpawn,
            WaveCompletionType completionType, int requiredKills, float duration)
        {
            var waveData = ScriptableObject.CreateInstance<WaveData>();

            var so = new SerializedObject(waveData);
            so.FindProperty("_waveName").stringValue = fileName.Replace("_", " ");
            so.FindProperty("_description").stringValue = description;
            so.FindProperty("_spawnInterval").floatValue = interval;
            so.FindProperty("_enemiesPerSpawn").intValue = enemiesPerSpawn;
            so.FindProperty("_completionType").enumValueIndex = (int)completionType;
            so.FindProperty("_requiredKills").intValue = requiredKills;
            so.FindProperty("_waveDuration").floatValue = duration;

            // Add enemy spawns
            var enemySpawnsProp = so.FindProperty("_enemySpawns");
            enemySpawnsProp.ClearArray();

            for (int i = 0; i < enemies.Length; i++)
            {
                enemySpawnsProp.InsertArrayElementAtIndex(i);
                var spawnElement = enemySpawnsProp.GetArrayElementAtIndex(i);
                spawnElement.FindPropertyRelative("_enemyData").objectReferenceValue = enemies[i];
                spawnElement.FindPropertyRelative("_spawnWeight").floatValue = weights[i];
                spawnElement.FindPropertyRelative("_isEnabled").boolValue = true;
            }

            so.ApplyModifiedProperties();

            string path = $"Assets/Resources/Waves/{fileName}.asset";
            AssetDatabase.CreateAsset(waveData, path);

            Debug.Log($"Created Wave Data: {fileName}");
            return waveData;
        }

        private static LevelConfiguration CreateLevelConfiguration(string fileName, string description,
            WaveData[] waves, float delayBetweenWaves, float offScreenDistance, float despawnDistance)
        {
            var levelConfig = ScriptableObject.CreateInstance<LevelConfiguration>();

            var so = new SerializedObject(levelConfig);
            so.FindProperty("_levelName").stringValue = fileName.Replace("_", " ");
            so.FindProperty("_levelDescription").stringValue = description;
            so.FindProperty("_delayBetweenWaves").floatValue = delayBetweenWaves;
            so.FindProperty("_offScreenSpawnDistance").floatValue = offScreenDistance;
            so.FindProperty("_despawnDistance").floatValue = despawnDistance;

            // Add waves
            var wavesProp = so.FindProperty("_waves");
            wavesProp.ClearArray();

            for (int i = 0; i < waves.Length; i++)
            {
                wavesProp.InsertArrayElementAtIndex(i);
                wavesProp.GetArrayElementAtIndex(i).objectReferenceValue = waves[i];
            }

            so.ApplyModifiedProperties();

            string path = $"Assets/Resources/Levels/{fileName}.asset";
            AssetDatabase.CreateAsset(levelConfig, path);

            Debug.Log($"Created Level Configuration: {fileName}");
            return levelConfig;
        }

        [MenuItem("Game/Quick Setup/Open Wave Editor")]
        public static void OpenWaveEditor()
        {
            WaveEditorWindow.ShowWindow();
        }

        [MenuItem("Game/Quick Setup/Documentation")]
        public static void OpenDocumentation()
        {
            string path = Application.dataPath + "/../COMBAT_SYSTEM_SETUP.md";
            if (System.IO.File.Exists(path))
            {
                Application.OpenURL("file://" + path);
            }
            else
            {
                EditorUtility.DisplayDialog("Documentation Not Found",
                    "COMBAT_SYSTEM_SETUP.md not found in project root.", "OK");
            }
        }
    }
}

