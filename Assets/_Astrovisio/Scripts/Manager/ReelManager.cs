using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Astrovisio
{
    public class ReelManager : MonoBehaviour
    {
        public static ReelManager Instance { get; private set; }

        [Header("Dependencies")]
        [SerializeField] private ProjectManager projectManager;

        // Processed cache: quick lookup by (projectId, fileId) → DataContainer
        private readonly Dictionary<ProjectFile, DataContainer> processedMap = new();

        // Reels per project: only processed files, ordered (as on server)
        private readonly Dictionary<int, ProjectReel> reelByProject = new();

        // Cursor per project: current index inside the reel (for prev/next)
        private readonly Dictionary<int, int> cursorByProject = new();


        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("Multiple instances of ReelManager found. Destroying the new one.");
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            projectManager.FileUpdated += OnFileUpdated;
            projectManager.FileProcessed += OnFileProcessed;
            projectManager.ProjectUpdated += OnProjectUpdated;
            projectManager.ProjectClosed += OnProjectClosed;
        }

        private void OnDestroy()
        {
            projectManager.FileUpdated -= OnFileUpdated;
            projectManager.FileProcessed -= OnFileProcessed;
            projectManager.ProjectUpdated -= OnProjectUpdated;
            projectManager.ProjectClosed -= OnProjectClosed;
        }

        private static string JoinIds(IEnumerable<int> ids)
        {
            return ids == null ? "∅" : string.Join(",", ids);
        }

        private int CountProcessedForProject(int projectId)
        {
            int count = 0;
            foreach (var k in processedMap.Keys)
            {
                if (k.ProjectId == projectId)
                {
                    count++;
                }
            }
            return count;
        }

        private string DumpReelStateLine(int projectId)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel) || reel == null)
            {
                return $"P{projectId} reel: ∅";
            }

            var ids = reel.OrderedIds;
            int count = ids.Count;
            int cursor = cursorByProject.TryGetValue(projectId, out var c) ? c : 0;
            return $"P{projectId} reel-count={count} cursor={cursor} order=[{JoinIds(ids)}]";
        }

        // ===== Key helpers =====
        private static ProjectFile Key(int projectId, int fileId)
        {
            return new ProjectFile(projectId, fileId);
        }

        private ProjectReel GetOrCreateReel(int projectId)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel))
            {
                reel = new ProjectReel();
                reelByProject[projectId] = reel;
                cursorByProject[projectId] = 0;
                // Debug.Log($"[ReelManager] Create reel for P{projectId}. {DumpReelStateLine(projectId)}");
            }

            return reel;
        }

        private void EnsureCursorInRange(int projectId)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel))
            {
                cursorByProject.Remove(projectId);
                Debug.Log($"[ReelManager] EnsureCursorInRange: reel missing for P{projectId} → remove cursor.");
                return;
            }

            int before = cursorByProject.TryGetValue(projectId, out var c0) ? c0 : 0;
            int count = reel?.Enumerate()?.Count() ?? 0;
            if (count <= 0)
            {
                cursorByProject[projectId] = 0;
                Debug.Log($"[ReelManager] EnsureCursorInRange: empty reel for P{projectId} → cursor=0.");
                return;
            }

            int after = Mathf.Clamp(before, 0, count - 1);
            if (before != after)
            {
                Debug.Log($"[ReelManager] EnsureCursorInRange: P{projectId} cursor {before}→{after} (count={count}).");
            }

            cursorByProject[projectId] = after;
        }

        /// <summary>
        /// Aligns the project's reel to the server order:
        /// uses Project.Files.Order and includes only entries present in the processed cache.
        /// </summary>
        private void SyncReelWithServerOrder(Project project)
        {
            if (project == null)
            {
                return;
            }

            var reel = GetOrCreateReel(project.Id);

            var orderedProcessedIds = (project.Files ?? new List<File>())
                .Where(f => f != null && f.Processed && processedMap.ContainsKey(Key(project.Id, f.Id)))
                .OrderBy(f => f.Order)
                .Select(f => f.Id)
                .ToArray();

            // Debug.Log($"[ReelManager] Sync start P{project.Id}: serverProcessedOrder=[{JoinIds(orderedProcessedIds)}] processedCount={CountProcessedForProject(project.Id)}");

            reel.SetOrder(orderedProcessedIds);

            var validSet = new HashSet<int>(orderedProcessedIds);
            var toRemove = reel.Enumerate()
                .Where(e => !validSet.Contains(e.FileId))
                .Select(e => e.FileId)
                .ToList();

            foreach (var rid in toRemove)
            {
                reel.Remove(rid);
                Debug.Log($"[ReelManager] Sync: removed not-processed-anymore P{project.Id} F{rid}");
            }

            EnsureCursorInRange(project.Id);
            // Debug.Log($"[ReelManager] Sync end - {DumpReelStateLine(project.Id)}");
        }

        // ===== Public API: processed cache (for RenderManager if needed) =====
        public bool TryGetDataContainer(int projectId, int fileId, out DataContainer dc)
        {
            return processedMap.TryGetValue(Key(projectId, fileId), out dc);
        }


        // ===== Public API: reels =====
        public bool TryGetReel(int projectId, out ProjectReel reel)
        {
            return reelByProject.TryGetValue(projectId, out reel);
        }

        public IReadOnlyList<int> GetReelOrderedIds(int projectId)
        {
            return reelByProject.TryGetValue(projectId, out var reel)
                ? reel.OrderedIds
                : Array.Empty<int>();
        }

        public void SetReelOrder(int projectId, IReadOnlyList<int> orderedIds)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel) || reel == null)
            {
                return;
            }

            Debug.Log($"[ReelManager] SetReelOrder P{projectId} → [{JoinIds(orderedIds)}]");
            reel.SetOrder(orderedIds);
            EnsureCursorInRange(projectId);
            Debug.Log($"[ReelManager] After SetReelOrder {DumpReelStateLine(projectId)}");
        }

        public bool RemoveFromReel(int projectId, int fileId)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel) || reel == null)
            {
                return false;
            }

            bool removed = reel.Remove(fileId);
            if (removed)
            {
                Debug.Log($"[ReelManager] RemoveFromReel P{projectId} F{fileId} → removed. {DumpReelStateLine(projectId)}");
                EnsureCursorInRange(projectId);
            }
            else
            {
                Debug.Log($"[ReelManager] RemoveFromReel P{projectId} F{fileId} → not present.");
            }

            return removed;
        }

        public void ClearReel(int projectId)
        {
            if (reelByProject.TryGetValue(projectId, out var reel) && reel != null)
            {
                reel.Clear();
            }

            cursorByProject[projectId] = 0;
            Debug.Log($"[ReelManager] ClearReel P{projectId}. {DumpReelStateLine(projectId)}");
        }

        public void RemoveReel(int projectId)
        {
            reelByProject.Remove(projectId);
            cursorByProject.Remove(projectId);
            // Debug.Log($"[ReelManager] RemoveReel P{projectId} (reel + cursor removed).");
        }

        public int? GetReelCurrentFileId(int projectId)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel) || reel == null)
            {
                return null;
            }

            int count = reel.Enumerate().Count();
            if (count == 0)
            {
                return null;
            }

            int cursor = cursorByProject.TryGetValue(projectId, out var c) ? c : 0;
            cursor = Mathf.Clamp(cursor, 0, count - 1);

            return reel.GetAt(cursor).FileId;
        }

        public DataContainer GetReelCurrentDataContainer(int projectId)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel) || reel == null)
            {
                return null;
            }

            int count = reel.Enumerate().Count();
            if (count == 0)
            {
                return null;
            }

            int cursor = cursorByProject.TryGetValue(projectId, out var c) ? c : 0;
            cursor = Mathf.Clamp(cursor, 0, count - 1);

            return reel.GetAt(cursor).Data;
        }

        public int MoveNext(int projectId)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel) || reel == null)
            {
                Debug.LogWarning($"[ReelManager] MoveNext: reel missing for P{projectId}");
                return -1;
            }

            int count = reel.Enumerate().Count();
            if (count == 0)
            {
                Debug.LogWarning($"[ReelManager] MoveNext: empty reel for P{projectId}");
                return -1;
            }

            int cursor = cursorByProject.TryGetValue(projectId, out var c) ? c : 0;
            int next = (cursor + 1) % count;
            cursorByProject[projectId] = next;
            var current = reel.GetAt(next);

            string name = current.Data?.File?.Name ?? $"F{current.FileId}";
            Debug.Log($"[ReelManager] MoveNext P{projectId} {cursor}→{next} (count={count}) → {name} (F{current.FileId})");

            return current.FileId;
        }

        public int MovePrev(int projectId)
        {
            if (!reelByProject.TryGetValue(projectId, out var reel) || reel == null)
            {
                Debug.LogWarning($"[ReelManager] MovePrev: reel missing for P{projectId}");
                return -1;
            }

            int count = reel.Enumerate().Count();
            if (count == 0)
            {
                Debug.LogWarning($"[ReelManager] MovePrev: empty reel for P{projectId}");
                return -1;
            }

            int cursor = cursorByProject.TryGetValue(projectId, out var c) ? c : 0;
            int prev = (cursor - 1 + count) % count;
            cursorByProject[projectId] = prev;
            var current = reel.GetAt(prev);

            string name = current.Data?.File?.Name ?? $"F{current.FileId}";
            Debug.Log($"[ReelManager] MovePrev P{projectId} {cursor}→{prev} (count={count}) → {name} (F{current.FileId})");

            return current.FileId;
        }


        // ===== Processed add/remove & project lifecycle (driven by ProjectManager events) =====
        private void OnFileProcessed(Project project, File file, DataPack pack)
        {
            if (project == null || file == null || pack == null)
            {
                Debug.LogError("[ReelManager] OnFileProcessed: null args.");
                return;
            }

            // Build DataContainer and register in processed cache
            DataContainer dataContainer = new DataContainer(pack, project, file);
            processedMap[Key(project.Id, file.Id)] = dataContainer;

            // Debug.Log($"[ReelManager] Processed ADD P{project.Id} {project.Name} | F{file.Id} {file.Name} | processedCount={CountProcessedForProject(project.Id)}");

            // Add to reel and sync with server order
            var reel = GetOrCreateReel(project.Id);
            reel.AddOrUpdate(file.Id, dataContainer);
            // Debug.Log($"[ReelManager] Reel AddOrUpdate P{project.Id} F{file.Id}. {DumpReelStateLine(project.Id)}");

            SyncReelWithServerOrder(project);
        }

        private void OnFileUpdated(Project project, File file)
        {
            if (project == null || file == null)
            {
                // Debug.LogError("[ReelManager] OnFileUpdated: null project or file.");
                return;
            }

            // Debug.Log($"[ReelManager] OnFileUpdated P{project.Id} {project.Name} | F{file.Id} {file.Name} → remove processed & reel entry");

            bool removedProcessed = processedMap.Remove(Key(project.Id, file.Id));
            if (removedProcessed)
            {
                // Debug.Log($"[ReelManager] Processed REMOVE P{project.Id} F{file.Id} | processedCount={CountProcessedForProject(project.Id)}");
            }
            else
            {
                // Debug.Log($"[ReelManager] Processed REMOVE skipped (not found) P{project.Id} F{file.Id}");
            }

            if (reelByProject.TryGetValue(project.Id, out var reel) && reel != null)
            {
                bool removedReel = reel.Remove(file.Id);
                if (removedReel)
                {
                    // Debug.Log($"[ReelManager] Reel REMOVE P{project.Id} F{file.Id}. {DumpReelStateLine(project.Id)}");
                    EnsureCursorInRange(project.Id);
                }
            }
        }

        private void OnProjectUpdated(Project project)
        {
            if (project == null)
            {
                return;
            }

            Debug.Log($"[ReelManager] OnProjectUpdated P{project.Id} {project.Name} → sync reel with server order.");
            SyncReelWithServerOrder(project);
        }

        private void OnProjectClosed(Project project)
        {
            if (project == null)
            {
                return;
            }

            // Remove processed cache for this project
            var toRemove = new List<ProjectFile>();
            foreach (var k in processedMap.Keys)
            {
                if (k.ProjectId == project.Id)
                {
                    toRemove.Add(k);
                }
            }

            foreach (var k in toRemove)
            {
                processedMap.Remove(k);
            }

            Debug.Log($"[ReelManager] Processed CLEAR project P{project.Id} removed={toRemove.Count}");

            // Remove reel + cursor
            RemoveReel(project.Id);
        }

        // ===== Bulk clear (all projects) =====
        public void ClearAll()
        {
            processedMap.Clear();
            reelByProject.Clear();
            cursorByProject.Clear();
            Debug.Log("[ReelManager] ClearAll: processed + reels + cursors cleared.");
        }

    }

}
