// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("gC65Wrh4xmHDlfq8NlDR9YylL7fhl/zYSe2Fb2aEOOusRnwY0E1Y1lOvVfxqrSSPN2gWrfc0RJK/1p3Z7d7p6Pwb4WhsPdl0oFMZDTnQsUIW9+dRfo+O8lQMVmTQki0pJ40UUUvIxsn5S8jDy0vIyMkD2OrxW/FVlJtJ1nlWpWtkZAZ1aCQFqBnNXf6Bq/HVLsyUE1QGAlmZwvvt0RwQOthFVzkGiZxaKLwrQdROIUfsOTf6/tcEjhHM+Y1VQ74W97wpl7bXTNT5XM9zzXuOtJa0IG3Y01a7EyXlFNSPxfD/C8ljPrWn9yYVjwAfZWEoCw8XEWqp9fI/P9OIdh2tzbQgCtT5S8jr+cTPwONPgU8+xMjIyMzJygKUcJZIaBJkQMvKyMnI");
        private static int[] order = new int[] { 7,2,11,3,12,11,13,11,9,13,13,12,13,13,14 };
        private static int key = 201;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
