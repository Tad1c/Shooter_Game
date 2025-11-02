using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Scripts;

namespace Editor {
	public class WaveEditorWindow : EditorWindow {
		private LevelConfiguration _selectedLevel;
		private Vector2 _scrollPosition;
		private Vector2 _timelineScrollPosition;

		private int _selectedWaveIndex = -1;
		private bool _showTimeline = true;
		private bool _showValidation = true;
		private bool _showStats = true;

		private static readonly Color EasyColor = new Color(0.4f, 0.8f, 0.4f);
		private static readonly Color MediumColor = new Color(0.9f, 0.7f, 0.3f);
		private static readonly Color HardColor = new Color(0.9f, 0.3f, 0.3f);

		[MenuItem("Game/Wave Configuration Editor")]
		public static void ShowWindow() {
			var window = GetWindow<WaveEditorWindow>("Wave Editor");
			window.minSize = new Vector2(800, 600);
		}

		private void OnGUI() {
			DrawHeader();

			if (_selectedLevel == null) {
				DrawLevelSelection();
				return;
			}

			_scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);

			DrawLevelInfo();

			if (_showTimeline) {
				DrawTimeline();
			}

			DrawWaveList();

			if (_showStats) {
				DrawLevelStats();
			}

			if (_showValidation) {
				DrawValidation();
			}

			EditorGUILayout.EndScrollView();
		}

		private void DrawHeader() {
			EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);

			if (GUILayout.Button("Select Level", EditorStyles.toolbarButton, GUILayout.Width(100))) {
				_selectedLevel = null;
			}

			GUILayout.FlexibleSpace();

			_showTimeline = GUILayout.Toggle(_showTimeline, "Timeline", EditorStyles.toolbarButton);
			_showStats = GUILayout.Toggle(_showStats, "Statistics", EditorStyles.toolbarButton);
			_showValidation = GUILayout.Toggle(_showValidation, "Validation", EditorStyles.toolbarButton);

			if (_selectedLevel != null) {
				if (GUILayout.Button("Create Wave", EditorStyles.toolbarButton, GUILayout.Width(100))) {
					CreateNewWave();
				}
			}

			EditorGUILayout.EndHorizontal();
		}

		private void DrawLevelSelection() {
			GUILayout.Space(20);

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Level Configuration", EditorStyles.boldLabel);

			var newLevel = (LevelConfiguration) EditorGUILayout.ObjectField(
				"Select Level",
				_selectedLevel,
				typeof(LevelConfiguration),
				false);

			if (newLevel != _selectedLevel) {
				_selectedLevel = newLevel;
				_selectedWaveIndex = -1;
			}

			GUILayout.Space(10);

			if (GUILayout.Button("Create New Level Configuration", GUILayout.Height(30))) {
				CreateNewLevelConfiguration();
			}

			EditorGUILayout.EndVertical();

			GUILayout.Space(20);
			DrawQuickTips();
		}

		private void DrawQuickTips() {
			EditorGUILayout.BeginVertical("helpbox");
			EditorGUILayout.LabelField("Quick Tips:", EditorStyles.boldLabel);
			EditorGUILayout.LabelField("• Create a Level Configuration to get started");
			EditorGUILayout.LabelField("• Use the Timeline to visualize wave progression");
			EditorGUILayout.LabelField("• Drag & drop Wave Data into wave list");
			EditorGUILayout.LabelField("• Check Validation for missing references");
			EditorGUILayout.EndVertical();
		}

		private void DrawLevelInfo() {
			EditorGUILayout.BeginVertical("box");

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField($"Level: {_selectedLevel.LevelName}", EditorStyles.largeLabel);

			if (GUILayout.Button("Edit Asset", GUILayout.Width(100))) {
				Selection.activeObject = _selectedLevel;
				EditorGUIUtility.PingObject(_selectedLevel);
			}
			EditorGUILayout.EndHorizontal();

			if (!string.IsNullOrEmpty(_selectedLevel.LevelDescription)) {
				EditorGUILayout.LabelField(_selectedLevel.LevelDescription, EditorStyles.wordWrappedLabel);
			}

			EditorGUILayout.EndVertical();
		}

		private void DrawTimeline() {
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Wave Timeline", EditorStyles.boldLabel);

			if (_selectedLevel.Waves.Count == 0) {
				EditorGUILayout.HelpBox("No waves configured. Create your first wave!", MessageType.Info);
				EditorGUILayout.EndVertical();
				return;
			}

			_timelineScrollPosition = EditorGUILayout.BeginScrollView(_timelineScrollPosition, GUILayout.Height(120));

			EditorGUILayout.BeginHorizontal();

			for (int i = 0; i < _selectedLevel.Waves.Count; i++) {
				var wave = _selectedLevel.Waves[i];
				if (wave == null) continue;

				DrawTimelineWave(wave, i);
			}

			EditorGUILayout.EndHorizontal();
			EditorGUILayout.EndScrollView();

			EditorGUILayout.EndVertical();
		}

		private void DrawTimelineWave(WaveData wave, int index) {
			Color difficultyColor = GetDifficultyColor(wave);

			EditorGUILayout.BeginVertical("box", GUILayout.Width(150), GUILayout.Height(100));
			
			var originalColor = GUI.backgroundColor;
			GUI.backgroundColor = difficultyColor;

			if (GUILayout.Button($"Wave {index + 1}", GUILayout.Height(25))) {
				_selectedWaveIndex = index;
				Repaint();
			}

			GUI.backgroundColor = originalColor;
			
			EditorGUILayout.LabelField(wave.WaveName, EditorStyles.boldLabel);
			EditorGUILayout.LabelField($"Enemies: {wave.TotalEnemyTypes}");
			EditorGUILayout.LabelField($"Interval: {wave.SpawnInterval}s");

			string completionText = wave.CompletionType switch {
				WaveCompletionType.KillCount => $"Kill {wave.RequiredKills}",
				WaveCompletionType.TimeBased => $"{wave.WaveDuration}s",
				WaveCompletionType.AllEnemiesDead => "Clear all",
				_ => "Manual"
			};
			EditorGUILayout.LabelField(completionText, EditorStyles.miniLabel);

			EditorGUILayout.EndVertical();

			// Draw arrow between waves
			if (index < _selectedLevel.Waves.Count - 1) {
				EditorGUILayout.LabelField("→", GUILayout.Width(20));
			}
		}

		private void DrawWaveList() {
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Wave Configuration", EditorStyles.boldLabel);

			if (_selectedLevel.Waves.Count == 0) {
				EditorGUILayout.HelpBox("No waves in this level. Click 'Create Wave' to add one.", MessageType.Info);
				EditorGUILayout.EndVertical();
				return;
			}

			for (int i = 0; i < _selectedLevel.Waves.Count; i++) {
				DrawWaveEntry(i);
			}

			EditorGUILayout.EndVertical();
		}

		private void DrawWaveEntry(int index) {
			var wave = _selectedLevel.Waves[index];
			bool isSelected = _selectedWaveIndex == index;

			EditorGUILayout.BeginVertical(isSelected ? "selectionRect" : "box");

			EditorGUILayout.BeginHorizontal();
			
			Color difficultyColor = GetDifficultyColor(wave);
			var originalColor = GUI.backgroundColor;
			GUI.backgroundColor = difficultyColor;
			GUILayout.Label("", GUILayout.Width(5), GUILayout.ExpandHeight(true));
			GUI.backgroundColor = originalColor;
			
			EditorGUILayout.BeginVertical();

			EditorGUILayout.BeginHorizontal();
			if (GUILayout.Button(isSelected ? "▼" : "►", GUILayout.Width(25))) {
				_selectedWaveIndex = isSelected ? -1 : index;
			}

			EditorGUILayout.LabelField($"Wave {index + 1}: {(wave != null ? wave.WaveName : "Missing!")}",
				EditorStyles.boldLabel);

			GUILayout.FlexibleSpace();
			
			if (GUILayout.Button("↑", GUILayout.Width(25)) && index > 0) {
				MoveWave(index, index - 1);
			}
			if (GUILayout.Button("↓", GUILayout.Width(25)) && index < _selectedLevel.Waves.Count - 1) {
				MoveWave(index, index + 1);
			}
			if (GUILayout.Button("Duplicate", GUILayout.Width(70))) {
				DuplicateWave(index);
			}
			if (GUILayout.Button("Remove", GUILayout.Width(70))) {
				RemoveWave(index);
			}

			EditorGUILayout.EndHorizontal();

			if (isSelected && wave != null) {
				DrawWaveDetails(wave, index);
			}

			EditorGUILayout.EndVertical();

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();

			GUILayout.Space(5);
		}

		private void DrawWaveDetails(WaveData wave, int index) {
			EditorGUI.indentLevel++;

			GUILayout.Space(10);
			
			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Wave Data Asset:", GUILayout.Width(120));
			var newWave = (WaveData) EditorGUILayout.ObjectField(wave, typeof(WaveData), false);
			if (newWave != wave) {
				Undo.RecordObject(_selectedLevel, "Change Wave Data");
				var waves = new List<WaveData>(_selectedLevel.Waves);
				waves[index] = newWave;
				SerializedObject so = new SerializedObject(_selectedLevel);
				so.FindProperty("_waves").ClearArray();
				so.ApplyModifiedProperties();

				for (int i = 0; i < waves.Count; i++) {
					so.FindProperty("_waves").InsertArrayElementAtIndex(i);
					so.FindProperty("_waves").GetArrayElementAtIndex(i).objectReferenceValue = waves[i];
				}
				so.ApplyModifiedProperties();
				EditorUtility.SetDirty(_selectedLevel);
			}

			if (GUILayout.Button("Edit", GUILayout.Width(60))) {
				Selection.activeObject = wave;
				EditorGUIUtility.PingObject(wave);
			}
			EditorGUILayout.EndHorizontal();
			
			EditorGUILayout.BeginVertical("helpbox");

			EditorGUILayout.LabelField("Spawn Configuration:", EditorStyles.boldLabel);
			EditorGUILayout.LabelField($"  Interval: {wave.SpawnInterval}s");
			EditorGUILayout.LabelField($"  Enemies per spawn: {wave.EnemiesPerSpawn}");

			GUILayout.Space(5);

			EditorGUILayout.LabelField("Enemies:", EditorStyles.boldLabel);
			if (wave.EnemySpawns.Count == 0) {
				EditorGUILayout.HelpBox("No enemies configured!", MessageType.Warning);
			}
			else {
				foreach (var enemySpawn in wave.EnemySpawns) {
					if (enemySpawn.EnemyData == null) {
						EditorGUILayout.LabelField("  ⚠ Missing Enemy Data", EditorStyles.miniLabel);
					}
					else {
						string status = enemySpawn.IsEnabled ? "✓" : "✗";
						EditorGUILayout.LabelField(
							$"  {status} {enemySpawn.EnemyData.EnemyName} (Weight: {enemySpawn.SpawnWeight})",
							EditorStyles.miniLabel);
					}
				}
			}

			GUILayout.Space(5);

			EditorGUILayout.LabelField("Completion:", EditorStyles.boldLabel);
			string completionText = wave.CompletionType switch {
				WaveCompletionType.KillCount => $"  Kill {wave.RequiredKills} enemies",
				WaveCompletionType.TimeBased => $"  Survive {wave.WaveDuration} seconds",
				WaveCompletionType.AllEnemiesDead => "  Eliminate all enemies",
				_ => "  Manual trigger"
			};
			EditorGUILayout.LabelField(completionText);

			EditorGUILayout.EndVertical();

			EditorGUI.indentLevel--;
		}

		private void DrawLevelStats() {
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Level Statistics", EditorStyles.boldLabel);

			if (_selectedLevel.Waves.Count == 0) {
				EditorGUILayout.HelpBox("No statistics available - add waves first.", MessageType.Info);
				EditorGUILayout.EndVertical();
				return;
			}

			int totalWaves = _selectedLevel.Waves.Count(w => w != null);
			int totalEnemyTypes = _selectedLevel.Waves
				.Where(w => w != null)
				.Sum(w => w.TotalEnemyTypes);

			float estimatedDuration = CalculateEstimatedDuration();

			EditorGUILayout.BeginHorizontal();

			DrawStatBox("Total Waves", totalWaves.ToString(), Color.cyan);
			DrawStatBox("Enemy Types", totalEnemyTypes.ToString(), Color.yellow);
			DrawStatBox("Est. Duration", $"{estimatedDuration:F0}s", Color.green);

			EditorGUILayout.EndHorizontal();

			EditorGUILayout.EndVertical();
		}

		private void DrawStatBox(string label, string value, Color color) {
			var originalColor = GUI.backgroundColor;
			GUI.backgroundColor = color;

			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField(label, EditorStyles.miniLabel);
			EditorGUILayout.LabelField(value, EditorStyles.boldLabel);
			EditorGUILayout.EndVertical();

			GUI.backgroundColor = originalColor;
		}

		private void DrawValidation() {
			EditorGUILayout.BeginVertical("box");
			EditorGUILayout.LabelField("Validation", EditorStyles.boldLabel);

			var issues = ValidateLevel();

			if (issues.Count == 0) {
				EditorGUILayout.HelpBox("✓ No issues found!", MessageType.Info);
			}
			else {
				foreach (var issue in issues) {
					EditorGUILayout.HelpBox(issue, MessageType.Warning);
				}
			}

			EditorGUILayout.EndVertical();
		}

		private List<string> ValidateLevel() {
			var issues = new List<string>();

			if (_selectedLevel.Waves.Count == 0) {
				issues.Add("No waves configured in level.");
			}

			for (int i = 0; i < _selectedLevel.Waves.Count; i++) {
				var wave = _selectedLevel.Waves[i];

				if (wave == null) {
					issues.Add($"Wave {i + 1} is missing (null reference).");
					continue;
				}

				if (wave.EnemySpawns.Count == 0) {
					issues.Add($"Wave {i + 1} ({wave.WaveName}) has no enemies configured.");
				}

				bool hasValidEnemy = false;
				foreach (var spawn in wave.EnemySpawns) {
					if (spawn.EnemyData == null) {
						issues.Add($"Wave {i + 1} ({wave.WaveName}) has a missing Enemy Data reference.");
					}
					else if (spawn.IsEnabled) {
						hasValidEnemy = true;

						if (spawn.EnemyData.EnemyPrefab == null) {
							issues.Add($"Wave {i + 1}: Enemy '{spawn.EnemyData.EnemyName}' is missing prefab reference.");
						}
					}
				}

				if (!hasValidEnemy && wave.EnemySpawns.Count > 0) {
					issues.Add($"Wave {i + 1} ({wave.WaveName}) has no enabled enemies.");
				}
			}

			return issues;
		}

		private Color GetDifficultyColor(WaveData wave) {
			if (wave == null) return Color.gray;

			float difficulty = wave.TotalEnemyTypes * (1f / wave.SpawnInterval) * wave.EnemiesPerSpawn;

			if (difficulty < 2f) return EasyColor;
			if (difficulty < 5f) return MediumColor;
			return HardColor;
		}

		private float CalculateEstimatedDuration() {
			float total = 0f;

			foreach (var wave in _selectedLevel.Waves) {
				if (wave == null) continue;

				total += wave.CompletionType switch {
					WaveCompletionType.TimeBased => wave.WaveDuration,
					WaveCompletionType.KillCount => wave.RequiredKills * wave.SpawnInterval / wave.EnemiesPerSpawn,
					_ => 30f
				};

				total += _selectedLevel.DelayBetweenWaves;
			}

			return total;
		}

		private void CreateNewWave() {
			string path = EditorUtility.SaveFilePanelInProject(
				"Create Wave Data",
				"NewWave",
				"asset",
				"Create a new Wave Data asset");

			if (string.IsNullOrEmpty(path)) return;

			var newWave = CreateInstance<WaveData>();
			AssetDatabase.CreateAsset(newWave, path);
			AssetDatabase.SaveAssets();

			Undo.RecordObject(_selectedLevel, "Add Wave");
			SerializedObject so = new SerializedObject(_selectedLevel);
			int newIndex = so.FindProperty("_waves").arraySize;
			so.FindProperty("_waves").InsertArrayElementAtIndex(newIndex);
			so.FindProperty("_waves").GetArrayElementAtIndex(newIndex).objectReferenceValue = newWave;
			so.ApplyModifiedProperties();
			EditorUtility.SetDirty(_selectedLevel);

			_selectedWaveIndex = newIndex;

			Selection.activeObject = newWave;
		}

		private void CreateNewLevelConfiguration() {
			string path = EditorUtility.SaveFilePanelInProject(
				"Create Level Configuration",
				"NewLevel",
				"asset",
				"Create a new Level Configuration asset");

			if (string.IsNullOrEmpty(path)) return;

			var newLevel = CreateInstance<LevelConfiguration>();
			AssetDatabase.CreateAsset(newLevel, path);
			AssetDatabase.SaveAssets();

			_selectedLevel = newLevel;
			Selection.activeObject = newLevel;
		}

		private void MoveWave(int fromIndex, int toIndex) {
			Undo.RecordObject(_selectedLevel, "Move Wave");

			SerializedObject so = new SerializedObject(_selectedLevel);
			var wavesProp = so.FindProperty("_waves");

			var temp = wavesProp.GetArrayElementAtIndex(fromIndex).objectReferenceValue;
			wavesProp.GetArrayElementAtIndex(fromIndex).objectReferenceValue =
				wavesProp.GetArrayElementAtIndex(toIndex).objectReferenceValue;
			wavesProp.GetArrayElementAtIndex(toIndex).objectReferenceValue = temp;

			so.ApplyModifiedProperties();
			EditorUtility.SetDirty(_selectedLevel);

			_selectedWaveIndex = toIndex;
		}

		private void DuplicateWave(int index) {
			var originalWave = _selectedLevel.Waves[index];
			if (originalWave == null) return;

			string path = AssetDatabase.GetAssetPath(originalWave);
			string directory = System.IO.Path.GetDirectoryName(path);
			string fileName = System.IO.Path.GetFileNameWithoutExtension(path);
			string newPath = AssetDatabase.GenerateUniqueAssetPath($"{directory}/{fileName}_Copy.asset");

			AssetDatabase.CopyAsset(path, newPath);
			AssetDatabase.SaveAssets();

			var duplicatedWave = AssetDatabase.LoadAssetAtPath<WaveData>(newPath);

			Undo.RecordObject(_selectedLevel, "Duplicate Wave");
			SerializedObject so = new SerializedObject(_selectedLevel);
			so.FindProperty("_waves").InsertArrayElementAtIndex(index + 1);
			so.FindProperty("_waves").GetArrayElementAtIndex(index + 1).objectReferenceValue = duplicatedWave;
			so.ApplyModifiedProperties();
			EditorUtility.SetDirty(_selectedLevel);
		}

		private void RemoveWave(int index) {
			if (!EditorUtility.DisplayDialog(
				"Remove Wave",
				$"Are you sure you want to remove wave {index + 1}? This will not delete the wave asset itself.",
				"Remove", "Cancel")) {
				return;
			}

			Undo.RecordObject(_selectedLevel, "Remove Wave");
			SerializedObject so = new SerializedObject(_selectedLevel);
			so.FindProperty("_waves").DeleteArrayElementAtIndex(index);
			so.ApplyModifiedProperties();
			EditorUtility.SetDirty(_selectedLevel);

			if (_selectedWaveIndex == index) {
				_selectedWaveIndex = -1;
			}
			else if (_selectedWaveIndex > index) {
				_selectedWaveIndex--;
			}
		}
	}
}
