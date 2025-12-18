using UnityEditor;
using System.Linq;

namespace UGESystem
{
    /// <summary>
    /// An asset postprocessor that detects changes to all <c>.asset</c> files and
    /// refreshes the cache of <see cref="GameEventGuidManager"/> to keep it up-to-date.
    /// </summary>
    public class GameEventAssetPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// Callback method that is invoked after any assets are imported, deleted, or moved.
        /// It triggers a refresh of the <see cref="GameEventGuidManager"/> cache if any <c>.asset</c> files were involved in the changes.
        /// </summary>
        /// <param name="importedAssets">Paths of all assets that were imported.</param>
        /// <param name="deletedAssets">Paths of all assets that were deleted.</param>
        /// <param name="movedAssets">Paths of all assets that were moved.</param>
        /// <param name="movedFromAssetPaths">Original paths of all assets that were moved.</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Check if any of the changed assets are GameEvent assets.
            // A simple check for any .asset file change is sufficient and robust.
            bool needsRefresh = importedAssets.Any(path => path.EndsWith(".asset")) ||
                                deletedAssets.Any(path => path.EndsWith(".asset")) ||
                                movedAssets.Any(path => path.EndsWith(".asset"));

            if (needsRefresh)
            {
                // We call the refresh method with a delay to ensure the asset database is fully updated.
                EditorApplication.delayCall += GameEventGuidManager.Refresh;
            }
        }
    }
}