using System;
using System.Collections.Generic;
using System.Text;

namespace MVVM.CoreEditor
{
    public class DuplicateNameFinder
    {
        private const int MaximumDistance = 2;
        private const string IgnoredPrefix = "on";

        public IReadOnlyList<IReadOnlyList<string>> GetDuplicateGroups(IReadOnlyList<string> names)
        {
            if (ReferenceEquals(names, null))
                throw new ArgumentNullException(nameof(names));

            List<IReadOnlyList<string>> duplicateGroups = new List<IReadOnlyList<string>>();
            HashSet<int> groupedIndexes = new HashSet<int>();

            for (int currentIndex = 0; currentIndex < names.Count; currentIndex++)
            {
                if (groupedIndexes.Contains(currentIndex))
                    continue;

                List<string> group = GetGroupForIndex(names, currentIndex, groupedIndexes);

                if (group.Count < 2)
                    continue;

                duplicateGroups.Add(group);
            }

            return duplicateGroups;
        }

        private List<string> GetGroupForIndex(IReadOnlyList<string> names, int currentIndex, HashSet<int> groupedIndexes)
        {
            List<string> group = new List<string> { names[currentIndex] };
            string currentName = Normalize(names[currentIndex]);

            for (int otherIndex = currentIndex + 1; otherIndex < names.Count; otherIndex++)
            {
                if (groupedIndexes.Contains(otherIndex))
                    continue;

                if (GetDistance(currentName, Normalize(names[otherIndex])) > MaximumDistance)
                    continue;

                group.Add(names[otherIndex]);
                groupedIndexes.Add(otherIndex);
            }

            if (group.Count > 1)
                groupedIndexes.Add(currentIndex);

            return group;
        }

        private string Normalize(string name)
        {
            StringBuilder normalized = new StringBuilder();

            foreach (char character in name.ToLowerInvariant())
            {
                if (!char.IsLetterOrDigit(character))
                    continue;

                normalized.Append(character);
            }

            return RemoveIgnoredPrefix(normalized.ToString());
        }

        private string RemoveIgnoredPrefix(string name)
        {
            if (!name.StartsWith(IgnoredPrefix, StringComparison.Ordinal))
                return name;

            return name.Substring(IgnoredPrefix.Length);
        }

        private int GetDistance(string firstName, string secondName)
        {
            int[,] distances = new int[firstName.Length + 1, secondName.Length + 1];

            for (int firstIndex = 0; firstIndex <= firstName.Length; firstIndex++)
                distances[firstIndex, 0] = firstIndex;

            for (int secondIndex = 0; secondIndex <= secondName.Length; secondIndex++)
                distances[0, secondIndex] = secondIndex;

            for (int firstIndex = 1; firstIndex <= firstName.Length; firstIndex++)
            {
                for (int secondIndex = 1; secondIndex <= secondName.Length; secondIndex++)
                {
                    int substitutionCost = GetSubstitutionCost(firstName[firstIndex - 1], secondName[secondIndex - 1]);

                    distances[firstIndex, secondIndex] = Math.Min(
                        Math.Min(distances[firstIndex - 1, secondIndex] + 1, distances[firstIndex, secondIndex - 1] + 1),
                        distances[firstIndex - 1, secondIndex - 1] + substitutionCost);
                }
            }

            return distances[firstName.Length, secondName.Length];
        }

        private int GetSubstitutionCost(char firstCharacter, char secondCharacter)
        {
            if (firstCharacter == secondCharacter)
                return 0;

            return 1;
        }
    }
}
