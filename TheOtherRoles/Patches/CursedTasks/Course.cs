namespace TheOtherRoles.Patches.CursedTasks
{
    internal class Course
    {
        [HarmonyPatch(typeof(CourseMinigame))]
        private static class CourseMinigamePatch
        {
            [HarmonyPatch(nameof(CourseMinigame.Begin)), HarmonyPrefix]
            private static void BeginPrefix(CourseMinigame __instance)
            {
                if (!ModOption.CursedTasks) return;
                var rd = new System.Random();
                __instance.NumPoints = rd.Next(20, 27);
            }
        }
    }
}
