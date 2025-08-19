// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("wGX2SvRCt42vjRlU4epvgioc3C3H7j23KPXAtGx6hy/OhRCuj+517e22/MnGMvBaB4yezh8stjkmXFgR2K7F4XDUvFZfvQHSlX9FIel0Ye+5F4BjgUH/WPqsw4UPaejMtZwWjjI2LihTkMzLBgbqsU8klPSNGTPtL87eaEe2t8ttNW9d6asUEB60LWjAcvHSwP32+dp2uHYH/fHx8fXw862icO9Ab5xSXV0/TFEdPJEg9GTH1OfQ0cUi2FFVBOBNmWogNADpiHvhfG4AP7ClYxGFEnjtdxh+1QAOw2qWbMVTlB22DlEvlM4NfauG76TguJLI7Bf1rSptPztgoPvC1OglKQNy8f/wwHLx+vJy8fHwOuHTyGLIbDutSa9xUStdefLz8fDx");
        private static int[] order = new int[] { 8,6,4,4,7,10,12,8,13,13,13,11,13,13,14 };
        private static int key = 240;

        public static readonly bool IsPopulated = true;

        public static byte[] Data() {
        	if (IsPopulated == false)
        		return null;
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
