using System;
using System.Linq;

namespace NLIIS_Speech_recognition.Services
{
    public static class Distancer
    {
        public static int GetDistance(string original, string modified)
        {
            if (original.Equals(modified))
            {
                return 0;
            }

            var originalLength = original.Length;
            var modifiedLength = modified.Length;
            
            if (originalLength == 0 || modifiedLength == 0)
            {
                return originalLength == 0 ? modifiedLength : originalLength;
            }

            var diffMatrix = new int[originalLength + 1, modifiedLength + 1];

            for (var originalIndex = 1; originalIndex <= originalLength; originalIndex++)
            {
                diffMatrix[originalIndex, 0] = originalIndex;
                
                for (var modifiedIndex = 1; modifiedIndex <= modifiedLength; modifiedIndex++)
                {
                    var cost = modified[modifiedIndex - 1] == original[originalIndex - 1] ? 0 : 1;
                    
                    if (originalIndex == 1)
                    {
                        diffMatrix[0, modifiedIndex] = modifiedIndex;
                    }

                    var values = new[]
                    {
                        diffMatrix[originalIndex - 1, modifiedIndex] + 1,
                        diffMatrix[originalIndex, modifiedIndex - 1] + 1,
                        diffMatrix[originalIndex - 1, modifiedIndex - 1] + cost
                    };
                    diffMatrix[originalIndex,modifiedIndex] = values.Min();
                    
                    if (originalIndex > 1 && modifiedIndex > 1 &&
                        original[originalIndex - 1] == modified[modifiedIndex - 2] &&
                        original[originalIndex - 2] == modified[modifiedIndex - 1])
                    {
                        diffMatrix[originalIndex,modifiedIndex] = Math.Min(
                            diffMatrix[originalIndex,modifiedIndex],
                            diffMatrix[originalIndex - 2, modifiedIndex - 2] + cost);
                    }
                }
            }
            
            return diffMatrix[originalLength, modifiedLength];
        }
    }
}
