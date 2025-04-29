// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("I4YVqRehVG5Mbvq3AgmMYcn/P84OVR8qJdETueRvfS38z1Xaxb+78lr0Y4Biohy7GU8gZuyKCy9Wf/VtzC09i6RVVCiO1oy+Ckj38/1Xzos3BDMyJsE7srbnA656icPX4wprmAKfjePcU0aA8mbxmw6U+5024+0gW3ErD/QWTsmO3NiDQxghNwvGyuDR1c3LsHMvKOXlCVKsx3cXbvrQDk5BkwyjjH+xvr7cr7L+33LDF4ckI5ESMSMeFRo5lVuV5B4SEhIWExAkDd5UyxYjV4+ZZMwtZvNNbA2WDjtNJgKTN1+1vF7iMXacpsIKl4IMiXWPJrB3/lXtssx3Le6eSGUMRwOREhwTI5ESGRGREhIT2QIwK4Erj9hOqkySssi+mhEQEhMS");
        private static int[] order = new int[] { 8,4,7,12,12,9,9,10,13,13,13,13,13,13,14 };
        private static int key = 19;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
